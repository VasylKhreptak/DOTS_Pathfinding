using Gameplay.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace Gameplay.Systems
{
    [DisableAutoCreation]
    public partial struct RandomInitializationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RandomNeedsInitializationFlag>();

            Initialize(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) => Initialize(ref state);

        private void Initialize(ref SystemState state)
        {
            InitializeRandomComponentsJob job = new InitializeRandomComponentsJob
            {
                ElapsedTime = state.WorldUnmanaged.Time.ElapsedTime
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct InitializeRandomComponentsJob : IJobEntity
        {
            public double ElapsedTime;

            public void Execute([EntityIndexInQuery] int sortKey, ref RandomComponent randomComponent, in Entity entity,
                EnabledRefRW<RandomNeedsInitializationFlag> randomNeedsInitializationFlag)
            {
                uint seed = math.hash(new uint2((uint)sortKey, (uint)(ElapsedTime * 1000))) + 1u;

                randomComponent.Value = new Random(seed);
                randomNeedsInitializationFlag.ValueRW = false;
            }
        }
    }
}