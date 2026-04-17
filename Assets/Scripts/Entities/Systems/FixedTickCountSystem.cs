using Entities.Components;
using Unity.Burst;
using Unity.Entities;
using ISystem = Unity.Entities.ISystem;

namespace Entities.Systems
{
    [BurstCompile]
    [DisableAutoCreation]
    public partial struct FixedTickCountSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton<FixedTickCount>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            RefRW<FixedTickCount> fixedTickCount = SystemAPI.GetSingletonRW<FixedTickCount>();
            fixedTickCount.ValueRW.Value++;
        }
    }
}