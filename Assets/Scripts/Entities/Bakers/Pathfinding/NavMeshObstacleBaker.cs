using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.AI;

namespace Entities.Bakers.Pathfinding
{
    public class NavMeshObstacleBaker : Baker<UnityEngine.AI.NavMeshObstacle>
    {
        public override void Bake(UnityEngine.AI.NavMeshObstacle authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            NavMeshObstacle navMeshObstacle = new NavMeshObstacle()
            {
                Shape = authoring.shape,
                Center = authoring.center,
                Size = authoring.size,
                Radius = authoring.radius,
                Height = authoring.height,
                Carve = authoring.carving,
                MoveThreshold = authoring.carvingMoveThreshold,
                TimeToStationary = authoring.carvingTimeToStationary,
                CarveOnlyStationary = authoring.carveOnlyStationary
            };

            AddComponent(entity, navMeshObstacle);
        }
    }

    public struct NavMeshObstacle : IComponentData, IEnableableComponent
    {
        public NavMeshObstacleShape Shape;
        public float3 Center;
        public float3 Size;
        public float Radius;
        public float Height;
        public bool Carve;
        public float MoveThreshold;
        public float TimeToStationary;
        public bool CarveOnlyStationary;
    }

    public struct NavMeshObstacleReference : ICleanupComponentData
    {
        public UnityObjectRef<UnityEngine.AI.NavMeshObstacle> Value;
    }
}