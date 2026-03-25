using Unity.Entities;

namespace Entities.Components
{
    public struct TickCount : IComponentData
    {
        public long Value;
    }
}