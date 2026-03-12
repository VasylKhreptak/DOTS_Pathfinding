using Unity.Entities;

namespace Entities.Components.Pathfinding
{
    [InternalBufferCapacity(32)]
    public struct AffectedAgentElement : IBufferElementData
    {
        public int ID;
    }
}