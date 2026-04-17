using Unity.Entities;

namespace Entities.Components
{
    public struct FixedTickCount : IComponentData
    {
        public long Value;
    }
}