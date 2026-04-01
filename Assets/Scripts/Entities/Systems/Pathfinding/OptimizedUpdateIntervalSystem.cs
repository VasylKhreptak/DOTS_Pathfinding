using Entities.Authoring.Pathfinding;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Entities.Systems.Pathfinding
{
    [BurstCompile]
    [DisableAutoCreation]
    public partial struct OptimizedUpdateIntervalSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            UpdateIntervalsJob job = new UpdateIntervalsJob();

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct UpdateIntervalsJob : IJobEntity
        {
            public void Execute(in LocalToWorld localToWorld, in Destination destination, ref Seeker seeker, in OptimizedUpdateInterval optimizedUpdateInterval)
            {
                float distance = math.distance(localToWorld.Position, destination.Value);

                float newInterval = math.remap(optimizedUpdateInterval.MinDistance,
                    optimizedUpdateInterval.MaxDistance,
                    optimizedUpdateInterval.MinInterval,
                    optimizedUpdateInterval.MaxInterval,
                    distance);

                newInterval = math.clamp(newInterval, optimizedUpdateInterval.MinInterval, optimizedUpdateInterval.MaxInterval);

                seeker.CalculateInterval = newInterval;
            }
        }
    }
}