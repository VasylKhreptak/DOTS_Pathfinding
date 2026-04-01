using Unity.Entities;

namespace Entities.Components.Pathfinding
{
    public struct NavMeshBakeSystemData : IComponentData
    {
        public bool IsUpdatingNavMeshData;
    }
}