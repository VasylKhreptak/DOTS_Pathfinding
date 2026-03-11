using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Components;
using Cysharp.Threading.Tasks;
using Entities.Bakers;
using Entities.Components.Pathfinding;
using Plugins.Extensions;
using Unity.AI.Navigation;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;
using BoxCollider = Unity.Physics.BoxCollider;
using CapsuleCollider = Unity.Physics.CapsuleCollider;
using Collider = Unity.Physics.Collider;
using Debug = UnityEngine.Debug;
using NavMeshModifier = Entities.Bakers.NavMeshModifier;
using NavMeshModifierVolume = Entities.Bakers.NavMeshModifierVolume;
using SphereCollider = Unity.Physics.SphereCollider;

namespace Entities.Systems.Pathdinding
{
    [DisableAutoCreation]
    public partial class NavMeshBakeSystem : SystemBase
    {
        private const float BakeInterval = 2f;
        private const float Range = 50f;
        private const int DefaultBufferSize = 1024 * 2;

        private readonly List<NavMeshData> _navMeshDataBuffer = new List<NavMeshData>();

        private readonly List<NavMeshBuildSource> _sourcesBuffer = new List<NavMeshBuildSource>();
        private readonly List<NavMeshBuildMarkup> _markupsBuffer = new List<NavMeshBuildMarkup>();

        private ComponentLookup<NavMeshModifier> _navMeshModifierLookup;
        private ComponentLookup<Parent> _parentLookup;
        private BufferLookup<AffectedAgentElement> _affectedAgentBufferLookup;
        private ComponentLookup<MeshColliderMeshReference> _meshColliderMeshReferenceLookup;

        private NativeList<BurstedNavMeshBuildSource> _sourcesNativeBuffer;

        private bool _isBaking;
        private float _lastBakeTime;

        protected override void OnCreate()
        {
            _sourcesNativeBuffer = new NativeList<BurstedNavMeshBuildSource>(DefaultBufferSize, Allocator.Persistent);

            _navMeshModifierLookup = GetComponentLookup<NavMeshModifier>(true);
            _parentLookup = GetComponentLookup<Parent>(true);
            _affectedAgentBufferLookup = GetBufferLookup<AffectedAgentElement>(true);
            _meshColliderMeshReferenceLookup = GetComponentLookup<MeshColliderMeshReference>(true);
        }

        protected override void OnUpdate()
        {
            if (_isBaking)
                return;

            float time = (float)SystemAPI.Time.ElapsedTime;
            if (time < _lastBakeTime + BakeInterval)
                return;

            BakeNavMeshes();
        }

        protected override void OnDestroy()
        {
            _sourcesNativeBuffer.Dispose();
        }

        private void BakeNavMeshes()
        {
            Bounds bounds = new Bounds(Vector3.zero, Vector3.one * Range * 2f);

            List<UniTask> tasks = new List<UniTask>();

            Stopwatch stopwatch = Stopwatch.StartNew();

            AssignNavMeshData();

            foreach (NavMeshSurface navMeshSurface in NavMeshSurface.activeSurfaces)
            {
                NavMeshBuildSettings settings = navMeshSurface.GetBuildSettings();

                CollectSources(bounds, navMeshSurface.layerMask, navMeshSurface.useGeometry, false, settings.agentTypeID, navMeshSurface.defaultArea, _sourcesBuffer);

                // _markupsBuffer.Clear();
                // _sourcesBuffer.Clear();
                // NavMeshBuilder.CollectSources(bounds, navMeshSurface.layerMask.value, navMeshSurface.useGeometry, navMeshSurface.defaultArea, _markupsBuffer, _sourcesBuffer);

                UniTask task = NavMeshBuilder.UpdateNavMeshDataAsync(navMeshSurface.navMeshData, settings, _sourcesBuffer, bounds).ToUniTask();

                tasks.Add(task);
            }

            _isBaking = true;
            UniTask
                .WhenAll(tasks)
                .ContinueWith(() =>
                {
                    _isBaking = false;
                    _lastBakeTime = (float)SystemAPI.Time.ElapsedTime;
                })
                .Forget();

            stopwatch.Stop();

            Debug.LogError("Collect sources duration: " + stopwatch.Elapsed.TotalMilliseconds + "ms, sources count: " + _sourcesBuffer.Count);
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

        private void CollectSources(Bounds bounds, LayerMask layerMask, NavMeshCollectGeometry geometry, bool generateLinks, int agentID, int defaultArea,
            List<NavMeshBuildSource> sources)
        {
            sources.Clear();
            _sourcesNativeBuffer.Clear();

            int collidersCount = SystemAPI.QueryBuilder().WithAll<PhysicsCollider>().Build().CalculateEntityCount();
            int meshesCount = SystemAPI.QueryBuilder().WithAll<MeshRendererMeshReference>().Build().CalculateEntityCount();
            int modifierVolumesCount = SystemAPI.QueryBuilder().WithAll<NavMeshModifierVolume>().Build().CalculateEntityCount();

            int targetCapacity = math.max(collidersCount, meshesCount) + modifierVolumesCount;

            if (_sourcesNativeBuffer.Capacity < targetCapacity)
                _sourcesNativeBuffer.SetCapacity(targetCapacity);

            _navMeshModifierLookup.Update(this);
            _parentLookup.Update(this);
            _affectedAgentBufferLookup.Update(this);
            _meshColliderMeshReferenceLookup.Update(this);

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

                collectPhysicSourcesJob.ScheduleParallel(Dependency).Complete();
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

                collectMeshSourcesJob.ScheduleParallel(Dependency).Complete();
            }

            CollectNavMeshModifierVolumeSourcesJob collectNavMeshModifierVolumeSourcesJob = new CollectNavMeshModifierVolumeSourcesJob
            {
                Bounds = new AABB { Center = bounds.center, Extents = bounds.extents },
                AgentID = agentID,
                Sources = _sourcesNativeBuffer.AsParallelWriter()
            };

            collectNavMeshModifierVolumeSourcesJob.ScheduleParallel(Dependency).Complete();

            int count = _sourcesNativeBuffer.Length;
            NativeArray<BurstedNavMeshBuildSource> nativeBuffer = _sourcesNativeBuffer.AsArray();

            for (int i = 0; i < count; i++)
                sources.Add(nativeBuffer[i]);
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

            public void Execute(ref LocalToWorld ltw, ref MeshRendererMeshReference meshRendererMeshReference, RenderFilterSettings renderFilterSettings,
                Entity entity)
            {
                if (((1 << renderFilterSettings.Layer) & (uint)LayerMaskValue) == 0)
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