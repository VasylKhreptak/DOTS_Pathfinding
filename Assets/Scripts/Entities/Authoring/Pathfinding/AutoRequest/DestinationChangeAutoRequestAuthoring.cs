using Unity.Entities;
using UnityEngine;

namespace Entities.Authoring.Pathfinding.AutoRequest
{
    public class DestinationChangeAutoRequestAuthoring : BaseAutoRequestAuthoring
    {
        [SerializeField] private float _minThreshold = 0.3f;
        [SerializeField] private float _minDistance = 10f;
        [SerializeField] private float _maxThreshold = 20f;
        [SerializeField] private float _maxDistance = 200f;

        private void OnValidate()
        {
            _minThreshold = Mathf.Max(_minThreshold, 0f);
            _maxThreshold = Mathf.Max(_maxThreshold, _minThreshold);
            _minDistance = Mathf.Max(_minDistance, 0f);
            _maxDistance = Mathf.Max(_maxDistance, _minDistance);
        }

        private class DestinationChangeAutoRequestBaker : Baker<DestinationChangeAutoRequestAuthoring>
        {
            public override void Bake(DestinationChangeAutoRequestAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new DestinationChangeAutoRequest
                {
                    MinThreshold = authoring._minThreshold,
                    MinDistance = authoring._minDistance,
                    MaxThreshold = authoring._maxThreshold,
                    MaxDistance = authoring._maxDistance
                });
            }
        }
    }

    public struct DestinationChangeAutoRequest : IComponentData
    {
        public float MinThreshold;
        public float MinDistance;
        public float MaxThreshold;
        public float MaxDistance;
    }
}