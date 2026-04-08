using Unity.Entities;
using UnityEngine;

namespace Entities.Authoring.Pathfinding.AutoRequest
{
    public class PathDynamicIntervalAutoRequestAuthoring : BaseAutoRequestAuthoring
    {
        [SerializeField] private float _minInterval = 0.3f;
        [SerializeField] private float _minDistance = 10f;
        [SerializeField] private float _maxInterval = 4f;
        [SerializeField] private float _maxDistance = 300f;

        private void OnValidate()
        {
            _minInterval = Mathf.Max(_minInterval, 0f);
            _maxInterval = Mathf.Max(_maxInterval, _minInterval);
            _minDistance = Mathf.Max(_minDistance, 0f);
            _maxDistance = Mathf.Max(_maxDistance, _minDistance);
        }

        private class PathDynamicIntervalAutoRequestBaker : Baker<PathDynamicIntervalAutoRequestAuthoring>
        {
            public override void Bake(PathDynamicIntervalAutoRequestAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new PathDynamicIntervalAutoRequest
                {
                    MinInterval = authoring._minInterval,
                    MaxInterval = authoring._maxInterval,
                    MinDistance = authoring._minDistance,
                    MaxDistance = authoring._maxDistance
                });
            }
        }
    }

    public struct PathDynamicIntervalAutoRequest : IComponentData
    {
        public float MinInterval;
        public float MaxInterval;
        public float MinDistance;
        public float MaxDistance;
    }
}