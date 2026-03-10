using Unity.Entities;

namespace Components
{
    [InternalBufferCapacity(32)]
    public struct AffectedAgentElement : IBufferElementData
    {
        public int ID;
    }
}