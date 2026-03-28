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
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
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
using TerrainCollider = UnityEngine.TerrainCollider;

namespace Entities.Systems.Pathfinding
{
    [DisableAutoCreation]
    public partial class NavMeshBakeSystem : SystemBase
    {
        private const int BakeThreadsCount = 2;
        private const float BakeInterval = 2f;
        private const float Range = 100f;
        private const int InitialSourcesBufferSize = 1024 * 2;
        private const int NavMeshSourceConversionBatchCount = 32;

        private List<NavMeshData> _navMeshDataBuffer;
        private List<NavMeshBuildSource> _sourcesBuffer;
        private NativeList<UnmanagedNavMeshBuildSource> _sourcesNativeBuffer;
        private NativeArray<int> _intCounter;

        private ComponentLookup<NavMeshModifier> _navMeshModifierLookup;
        private ComponentLookup<Parent> _parentLookup;
        private ComponentLookup<LocalTransform> _localTransformLookup;
        private ComponentLookup<LocalToWorld> _localToWorldLookup;
        private BufferLookup<AffectedAgentElement> _affectedAgentBufferLookup;
        private BufferLookup<PhysicsColliderKeyEntityPair> _colliderKeyEntityPairBufferLookup;
        private ComponentLookup<MeshColliderMeshReference> _meshColliderMeshReferenceLookup;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private bool _isBaking;
        private float _lastCompletedBakeTime;

        protected override void OnCreate()
        {
            _navMeshDataBuffer = new List<NavMeshData>(32);
            _sourcesBuffer = new List<NavMeshBuildSource>(InitialSourcesBufferSize);
            _sourcesNativeBuffer = new NativeList<UnmanagedNavMeshBuildSource>(InitialSourcesBufferSize, Allocator.Persistent);
            _intCounter = new NativeArray<int>(JobsUtility.MaxJobThreadCount, Allocator.Persistent);

            _navMeshModifierLookup = GetComponentLookup<NavMeshModifier>(true);
            _parentLookup = GetComponentLookup<Parent>(true);
            _localTransformLookup = GetComponentLookup<LocalTransform>(true);
            _localToWorldLookup = GetComponentLookup<LocalToWorld>(true);
            _affectedAgentBufferLookup = GetBufferLookup<AffectedAgentElement>(true);
            _colliderKeyEntityPairBufferLookup = GetBufferLookup<PhysicsColliderKeyEntityPair>(true);
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
            _intCounter.Dispose();
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

                settings.maxJobWorkers = BakeThreadsCount;

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
                if (!_navMeshDataBuffer.Contains(navMeshSurface.navMeshData))
                {
                    NavMeshData navMeshData = new NavMeshData();

                    navMeshSurface.navMeshData = navMeshData;

                    navMeshSurface.RemoveData();
                    navMeshSurface.AddData();

                    _navMeshDataBuffer.Add(navMeshData);
                }
            }

            _navMeshDataBuffer.RemoveAll(navMeshData => NavMeshSurface.activeSurfaces.All(surface => surface.navMeshData != navMeshData));
        }

        private async UniTask CollectSourcesAsync(Bounds bounds, LayerMask layerMask, NavMeshCollectGeometry geometry, bool generateLinks, int agentID, int defaultArea,
            List<NavMeshBuildSource> sources, CancellationToken token)
        {
            sources.Clear();
            _sourcesNativeBuffer.Clear();

            EnsureSourceBufferCapacity(geometry, bounds, layerMask);

            _navMeshModifierLookup.Update(this);
            _parentLookup.Update(this);
            _localTransformLookup.Update(this);
            _localToWorldLookup.Update(this);
            _affectedAgentBufferLookup.Update(this);
            _colliderKeyEntityPairBufferLookup.Update(this);
            _meshColliderMeshReferenceLookup.Update(this);

            JobHandle collectSourcesJobHandle;

            if (geometry == NavMeshCollectGeometry.PhysicsColliders)
            {
                CollectPhysicSourcesJob collectPhysicSourcesJob = new CollectPhysicSourcesJob
                {
                    Bounds = new Aabb { Min = bounds.min, Max = bounds.max },
                    LayerMaskValue = layerMask.value,
                    GenerateLinks = generateLinks,
                    AgentID = agentID,
                    DefaultArea = defaultArea,
                    NavMeshModifierLookup = _navMeshModifierLookup,
                    ParentLookup = _parentLookup,
                    LocalTransformLookup = _localTransformLookup,
                    LocalToWorldLookup = _localToWorldLookup,
                    AffectedAgentBufferLookup = _affectedAgentBufferLookup,
                    ColliderKeyEntityPairBufferLookup = _colliderKeyEntityPairBufferLookup,
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

            await UniTask.WaitUntil(() => Dependency.IsCompleted, cancellationToken: token);

            await UniTask.SwitchToMainThread(token);

            NavMeshBuildSource source = default;

            for (int i = 0; i < _sourcesNativeBuffer.Length; i++)
            {
                UnmanagedNavMeshBuildSource unmanagedSource = _sourcesNativeBuffer[i];

                source.transform = unmanagedSource.TransformMatrix;
                source.size = unmanagedSource.Size;
                source.shape = unmanagedSource.Shape;
                source.area = unmanagedSource.Area;
                source.sourceObject = unmanagedSource.MeshReference.Value;
                source.generateLinks = unmanagedSource.GenerateLinks;

                sources.Add(source);

                if (i % NavMeshSourceConversionBatchCount == 0)
                    await UniTask.Yield(token);
            }

            await UniTask.Yield(token);

            CollectTerrainSources(bounds, layerMask, defaultArea, geometry, sources);
        }

        private void EnsureSourceBufferCapacity(NavMeshCollectGeometry geometry, Bounds bounds, LayerMask layerMask)
        {
            int modifierVolumesCount = SystemAPI.QueryBuilder().WithAll<NavMeshModifierVolume>().Build().CalculateEntityCount();

            int terrainsCount = CalculateTerrainSourcesCount(bounds, layerMask, geometry);

            for (int i = 0; i < _intCounter.Length; i++)
                _intCounter[i] = 0;

            int sourcesCount = 0;

            if (geometry == NavMeshCollectGeometry.PhysicsColliders)
            {
                CalculatePhysicSourcesCountJob calculatePhysicSourcesCountJob = new CalculatePhysicSourcesCountJob
                {
                    Bounds = new Aabb { Min = bounds.min, Max = bounds.max },
                    LayerMaskValue = layerMask.value,
                    Counter = _intCounter
                };

                Dependency = calculatePhysicSourcesCountJob.ScheduleParallel(Dependency);
                Dependency.Complete();

                for (int i = 0; i < _intCounter.Length; i++)
                    sourcesCount += _intCounter[i];
            }
            else
            {
                CalculateMeshSourcesCountJob calculateMeshSourcesCountJob = new CalculateMeshSourcesCountJob
                {
                    Bounds = new AABB { Center = bounds.center, Extents = bounds.extents },
                    LayerMaskValue = layerMask.value,
                    Counter = _intCounter
                };

                Dependency = calculateMeshSourcesCountJob.ScheduleParallel(Dependency);
                Dependency.Complete();

                for (int i = 0; i < _intCounter.Length; i++)
                    sourcesCount += _intCounter[i];
            }

            int targetCapacity = sourcesCount + modifierVolumesCount + terrainsCount;

            if (_sourcesNativeBuffer.Capacity < targetCapacity)
                _sourcesNativeBuffer.SetCapacity(targetCapacity);

            if (_sourcesBuffer.Capacity < targetCapacity)
                _sourcesBuffer.Capacity = targetCapacity;
        }

        [BurstCompile]
        private partial struct CalculatePhysicSourcesCountJob : IJobEntity
        {
            public Aabb Bounds;
            public int LayerMaskValue;

            [NativeDisableParallelForRestriction] public NativeArray<int> Counter;

            [NativeSetThreadIndex] private int _threadIndex;

            public void Execute(in LocalToWorld ltw, in PhysicsCollider physicsCollider, in Entity entity)
            {
                if (!physicsCollider.IsValid)
                    return;

                unsafe
                {
                    Collider* colliderPtr = physicsCollider.ColliderPtr;

                    CollisionFilter filter = colliderPtr->GetCollisionFilter();

                    if ((filter.BelongsTo & LayerMaskValue) == 0)
                        return;

                    RigidTransform worldTransform = new RigidTransform(ltw.Value.GetRotation(), ltw.Value.GetPosition());

                    Aabb colliderAabb = colliderPtr->CalculateAabb(worldTransform);

                    if (!Bounds.Contains(colliderAabb) && !Bounds.Overlaps(colliderAabb))
                        return;

                    if (colliderPtr->Type == ColliderType.Compound)
                    {
                        CompoundCollider compoundCollider = *(CompoundCollider*)colliderPtr;

                        Counter[_threadIndex] += compoundCollider.NumChildren;
                    }
                    else
                        Counter[_threadIndex]++;
                }
            }
        }

        [BurstCompile]
        [WithAll(typeof(MeshRendererMeshReference))]
        private partial struct CalculateMeshSourcesCountJob : IJobEntity
        {
            public AABB Bounds;
            public int LayerMaskValue;

            [NativeDisableParallelForRestriction] public NativeArray<int> Counter;

            [NativeSetThreadIndex] private int _threadIndex;

            public void Execute(in WorldRenderBounds worldRenderBounds, in RenderFilterSettings renderFilterSettings, in Entity entity)
            {
                if (((1 << renderFilterSettings.Layer) & (uint)LayerMaskValue) == 0)
                    return;

                if (!Bounds.Contains(worldRenderBounds.Value) && !Bounds.Overlaps(worldRenderBounds.Value))
                    return;

                Counter[_threadIndex]++;
            }
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
            [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
            [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorldLookup;
            [ReadOnly] public BufferLookup<AffectedAgentElement> AffectedAgentBufferLookup;
            [ReadOnly] public BufferLookup<PhysicsColliderKeyEntityPair> ColliderKeyEntityPairBufferLookup;
            [ReadOnly] public ComponentLookup<MeshColliderMeshReference> MeshColliderMeshReferenceLookup;

            [WriteOnly] public NativeList<UnmanagedNavMeshBuildSource>.ParallelWriter Sources;

            public void Execute(in LocalToWorld ltw, in PhysicsCollider physicsCollider, in Entity entity)
            {
                if (!physicsCollider.IsValid)
                    return;

                unsafe
                {
                    TryAddColliderSource(physicsCollider.ColliderPtr, ltw.Value, entity);
                }
            }

            private unsafe void TryAddColliderSource(Collider* colliderPtr, float4x4 matrix, Entity entity)
            {
                int area = DefaultArea;
                bool generateLinks = GenerateLinks;

                if (colliderPtr->Type != ColliderType.Compound)
                {
                    CollisionFilter filter = colliderPtr->GetCollisionFilter();

                    if ((filter.BelongsTo & LayerMaskValue) == 0)
                        return;

                    RigidTransform worldTransform = new RigidTransform(matrix.GetRotation(), matrix.GetPosition());

                    Aabb colliderAabb = colliderPtr->CalculateAabb(worldTransform);

                    if (!Bounds.Contains(colliderAabb) && !Bounds.Overlaps(colliderAabb))
                        return;

                    if (EntityManagerExtensions.HasComponentInParent(entity, ref NavMeshModifierLookup, ref ParentLookup, out Entity componentEntity))
                    {
                        NavMeshModifier navMeshModifier = NavMeshModifierLookup[componentEntity];

                        if (componentEntity != entity && !navMeshModifier.ApplyToChildren)
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
                                area = navMeshModifier.Area;

                            if (navMeshModifier.OverrideGenerateLinks)
                                generateLinks = navMeshModifier.GenerateLinks;
                        }
                    }
                }

                switch (colliderPtr->Type)
                {
                    case ColliderType.Box:
                    {
                        BoxCollider boxCollider = *(BoxCollider*)colliderPtr;

                        float4x4 transform = float4x4.TRS(matrix.GetPosition(), matrix.GetRotation(), 1f);

                        float3 originalScale = matrix.GetScale();

                        float3 centerOffset = boxCollider.Center;

                        if (!matrix.HasNonUniformScale())
                            centerOffset *= originalScale;

                        transform = math.mul(transform, float4x4.Translate(centerOffset));

                        UnmanagedNavMeshBuildSource source = new UnmanagedNavMeshBuildSource
                        {
                            Area = area,
                            GenerateLinks = generateLinks,
                            Shape = NavMeshBuildSourceShape.Box,
                            TransformMatrix = transform,
                            Size = boxCollider.Size
                        };

                        if (!matrix.HasNonUniformScale())
                            source.Size *= originalScale;

                        Sources.AddNoResize(source);

                        break;
                    }
                    case ColliderType.Sphere:
                    {
                        SphereCollider sphereCollider = *(SphereCollider*)colliderPtr;

                        float4x4 transform = float4x4.TRS(matrix.GetPosition(), matrix.GetRotation(), 1f);

                        float3 originalScale = matrix.GetScale();

                        float3 centerOffset = sphereCollider.Center;

                        if (!matrix.HasNonUniformScale())
                            centerOffset *= originalScale;

                        transform = math.mul(transform, float4x4.Translate(centerOffset));

                        UnmanagedNavMeshBuildSource source = new UnmanagedNavMeshBuildSource
                        {
                            Area = area,
                            GenerateLinks = generateLinks,
                            Shape = NavMeshBuildSourceShape.Sphere,
                            TransformMatrix = transform,
                            Size = new float3(sphereCollider.Radius * 2)
                        };

                        if (!matrix.HasNonUniformScale())
                            source.Size *= originalScale;

                        Sources.AddNoResize(source);

                        break;
                    }
                    case ColliderType.Capsule:
                    {
                        CapsuleCollider capsuleCollider = *(CapsuleCollider*)colliderPtr;

                        float4x4 transform = float4x4.TRS(matrix.GetPosition(), matrix.GetRotation(), 1f);

                        float3 originalScale = matrix.GetScale();

                        float3 centerOffset = capsuleCollider.Geometry.GetCenter();

                        if (!matrix.HasNonUniformScale())
                            centerOffset *= originalScale;

                        transform = math.mul(transform, float4x4.Translate(centerOffset));

                        float height = math.distance(capsuleCollider.Geometry.Vertex0, capsuleCollider.Geometry.Vertex1) + capsuleCollider.Radius * 2;
                        float width = capsuleCollider.Radius * 2;

                        UnmanagedNavMeshBuildSource source = new UnmanagedNavMeshBuildSource
                        {
                            Area = area,
                            GenerateLinks = generateLinks,
                            Shape = NavMeshBuildSourceShape.Capsule,
                            TransformMatrix = transform,
                            Size = new float3(width, height, width)
                        };

                        if (!matrix.HasNonUniformScale())
                            source.Size *= originalScale;

                        Sources.AddNoResize(source);

                        break;
                    }
                    case ColliderType.Mesh:
                    {
                        if (MeshColliderMeshReferenceLookup.HasComponent(entity))
                        {
                            MeshColliderMeshReference meshColliderMeshReference = MeshColliderMeshReferenceLookup[entity];

                            UnmanagedNavMeshBuildSource source = new UnmanagedNavMeshBuildSource
                            {
                                Area = area,
                                GenerateLinks = generateLinks,
                                Shape = NavMeshBuildSourceShape.Mesh,
                                TransformMatrix = matrix,
                                MeshReference = meshColliderMeshReference.Value
                            };

                            Sources.AddNoResize(source);
                        }

                        break;
                    }
                    case ColliderType.Compound:
                    {
                        CompoundCollider* compoundCollider = (CompoundCollider*)colliderPtr;

                        DynamicBuffer<PhysicsColliderKeyEntityPair> colliderKeyEntityPairs = ColliderKeyEntityPairBufferLookup[entity];

                        for (int i = 0; i < colliderKeyEntityPairs.Length; i++)
                        {
                            ColliderKey colliderKey = colliderKeyEntityPairs[i].Key;
                            Entity childEntity = colliderKeyEntityPairs[i].Entity;

                            compoundCollider->GetChild(ref colliderKey, out ChildCollider childCollider);
                            Collider* childColliderPtr = childCollider.Collider;

                            if (childColliderPtr->Type == ColliderType.Mesh)
                            {
                                LocalToWorld localToWorld = LocalToWorldLookup[childEntity];
                                TryAddColliderSource(childColliderPtr, localToWorld.Value, childEntity);
                            }
                            else
                            {
                                LocalTransform localTransform = LocalTransformLookup[childEntity];
                                TryAddColliderSource(childColliderPtr, math.mul(matrix, float4x4.TRS(localTransform.Position, localTransform.Rotation, 1f)), childEntity);
                            }
                        }

                        break;
                    }
                }
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

            [WriteOnly] public NativeList<UnmanagedNavMeshBuildSource>.ParallelWriter Sources;

            public void Execute(in LocalToWorld ltw, in MeshRendererMeshReference meshRendererMeshReference, in WorldRenderBounds worldRenderBounds,
                in RenderFilterSettings renderFilterSettings, in Entity entity)
            {
                if (((1 << renderFilterSettings.Layer) & (uint)LayerMaskValue) == 0)
                    return;

                if (!Bounds.Contains(worldRenderBounds.Value) && !Bounds.Overlaps(worldRenderBounds.Value))
                    return;

                UnmanagedNavMeshBuildSource source = new UnmanagedNavMeshBuildSource
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

                    if (componentEntity != entity && !navMeshModifier.ApplyToChildren)
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

            [WriteOnly] public NativeList<UnmanagedNavMeshBuildSource>.ParallelWriter Sources;

            public void Execute(in LocalToWorld ltw, in NavMeshModifierVolume navMeshModifierVolume, in DynamicBuffer<AffectedAgentElement> affectedAgents)
            {
                AABB localBounds = new AABB { Center = navMeshModifierVolume.Center, Extents = navMeshModifierVolume.Size / 2f };
                AABB worldBounds = localBounds.ToWorld(ltw.Value);

                if (!Bounds.Contains(worldBounds) && !Bounds.Overlaps(worldBounds))
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

                if (!containsTargetAgent)
                    return;

                float4x4 transform = float4x4.TRS(ltw.Position, ltw.Rotation, new float3(1));
                transform = math.mul(transform, float4x4.Translate(navMeshModifierVolume.Center * ltw.Value.Scale()));

                UnmanagedNavMeshBuildSource source = new UnmanagedNavMeshBuildSource
                {
                    Area = navMeshModifierVolume.AreaType,
                    Shape = NavMeshBuildSourceShape.ModifierBox,
                    TransformMatrix = transform,
                    Size = navMeshModifierVolume.Size * ltw.Value.Scale()
                };

                Sources.AddNoResize(source);
            }
        }

        private int CalculateTerrainSourcesCount(Bounds bounds, LayerMask layerMask, NavMeshCollectGeometry geometry)
        {
            int count = 0;

            foreach (Terrain terrain in Terrain.activeTerrains)
            {
                if (!layerMask.ContainsLayer(terrain.gameObject.layer))
                    continue;

                Bounds terrainBounds = terrain.terrainData.bounds;
                terrainBounds.center += terrain.transform.position;

                if (!bounds.Contains(terrainBounds) && !bounds.Intersects(terrainBounds))
                    continue;

                if (geometry == NavMeshCollectGeometry.PhysicsColliders)
                {
                    if (!terrain.TryGetComponent(out TerrainCollider _))
                        continue;
                }

                count++;
            }

            return count;
        }

        private void CollectTerrainSources(Bounds bounds, LayerMask layerMask, int area, NavMeshCollectGeometry geometry, List<NavMeshBuildSource> sources)
        {
            NavMeshBuildSource source = new NavMeshBuildSource
            {
                area = area,
                shape = NavMeshBuildSourceShape.Terrain
            };

            foreach (Terrain terrain in Terrain.activeTerrains)
            {
                if (!layerMask.ContainsLayer(terrain.gameObject.layer))
                    continue;

                Bounds terrainBounds = terrain.terrainData.bounds;
                terrainBounds.center += terrain.transform.position;

                if (!bounds.Contains(terrainBounds) && !bounds.Intersects(terrainBounds))
                    continue;

                if (geometry == NavMeshCollectGeometry.PhysicsColliders)
                {
                    if (!terrain.TryGetComponent(out TerrainCollider _))
                        continue;
                }

                source.transform = terrain.transform.localToWorldMatrix;
                source.sourceObject = terrain.terrainData;
                source.component = terrain;

                sources.Add(source);
            }
        }
    }
}