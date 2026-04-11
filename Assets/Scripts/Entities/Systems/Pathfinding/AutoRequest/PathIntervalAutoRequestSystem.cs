using Entities.Authoring.Pathfinding;
using Entities.Authoring.Pathfinding.AutoRequest;
using Unity.Burst;
using Unity.Entities;

namespace Entities.Systems.Pathfinding.AutoRequest
{
    [BurstCompile]
    public partial struct PathIntervalAutoRequestSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            MakeRequestsJob makeRequestsJob = new MakeRequestsJob()
            {
                ElapsedTime = (float)state.WorldUnmanaged.Time.ElapsedTime
            };

            state.Dependency = makeRequestsJob.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct MakeRequestsJob : IJobEntity
        {
            public float ElapsedTime;

            public void Execute(in MinAutoRequestInterval minAutoRequestInterval, in IntervalAutoRequest intervalAutoRequest, ref Seeker seeker, in Agent agent)
            {
                if (ElapsedTime < seeker.LastUpdateTime + minAutoRequestInterval.Value)
                    return;

                if (seeker.Status == PathStatus.Requested || seeker.Status == PathStatus.InProgress)
                    return;

                if (agent.ReachedEndOfPath || agent.ReachedDestination)
                    return;

                if (ElapsedTime > seeker.LastUpdateTime + intervalAutoRequest.Value)
                    seeker.Status = PathStatus.Requested;
            }
        }
    }
}