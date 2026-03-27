using Unity.Entities;
using UnityEngine;

namespace Entities.Authoring.Pathfinding
{
    public class OptimizedUpdateIntervalAuthoring : MonoBehaviour
    {
        [SerializeField] private float _minInterval = 0.1f;
        [SerializeField] private float _minDistance = 10;
        [SerializeField] private float _maxInterval = 4f;
        [SerializeField] private float _maxDistance = 300f;

        private class SmartPathUpdateBaker : Baker<OptimizedUpdateIntervalAuthoring>
        {
            public override void Bake(OptimizedUpdateIntervalAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                OptimizedUpdateInterval optimizedUpdateInterval = new OptimizedUpdateInterval()
                {
                    MinInterval = authoring._minInterval,
                    MinDistance = authoring._minDistance,
                    MaxInterval = authoring._maxInterval,
                    MaxDistance = authoring._maxDistance
                };

                AddComponent(entity, optimizedUpdateInterval);
            }
        }
    }

    public struct OptimizedUpdateInterval : IComponentData
    {
        public float MinInterval;
        public float MinDistance;
        public float MaxInterval;
        public float MaxDistance;
    }
}