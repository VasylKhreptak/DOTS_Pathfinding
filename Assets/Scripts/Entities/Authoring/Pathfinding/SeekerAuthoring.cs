using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Entities.Authoring.Pathfinding
{
    public class SeekerAuthoring : MonoBehaviour
    {
        [SerializeField] private float _startPositionSnappingDistance = 500f;
        [SerializeField] private float _destinationPositionSnappingDistance = 500f;
        [SerializeField] private float _calculateInterval = 0.3f;

        #region MonoBehaviour

        private void OnValidate()
        {
            _calculateInterval = Mathf.Max(0.1f, _calculateInterval);
            _startPositionSnappingDistance = Mathf.Max(1, _startPositionSnappingDistance);
            _destinationPositionSnappingDistance = Mathf.Max(1, _destinationPositionSnappingDistance);
        }

        #endregion

        private class Baker : Baker<SeekerAuthoring>
        {
            public override void Bake(SeekerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                Seeker seeker = new Seeker
                {
                    StartPositionSnappingDistance = authoring._startPositionSnappingDistance,
                    DestinationPositionSnappingDistance = authoring._destinationPositionSnappingDistance,
                    CalculateInterval = authoring._calculateInterval
                };

                AddComponent(entity, seeker);
            }
        }
    }

    public struct Seeker : IComponentData
    {
        public float StartPositionSnappingDistance;
        public float DestinationPositionSnappingDistance;
        public float CalculateInterval;

        public PathStatus Status;
        public float LastCalculationTime;
        public long LastCalculationTickCount;

        public float3 RequestStartPosition;
        public float3 RequestEndPosition;
        public float3 NavMeshStartPosition;
        public float3 NavMeshEndPosition;
    }

    public struct SeekerQuerryIndex : ICleanupComponentData
    {
        public int Value;
    }

    public enum PathStatus
    {
        Requested,
        InProgress,
        Success,
        Failure
    }
}