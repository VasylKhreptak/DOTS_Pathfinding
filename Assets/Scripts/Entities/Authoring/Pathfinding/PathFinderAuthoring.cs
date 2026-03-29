using Unity.Entities;
using UnityEngine;

namespace Entities.Authoring.Pathfinding
{
    public class PathFinderAuthoring : MonoBehaviour
    {
        [SerializeField] private float _calculateInterval;

        #region MonoBehaviour

        private void OnValidate() => _calculateInterval = Mathf.Max(0.1f, _calculateInterval);

        #endregion

        private class PathFinderBaker : Baker<PathFinderAuthoring>
        {
            public override void Bake(PathFinderAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                PathFinder pathFinder = new PathFinder
                {
                    CalculateInterval = authoring._calculateInterval
                };

                AddComponent(entity, pathFinder);
            }
        }
    }

    public struct PathFinder : IComponentData, IEnableableComponent
    {
        public PathStatus Status;
        public float LastCalculationTime;
        public long LastCalculationTickCount;
        public float CalculateInterval;
    }

    public enum PathStatus
    {
        Requested,
        InProgress,
        Success,
        Failure
    }
}