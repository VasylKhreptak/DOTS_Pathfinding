using Unity.Entities;

namespace Entities.Components.Pathfinding
{
    public struct PathfindingSystemData : IComponentData
    {
        public int PathFindersCount;
        public int RequestedPathsCount;
        public int InProgressPathsCount;

        public bool SkipNewRequests;
    }
}