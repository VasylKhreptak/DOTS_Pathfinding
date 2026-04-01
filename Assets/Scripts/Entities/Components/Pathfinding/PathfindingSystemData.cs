using Unity.Entities;

namespace Entities.Components.Pathfinding
{
    public struct PathfindingSystemData : IComponentData
    {
        public int SeekersCount;
        public int RequestedPathsCount;
        public int InProgressPathsCount;

        public bool SkipNewRequests;
    }
}