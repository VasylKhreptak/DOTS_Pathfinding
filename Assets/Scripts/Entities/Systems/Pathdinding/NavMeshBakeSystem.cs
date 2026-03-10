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

namespace Systems
{
    [DisableAutoCreation]
    public partial class NavMeshBakeSystem : SystemBase
    {
        private const float BakeInterval = 2f;
        private const float Range = 450f;
        private const int DefaultBufferSize = 1024 * 2;

        private bool _isBaking;
        private float _lastBakeTime;

        private readonly List<NavMeshData> _navMeshDataBuffer = new List<NavMeshData>();

        private readonly List<NavMeshBuildSource> _sourcesBuffer = new List<NavMeshBuildSource>();
        private readonly List<NavMeshBuildMarkup> _markupsBuffer = new List<NavMeshBuildMarkup>();

        private NativeList<BurstedNavMeshBuildSource> _sourcesNativeBuffer;

        protected override void OnCreate()
        {
            _sourcesNativeBuffer = new NativeList<BurstedNavMeshBuildSource>(DefaultBufferSize, Allocator.Persistent);
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

                // NavMeshBuilder.CollectSources(bounds, navMeshSurface.layerMask, navMeshSurface.useGeometry, navMeshSurface.defaultArea, _markups, _sources);

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

            EntityManager em = EntityManager;

            if (geometry == NavMeshCollectGeometry.PhysicsColliders)
            {
                foreach ((RefRO<LocalToWorld> ltw, RefRO<PhysicsCollider> physicsCollider, Entity entity) in SystemAPI
                             .Query<RefRO<LocalToWorld>, RefRO<PhysicsCollider>>()
                             .WithEntityAccess())
                {
                    if (physicsCollider.ValueRO.IsValid == false)
                        continue;

                    CollisionFilter filter = physicsCollider.ValueRO.Value.Value.GetCollisionFilter();

                    if ((filter.BelongsTo & (uint)layerMask.value) == 0)
                        continue;

                    NavMeshBuildSource source = new NavMeshBuildSource
                    {
                        area = defaultArea,
                        generateLinks = generateLinks
                    };

                    if (em.HasComponentInParent<NavMeshModifier>(entity, out Entity componentEntity))
                    {
                        NavMeshModifier navMeshModifier = em.GetComponentData<NavMeshModifier>(componentEntity);

                        if (componentEntity != entity && navMeshModifier.ApplyToChildren == false)
                            continue;

                        DynamicBuffer<AffectedAgentElement> affectedAgents = em.GetBuffer<AffectedAgentElement>(componentEntity);

                        bool containsTargetAgent = false;

                        foreach (AffectedAgentElement affectedAgentElement in affectedAgents)
                        {
                            if (affectedAgentElement.ID == agentID)
                            {
                                containsTargetAgent = true;
                                break;
                            }
                        }

                        if (containsTargetAgent)
                        {
                            if (navMeshModifier.Mode == NavMeshModifierMode.RemoveObject)
                                continue;

                            if (navMeshModifier.OverrideArea)
                                source.area = navMeshModifier.Area;

                            if (navMeshModifier.OverrideGenerateLinks)
                                source.generateLinks = navMeshModifier.GenerateLinks;
                        }
                    }

                    unsafe
                    {
                        Collider* collider = physicsCollider.ValueRO.ColliderPtr;

                        switch (collider->Type)
                        {
                            case ColliderType.Box:
                            {
                                BoxCollider boxCollider = *(BoxCollider*)collider;

                                float4x4 transform = float4x4.TRS(ltw.ValueRO.Position, ltw.ValueRO.Rotation, new float3(1));

                                float3 centerOffset = boxCollider.Center;

                                if (ltw.ValueRO.Value.HasNonUniformScale() == false)
                                    centerOffset *= ltw.ValueRO.Value.Scale();

                                transform = math.mul(transform, float4x4.Translate(centerOffset));

                                source.shape = NavMeshBuildSourceShape.Box;
                                source.transform = transform;
                                source.size = boxCollider.Size;

                                if (ltw.ValueRO.Value.HasNonUniformScale() == false)
                                    source.size *= ltw.ValueRO.Value.Scale();

                                break;
                            }
                            case ColliderType.Sphere:
                            {
                                SphereCollider sphereCollider = *(SphereCollider*)collider;

                                float4x4 transform = float4x4.TRS(ltw.ValueRO.Position, ltw.ValueRO.Rotation, new float3(1));

                                float3 centerOffset = sphereCollider.Center;

                                if (ltw.ValueRO.Value.HasNonUniformScale() == false)
                                    centerOffset *= ltw.ValueRO.Value.Scale();

                                transform = math.mul(transform, float4x4.Translate(centerOffset));

                                source.shape = NavMeshBuildSourceShape.Sphere;
                                source.transform = transform;
                                source.size = new float3(sphereCollider.Radius * 2);

                                if (ltw.ValueRO.Value.HasNonUniformScale() == false)
                                    source.size *= ltw.ValueRO.Value.Scale();

                                break;
                            }
                            case ColliderType.Capsule:
                            {
                                CapsuleCollider capsuleCollider = *(CapsuleCollider*)collider;

                                float4x4 transform = float4x4.TRS(ltw.ValueRO.Position, ltw.ValueRO.Rotation, new float3(1));

                                float3 centerOffset = capsuleCollider.Geometry.GetCenter();

                                if (ltw.ValueRO.Value.HasNonUniformScale() == false)
                                    centerOffset *= ltw.ValueRO.Value.Scale();

                                transform = math.mul(transform, float4x4.Translate(centerOffset));

                                source.shape = NavMeshBuildSourceShape.Capsule;
                                source.transform = transform;
                                float height = math.distance(capsuleCollider.Geometry.Vertex0, capsuleCollider.Geometry.Vertex1) + capsuleCollider.Radius * 2;
                                float width = capsuleCollider.Radius * 2;
                                source.size = new float3(width, height, width);

                                if (ltw.ValueRO.Value.HasNonUniformScale() == false)
                                    source.size *= ltw.ValueRO.Value.Scale();

                                break;
                            }
                            case ColliderType.Mesh:
                            {
                                if (em.HasComponent<MeshColliderMeshReference>(entity))
                                {
                                    MeshColliderMeshReference meshColliderMeshReference = em.GetComponentData<MeshColliderMeshReference>(entity);

                                    source.shape = NavMeshBuildSourceShape.Mesh;
                                    source.transform = ltw.ValueRO.Value;
                                    source.sourceObject = meshColliderMeshReference.Value.Value;
                                }

                                break;
                            }
                            default:
                                continue;
                        }
                    }

                    sources.Add(source);
                }
            }
            else
            {
                foreach ((RefRO<LocalToWorld> ltw, RefRO<MeshRendererMeshReference> meshRendererMeshReference, RenderFilterSettings renderFilterSettings,
                             Entity entity) in SystemAPI.Query<RefRO<LocalToWorld>, RefRO<MeshRendererMeshReference>, RenderFilterSettings>().WithEntityAccess())
                {
                    if (((1 << renderFilterSettings.Layer) & (uint)layerMask.value) == 0)
                        continue;

                    NavMeshBuildSource source = new NavMeshBuildSource
                    {
                        area = defaultArea,
                        generateLinks = generateLinks,
                        shape = NavMeshBuildSourceShape.Mesh,
                        transform = ltw.ValueRO.Value,
                        sourceObject = meshRendererMeshReference.ValueRO.Value.Value
                    };

                    if (em.HasComponentInParent<NavMeshModifier>(entity, out Entity componentEntity))
                    {
                        NavMeshModifier navMeshModifier = em.GetComponentData<NavMeshModifier>(componentEntity);

                        if (componentEntity != entity && navMeshModifier.ApplyToChildren == false)
                            continue;

                        DynamicBuffer<AffectedAgentElement> affectedAgents = em.GetBuffer<AffectedAgentElement>(componentEntity);

                        bool containsTargetAgent = false;

                        foreach (AffectedAgentElement affectedAgentElement in affectedAgents)
                        {
                            if (affectedAgentElement.ID == agentID)
                            {
                                containsTargetAgent = true;
                                break;
                            }
                        }

                        if (containsTargetAgent)
                        {
                            if (navMeshModifier.Mode == NavMeshModifierMode.RemoveObject)
                                continue;

                            if (navMeshModifier.OverrideArea)
                                source.area = navMeshModifier.Area;

                            if (navMeshModifier.OverrideGenerateLinks)
                                source.generateLinks = navMeshModifier.GenerateLinks;
                        }
                    }

                    sources.Add(source);
                }
            }

            foreach ((RefRO<LocalToWorld> ltw, RefRO<NavMeshModifierVolume> navMeshModifierVolume, DynamicBuffer<AffectedAgentElement> affectedAgents) in
                     SystemAPI.Query<RefRO<LocalToWorld>, RefRO<NavMeshModifierVolume>, DynamicBuffer<AffectedAgentElement>>())
            {
                bool containsTargetAgent = false;

                foreach (AffectedAgentElement affectedAgentElement in affectedAgents)
                {
                    if (affectedAgentElement.ID == agentID)
                    {
                        containsTargetAgent = true;
                        break;
                    }
                }

                if (containsTargetAgent == false)
                    continue;

                float4x4 transform = float4x4.TRS(ltw.ValueRO.Position, ltw.ValueRO.Rotation, new float3(1));
                transform = math.mul(transform, float4x4.Translate(navMeshModifierVolume.ValueRO.Center * ltw.ValueRO.Value.Scale()));

                NavMeshBuildSource source = new NavMeshBuildSource
                {
                    area = navMeshModifierVolume.ValueRO.AreaType,
                    shape = NavMeshBuildSourceShape.ModifierBox,
                    transform = transform,
                    size = navMeshModifierVolume.ValueRO.Size * ltw.ValueRO.Value.Scale(),
                };

                sources.Add(source);
            }
        }

        [BurstCompile]
        public partial struct CollectPhysicSourcesJob : IJobEntity
        {
            public AABB Bounds;
            public LayerMask LayerMask;
            public int LayerMaskValue;
            bool GenerateLinks;
            int AgentID;
            int DefaultArea;
            
            public NativeList<BurstedNavMeshBuildSource>.ParallelWriter Sources;
            
            public void Execute(ref LocalToWorld ltw, ref PhysicsCollider physicsCollider, Entity entity)
            {
                
            }
        }
    }
}