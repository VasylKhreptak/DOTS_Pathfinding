using Unity.Entities;
using UnityEngine;

namespace Entities.Authoring.Pathfinding.AutoRequest
{
    public class NavMeshObstacleOverlapAutoRequestAuthoring : BaseAutoRequestAuthoring
    {
        [SerializeField] private float _minInterval = 0.5f;

        private void OnValidate() => _minInterval = Mathf.Max(_minInterval, 0);

        private class NavMeshObstacleOverlapAutoRequestBaker : Baker<NavMeshObstacleOverlapAutoRequestAuthoring>
        {
            public override void Bake(NavMeshObstacleOverlapAutoRequestAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new NavMeshObstacleOverlapAutoRequest()
                {
                    MinInterval = authoring._minInterval
                });
            }
        }
    }

    public struct NavMeshObstacleOverlapAutoRequest : IComponentData
    {
        public float MinInterval;
    }
}