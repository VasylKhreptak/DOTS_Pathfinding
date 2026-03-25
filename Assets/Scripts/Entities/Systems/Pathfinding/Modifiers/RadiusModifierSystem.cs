using Unity.Burst;
using Unity.Entities;

namespace Entities.Systems.Pathfinding.Modifiers
{
    [BurstCompile]
    [DisableAutoCreation]
    public partial struct RadiusModifierSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state) { }
    }
}