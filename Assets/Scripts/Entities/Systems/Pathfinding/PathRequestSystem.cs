using Entities.Authoring.Pathfinding;
using Unity.Burst;
using Unity.Entities;

namespace Entities.Systems.Pathfinding
{
    [BurstCompile]
    [DisableAutoCreation]
    public partial struct PathRequestSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            MakePathRequestsOnTimeJob job = new MakePathRequestsOnTimeJob
            {
                ElapsedTime = (float)state.WorldUnmanaged.Time.ElapsedTime
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct MakePathRequestsOnTimeJob : IJobEntity
        {
            public float ElapsedTime;

            public void Execute(ref Seeker seeker, in Agent agent)
            {
                if (seeker.Status == PathStatus.Requested || seeker.Status == PathStatus.InProgress)
                    return;

                if (agent.ReachedEndOfPath || agent.ReachedDestination)
                    return;

                if (ElapsedTime > seeker.LastCalculationTime + seeker.CalculateInterval)
                    seeker.Status = PathStatus.Requested;
            }
        }
    }
}