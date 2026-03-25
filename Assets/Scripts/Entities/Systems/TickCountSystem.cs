using Entities.Components;
using Unity.Burst;
using Unity.Entities;
using ISystem = Unity.Entities.ISystem;

namespace Entities.Systems
{
    [DisableAutoCreation]
    [BurstCompile]
    public partial struct TickCountSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton<TickCount>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            RefRW<TickCount> tickCount = SystemAPI.GetSingletonRW<TickCount>();
            tickCount.ValueRW.Value++;
        }
    }
}