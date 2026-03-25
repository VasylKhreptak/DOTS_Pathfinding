using Entities.Bakers.Pathfinding;
using Plugins.Extensions;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Entities.Systems.Pathdinding
{
    [DisableAutoCreation]
    public partial class NavMeshObstacleSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<NavMeshObstacle>();
        }

        protected override void OnUpdate()
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            CreateNavMeshObstacles(ecb);
            UpdateNavMeshObstacles();
            DestroyNavMeshObstacles(ecb);

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        private void CreateNavMeshObstacles(EntityCommandBuffer ecb)
        {
            foreach ((RefRO<NavMeshObstacle> navMeshObstacle, RefRO<LocalToWorld> localToWorld, Entity entity) in SystemAPI
                         .Query<RefRO<NavMeshObstacle>, RefRO<LocalToWorld>>()
                         .WithEntityAccess()
                         .WithNone<NavMeshObstacleReference>())
            {
                GameObject gameObject = new GameObject("NavMeshObstacle");

                UnityEngine.AI.NavMeshObstacle managedObstacle = gameObject.AddComponent<UnityEngine.AI.NavMeshObstacle>();

                CopyData(in navMeshObstacle.ValueRO, in localToWorld.ValueRO.Value, managedObstacle);

                NavMeshObstacleReference reference = new NavMeshObstacleReference
                {
                    Value = managedObstacle
                };

                ecb.AddComponent(entity, reference);
            }
        }

        private void UpdateNavMeshObstacles()
        {
            foreach ((RefRO<NavMeshObstacle> navMeshObstacle, RefRO<NavMeshObstacleReference> navMeshObstacleReference, RefRO<LocalToWorld> localToWorld) in SystemAPI
                         .Query<RefRO<NavMeshObstacle>, RefRO<NavMeshObstacleReference>, RefRO<LocalToWorld>>()
                         .WithChangeFilter<LocalToWorld>()
                         .WithChangeFilter<NavMeshObstacle>())
            {
                UnityEngine.AI.NavMeshObstacle managedObstacle = navMeshObstacleReference.ValueRO.Value;

                CopyData(in navMeshObstacle.ValueRO, in localToWorld.ValueRO.Value, managedObstacle);
            }
        }

        private void DestroyNavMeshObstacles(EntityCommandBuffer ecb)
        {
            foreach ((RefRO<NavMeshObstacleReference> navMeshObstacleReference, Entity entity) in SystemAPI
                         .Query<RefRO<NavMeshObstacleReference>>()
                         .WithEntityAccess()
                         .WithNone<LocalTransform>())
            {
                Object.Destroy(navMeshObstacleReference.ValueRO.Value);

                ecb.RemoveComponent<NavMeshObstacleReference>(entity);
            }
        }

        private void CopyData(in NavMeshObstacle source, in float4x4 sourceMatrix, UnityEngine.AI.NavMeshObstacle target)
        {
            target.transform.SetPositionAndRotation(sourceMatrix.GetPosition(), sourceMatrix.GetRotation());
            target.transform.localScale = sourceMatrix.GetScale();

            if (target.shape != source.Shape)
                target.shape = source.Shape;

            if (target.center != (Vector3)source.Center)
                target.center = source.Center;

            if (target.size != (Vector3)source.Size)
                target.size = source.Size;

            if (!Mathf.Approximately(target.radius, source.Radius))
                target.radius = source.Radius;

            if (!Mathf.Approximately(target.height, source.Height))
                target.height = source.Height;

            if (target.carving != source.Carve)
                target.carving = source.Carve;

            if (!Mathf.Approximately(target.carvingMoveThreshold, source.MoveThreshold))
                target.carvingMoveThreshold = source.MoveThreshold;

            if (!Mathf.Approximately(target.carvingTimeToStationary, source.TimeToStationary))
                target.carvingTimeToStationary = source.TimeToStationary;

            if (target.carveOnlyStationary != source.CarveOnlyStationary)
                target.carveOnlyStationary = source.CarveOnlyStationary;
        }
    }
}