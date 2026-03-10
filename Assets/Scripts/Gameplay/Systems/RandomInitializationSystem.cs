using Gameplay.Components;
using Unity.Burst;
using Unity.Collections;
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
            EntityCommandBuffer endJobECB = new EntityCommandBuffer(Allocator.TempJob);

            InitializeRandomComponentsJob job = new InitializeRandomComponentsJob
            {
                EndJobECB = endJobECB.AsParallelWriter(),
                ElapsedTime = state.WorldUnmanaged.Time.ElapsedTime
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();

            endJobECB.Playback(state.EntityManager);
            endJobECB.Dispose();
        }

        [BurstCompile]
        [WithAll(typeof(RandomNeedsInitializationFlag))]
        public partial struct InitializeRandomComponentsJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter EndJobECB;
            public double ElapsedTime;

            public void Execute([EntityIndexInQuery] int sortKey, ref RandomComponent randomComponent, in Entity entity)
            {
                uint seed = math.hash(new uint2((uint)sortKey, (uint)(ElapsedTime * 1000))) + 1u;

                randomComponent.Value = new Random(seed);
                EndJobECB.SetComponentEnabled<RandomNeedsInitializationFlag>(sortKey, entity, false);
            }
        }
    }
}