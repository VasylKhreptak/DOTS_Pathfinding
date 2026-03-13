using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Entities.Authoring.Pathfinding;
using Entities.Bakers.Pathfinding;
using Entities.Components.Pathfinding;
using Plugins.Extensions;
using Unity.AI.Navigation;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;
using BoxCollider = Unity.Physics.BoxCollider;
using CapsuleCollider = Unity.Physics.CapsuleCollider;
using Collider = Unity.Physics.Collider;
using NavMeshModifier = Entities.Bakers.Pathfinding.NavMeshModifier;
using NavMeshModifierVolume = Entities.Bakers.Pathfinding.NavMeshModifierVolume;
using SphereCollider = Unity.Physics.SphereCollider;

namespace Entities.Systems.Pathdinding
{
    [DisableAutoCreation]
    public partial class NavMeshBakeSystem : SystemBase
    {
        private const float BakeInterval = 2f;
        private const float Range = 100f;
        private const int InitialSourcesBufferSize = 1024 * 2;
        private const int NavMeshSourceConversionBatchCount = 64;

        private readonly List<NavMeshData> _navMeshDataBuffer = new List<NavMeshData>();
        private readonly List<NavMeshBuildSource> _sourcesBuffer = new List<NavMeshBuildSource>();
        private NativeList<BurstedNavMeshBuildSource> _sourcesNativeBuffer;

        private ComponentLookup<NavMeshModifier> _navMeshModifierLookup;
        private ComponentLookup<Parent> _parentLookup;
        private BufferLookup<AffectedAgentElement> _affectedAgentBufferLookup;
        private ComponentLookup<MeshColliderMeshReference> _meshColliderMeshReferenceLookup;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private bool _isBaking;
        private float _lastCompletedBakeTime;

        protected override void OnCreate()
        {
            _sourcesNativeBuffer = new NativeList<BurstedNavMeshBuildSource>(InitialSourcesBufferSize, Allocator.Persistent);

            _navMeshModifierLookup = GetComponentLookup<NavMeshModifier>(true);
            _parentLookup = GetComponentLookup<Parent>(true);
            _affectedAgentBufferLookup = GetBufferLookup<AffectedAgentElement>(true);
            _meshColliderMeshReferenceLookup = GetComponentLookup<MeshColliderMeshReference>(true);

            RequireForUpdate<NavMeshBakeCenterTag>();
        }

        protected override void OnUpdate()
        {
            if (NavMeshSurface.activeSurfaces.Count == 0)
                return;

            if (_isBaking)
                return;

            if ((float)SystemAPI.Time.ElapsedTime < _lastCompletedBakeTime + BakeInterval)
                return;

            BakeNavMeshes(_cts.Token).Forget();
        }

        protected override void OnDestroy()
        {
            _sourcesNativeBuffer.Dispose();
            _cts.Cancel();
        }

        private async UniTask BakeNavMeshes(CancellationToken token)
        {
            _isBaking = true;

            Entity bakeCenterEntity = SystemAPI.GetSingletonEntity<NavMeshBakeCenterTag>();
            LocalToWorld localToWorld = EntityManager.GetComponentData<LocalToWorld>(bakeCenterEntity);

            Bounds bounds = new Bounds(localToWorld.Position, Vector3.one * Range * 2f);

            AssignNavMeshData();

            foreach (NavMeshSurface navMeshSurface in NavMeshSurface.activeSurfaces.ToList())
            {
                NavMeshBuildSettings settings = navMeshSurface.GetBuildSettings();

                await CollectSourcesAsync(bounds, navMeshSurface.layerMask, navMeshSurface.useGeometry, false, settings.agentTypeID, navMeshSurface.defaultArea,
                    _sourcesBuffer, token);

                await NavMeshBuilder.UpdateNavMeshDataAsync(navMeshSurface.navMeshData, settings, _sourcesBuffer, bounds).ToUniTask(cancellationToken: token);
            }

            _isBaking = false;
            _lastCompletedBakeTime = (float)SystemAPI.Time.ElapsedTime;
        }

        private void AssignNavMeshData()
        {
            foreach (NavMeshSurface navMeshSurface in NavMeshSurface.activeSurfaces.ToList())
            {
                if (_navMeshDataBuffer.Contains(navMeshSurface.navMeshData) == false)
                {
                    NavMeshData navMeshData = new NavMeshData();

                    navMeshSurface.navMeshData = navMeshData;

                    _navMeshDataBuffer.Add(navMeshData);
                }
            }

            _navMeshDataBuffer.RemoveAll(navMeshData => NavMeshSurface.activeSurfaces.Any(surface => surface.navMeshData == navMeshData) == false);
        }

        private async UniTask CollectSourcesAsync(Bounds bounds, LayerMask layerMask, NavMeshCollectGeometry geometry, bool generateLinks, int agentID, int defaultArea,
            List<NavMeshBuildSource> sources, CancellationToken token)
        {
            sources.Clear();
            _sourcesNativeBuffer.Clear();

            EnsureSourceBufferCapacity(geometry);

            _navMeshModifierLookup.Update(this);
            _parentLookup.Update(this);
            _affectedAgentBufferLookup.Update(this);
            _meshColliderMeshReferenceLookup.Update(this);

            JobHandle collectSourcesJobHandle;

            if (geometry == NavMeshCollectGeometry.PhysicsColliders)
            {
                CollectPhysicSourcesJob collectPhysicSourcesJob = new CollectPhysicSourcesJob
                {
                    Bounds = new Aabb() { Min = bounds.min, Max = bounds.max },
                    LayerMaskValue = layerMask.value,
                    GenerateLinks = generateLinks,
                    AgentID = agentID,
                    DefaultArea = defaultArea,
                    NavMeshModifierLookup = _navMeshModifierLookup,
                    ParentLookup = _parentLookup,
                    AffectedAgentBufferLookup = _affectedAgentBufferLookup,
                    MeshColliderMeshReferenceLookup = _meshColliderMeshReferenceLookup,
                    Sources = _sourcesNativeBuffer.AsParallelWriter()
                };

                collectSourcesJobHandle = collectPhysicSourcesJob.ScheduleParallel(Dependency);
            }
            else
            {
                CollectMeshSourcesJob collectMeshSourcesJob = new CollectMeshSourcesJob
                {
                    Bounds = new AABB { Center = bounds.center, Extents = bounds.extents },
                    LayerMaskValue = layerMask.value,
                    GenerateLinks = generateLinks,
                    AgentID = agentID,
                    DefaultArea = defaultArea,
                    NavMeshModifierLookup = _navMeshModifierLookup,
                    ParentLookup = _parentLookup,
                    AffectedAgentBufferLookup = _affectedAgentBufferLookup,
                    Sources = _sourcesNativeBuffer.AsParallelWriter()
                };

                collectSourcesJobHandle = collectMeshSourcesJob.ScheduleParallel(Dependency);
            }

            CollectNavMeshModifierVolumeSourcesJob collectNavMeshModifierVolumeSourcesJob = new CollectNavMeshModifierVolumeSourcesJob
            {
                Bounds = new AABB { Center = bounds.center, Extents = bounds.extents },
                AgentID = agentID,
                Sources = _sourcesNativeBuffer.AsParallelWriter()
            };

            Dependency = collectNavMeshModifierVolumeSourcesJob.ScheduleParallel(collectSourcesJobHandle);

            Dependency.Complete();

            await UniTask.Yield(token);

            NavMeshBuildSource source = default;

            for (int i = 0; i < _sourcesNativeBuffer.Length; i++)
            {
                BurstedNavMeshBuildSource burstedSource = _sourcesNativeBuffer[i];

                source.transform = burstedSource.TransformMatrix;
                source.size = burstedSource.Size;
                source.shape = burstedSource.Shape;
                source.area = burstedSource.Area;
                source.sourceObject = burstedSource.MeshReference.Value;
                source.generateLinks = burstedSource.GenerateLinks;

                sources.Add(source);

                if (i % NavMeshSourceConversionBatchCount == 0)
                    await UniTask.Yield(token);
            }
        }

        private void EnsureSourceBufferCapacity(NavMeshCollectGeometry geometry)
        {
            int targetCapacity;

            int modifierVolumesCount = SystemAPI.QueryBuilder().WithAll<NavMeshModifierVolume>().Build().CalculateEntityCount();

            if (geometry == NavMeshCollectGeometry.PhysicsColliders)
            {
                int collidersCount = SystemAPI.QueryBuilder().WithAll<PhysicsCollider>().Build().CalculateEntityCount();

                targetCapacity = math.max(collidersCount, modifierVolumesCount);
            }
            else
            {
                int meshesCount = SystemAPI.QueryBuilder().WithAll<MeshRendererMeshReference>().Build().CalculateEntityCount();

                targetCapacity = math.max(meshesCount, modifierVolumesCount);
            }

            if (_sourcesNativeBuffer.Capacity < targetCapacity)
                _sourcesNativeBuffer.SetCapacity(targetCapacity);
        }

        [BurstCompile]
        public partial struct CollectPhysicSourcesJob : IJobEntity
        {
            public Aabb Bounds;
            public int LayerMaskValue;
            public bool GenerateLinks;
            public int AgentID;
            public int DefaultArea;

            [ReadOnly] public ComponentLookup<NavMeshModifier> NavMeshModifierLookup;
            [ReadOnly] public ComponentLookup<Parent> ParentLookup;
            [ReadOnly] public BufferLookup<AffectedAgentElement> AffectedAgentBufferLookup;
            [ReadOnly] public ComponentLookup<MeshColliderMeshReference> MeshColliderMeshReferenceLookup;

            public NativeList<BurstedNavMeshBuildSource>.ParallelWriter Sources;

            public void Execute(ref LocalToWorld ltw, ref PhysicsCollider physicsCollider, Entity entity)
            {
                if (physicsCollider.IsValid == false)
                    return;

                CollisionFilter filter = physicsCollider.Value.Value.GetCollisionFilter();

                if ((filter.BelongsTo & LayerMaskValue) == 0)
                    return;

                unsafe
                {
                    Collider* collider = physicsCollider.ColliderPtr;

                    RigidTransform worldTransform = new RigidTransform(ltw.Rotation, ltw.Position);

                    Aabb colliderAabb = collider->CalculateAabb(worldTransform);

                    if (Bounds.Contains(colliderAabb) == false && Bounds.Overlaps(colliderAabb) == false)
                        return;
                }

                BurstedNavMeshBuildSource source = new BurstedNavMeshBuildSource
                {
                    Area = DefaultArea,
                    GenerateLinks = GenerateLinks
                };

                if (EntityManagerExtensions.HasComponentInParent(entity, ref NavMeshModifierLookup, ref ParentLookup, out Entity componentEntity))
                {
                    NavMeshModifier navMeshModifier = NavMeshModifierLookup[componentEntity];

                    if (componentEntity != entity && navMeshModifier.ApplyToChildren == false)
                        return;

                    DynamicBuffer<AffectedAgentElement> affectedAgents = AffectedAgentBufferLookup[componentEntity];

                    bool containsTargetAgent = false;

                    foreach (AffectedAgentElement affectedAgentElement in affectedAgents)
                    {
                        if (affectedAgentElement.ID == AgentID)
                        {
                            containsTargetAgent = true;
                            break;
                        }
                    }

                    if (containsTargetAgent)
                    {
                        if (navMeshModifier.Mode == NavMeshModifierMode.RemoveObject)
                            return;

                        if (navMeshModifier.OverrideArea)
                            source.Area = navMeshModifier.Area;

                        if (navMeshModifier.OverrideGenerateLinks)
                            source.GenerateLinks = navMeshModifier.GenerateLinks;
                    }
                }

                unsafe
                {
                    Collider* collider = physicsCollider.ColliderPtr;

                    switch (collider->Type)
                    {
                        case ColliderType.Box:
                        {
                            BoxCollider boxCollider = *(BoxCollider*)collider;

                            float4x4 transform = float4x4.TRS(ltw.Position, ltw.Rotation, new float3(1));

                            float3 centerOffset = boxCollider.Center;

                            if (ltw.Value.HasNonUniformScale() == false)
                                centerOffset *= ltw.Value.Scale();

                            transform = math.mul(transform, float4x4.Translate(centerOffset));

                            source.Shape = NavMeshBuildSourceShape.Box;
                            source.TransformMatrix = transform;
                            source.Size = boxCollider.Size;

                            if (ltw.Value.HasNonUniformScale() == false)
                                source.Size *= ltw.Value.Scale();

                            break;
                        }
                        case ColliderType.Sphere:
                        {
                            SphereCollider sphereCollider = *(SphereCollider*)collider;

                            float4x4 transform = float4x4.TRS(ltw.Position, ltw.Rotation, new float3(1));

                            float3 centerOffset = sphereCollider.Center;

                            if (ltw.Value.HasNonUniformScale() == false)
                                centerOffset *= ltw.Value.Scale();

                            transform = math.mul(transform, float4x4.Translate(centerOffset));

                            source.Shape = NavMeshBuildSourceShape.Sphere;
                            source.TransformMatrix = transform;
                            source.Size = new float3(sphereCollider.Radius * 2);

                            if (ltw.Value.HasNonUniformScale() == false)
                                source.Size *= ltw.Value.Scale();

                            break;
                        }
                        case ColliderType.Capsule:
                        {
                            CapsuleCollider capsuleCollider = *(CapsuleCollider*)collider;

                            float4x4 transform = float4x4.TRS(ltw.Position, ltw.Rotation, new float3(1));

                            float3 centerOffset = capsuleCollider.Geometry.GetCenter();

                            if (ltw.Value.HasNonUniformScale() == false)
                                centerOffset *= ltw.Value.Scale();

                            transform = math.mul(transform, float4x4.Translate(centerOffset));

                            source.Shape = NavMeshBuildSourceShape.Capsule;
                            source.TransformMatrix = transform;
                            float height = math.distance(capsuleCollider.Geometry.Vertex0, capsuleCollider.Geometry.Vertex1) + capsuleCollider.Radius * 2;
                            float width = capsuleCollider.Radius * 2;
                            source.Size = new float3(width, height, width);

                            if (ltw.Value.HasNonUniformScale() == false)
                                source.Size *= ltw.Value.Scale();

                            break;
                        }
                        case ColliderType.Mesh:
                        {
                            if (MeshColliderMeshReferenceLookup.HasComponent(entity))
                            {
                                MeshColliderMeshReference meshColliderMeshReference = MeshColliderMeshReferenceLookup[entity];

                                source.Shape = NavMeshBuildSourceShape.Mesh;
                                source.TransformMatrix = ltw.Value;
                                source.MeshReference = meshColliderMeshReference.Value;
                            }

                            break;
                        }
                        default:
                            return;
                    }
                }

                Sources.AddNoResize(source);
            }
        }

        [BurstCompile]
        public partial struct CollectMeshSourcesJob : IJobEntity
        {
            public AABB Bounds;
            public int LayerMaskValue;
            public bool GenerateLinks;
            public int AgentID;
            public int DefaultArea;

            [ReadOnly] public ComponentLookup<NavMeshModifier> NavMeshModifierLookup;
            [ReadOnly] public ComponentLookup<Parent> ParentLookup;
            [ReadOnly] public BufferLookup<AffectedAgentElement> AffectedAgentBufferLookup;

            public NativeList<BurstedNavMeshBuildSource>.ParallelWriter Sources;

            public void Execute(ref LocalToWorld ltw, ref MeshRendererMeshReference meshRendererMeshReference, ref WorldRenderBounds worldRenderBounds,
                RenderFilterSettings renderFilterSettings, Entity entity)
            {
                if (((1 << renderFilterSettings.Layer) & (uint)LayerMaskValue) == 0)
                    return;

                if (Bounds.Contains(worldRenderBounds.Value) == false && Bounds.Overlaps(worldRenderBounds.Value) == false)
                    return;

                BurstedNavMeshBuildSource source = new BurstedNavMeshBuildSource
                {
                    Area = DefaultArea,
                    GenerateLinks = GenerateLinks,
                    Shape = NavMeshBuildSourceShape.Mesh,
                    TransformMatrix = ltw.Value,
                    MeshReference = meshRendererMeshReference.Value
                };

                if (EntityManagerExtensions.HasComponentInParent(entity, ref NavMeshModifierLookup, ref ParentLookup, out Entity componentEntity))
                {
                    NavMeshModifier navMeshModifier = NavMeshModifierLookup[componentEntity];

                    if (componentEntity != entity && navMeshModifier.ApplyToChildren == false)
                        return;

                    DynamicBuffer<AffectedAgentElement> affectedAgents = AffectedAgentBufferLookup[componentEntity];

                    bool containsTargetAgent = false;

                    foreach (AffectedAgentElement affectedAgentElement in affectedAgents)
                    {
                        if (affectedAgentElement.ID == AgentID)
                        {
                            containsTargetAgent = true;
                            break;
                        }
                    }

                    if (containsTargetAgent)
                    {
                        if (navMeshModifier.Mode == NavMeshModifierMode.RemoveObject)
                            return;

                        if (navMeshModifier.OverrideArea)
                            source.Area = navMeshModifier.Area;

                        if (navMeshModifier.OverrideGenerateLinks)
                            source.GenerateLinks = navMeshModifier.GenerateLinks;
                    }
                }

                Sources.AddNoResize(source);
            }
        }

        [BurstCompile]
        public partial struct CollectNavMeshModifierVolumeSourcesJob : IJobEntity
        {
            public AABB Bounds;
            public int AgentID;

            public NativeList<BurstedNavMeshBuildSource>.ParallelWriter Sources;

            public void Execute(ref LocalToWorld ltw, ref NavMeshModifierVolume navMeshModifierVolume, DynamicBuffer<AffectedAgentElement> affectedAgents)
            {
                AABB localBounds = new AABB { Center = navMeshModifierVolume.Center, Extents = navMeshModifierVolume.Size / 2f };
                AABB worldBounds = localBounds.ToWorld(ltw.Value);

                if (Bounds.Contains(worldBounds) == false && Bounds.Overlaps(worldBounds) == false)
                    return;

                bool containsTargetAgent = false;

                foreach (AffectedAgentElement affectedAgentElement in affectedAgents)
                {
                    if (affectedAgentElement.ID == AgentID)
                    {
                        containsTargetAgent = true;
                        break;
                    }
                }

                if (containsTargetAgent == false)
                    return;

                float4x4 transform = float4x4.TRS(ltw.Position, ltw.Rotation, new float3(1));
                transform = math.mul(transform, float4x4.Translate(navMeshModifierVolume.Center * ltw.Value.Scale()));

                BurstedNavMeshBuildSource source = new BurstedNavMeshBuildSource
                {
                    Area = navMeshModifierVolume.AreaType,
                    Shape = NavMeshBuildSourceShape.ModifierBox,
                    TransformMatrix = transform,
                    Size = navMeshModifierVolume.Size * ltw.Value.Scale(),
                };

                Sources.AddNoResize(source);
            }
        }
    }
}