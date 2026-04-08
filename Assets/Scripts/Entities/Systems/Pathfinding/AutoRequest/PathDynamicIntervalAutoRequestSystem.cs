using Entities.Authoring.Pathfinding;
using Entities.Authoring.Pathfinding.AutoRequest;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Entities.Systems.Pathfinding.AutoRequest
{
    [BurstCompile]
    public partial struct PathDynamicIntervalAutoRequestSystem : ISystem
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

            public void Execute(in LocalToWorld localToWorld, in MinAutoRequestInterval minAutoRequestInterval,
                in PathDynamicIntervalAutoRequest pathDynamicIntervalAutoRequest, ref Seeker seeker,
                in Agent agent, in Destination destination)
            {
                if (ElapsedTime < seeker.LastUpdateTime + minAutoRequestInterval.Value)
                    return;

                if (agent.ReachedEndOfPath || agent.ReachedDestination)
                    return;

                if (seeker.Status == PathStatus.Requested || seeker.Status == PathStatus.InProgress)
                    return;

                float minDistanceSq = pathDynamicIntervalAutoRequest.MinDistance * pathDynamicIntervalAutoRequest.MinDistance;
                float maxDistanceSq = pathDynamicIntervalAutoRequest.MaxDistance * pathDynamicIntervalAutoRequest.MaxDistance;

                float distanceSq = math.distancesq(localToWorld.Position, destination.Value);

                float interval = math.remap(minDistanceSq, maxDistanceSq, pathDynamicIntervalAutoRequest.MinInterval, pathDynamicIntervalAutoRequest.MaxInterval,
                    distanceSq);

                interval = math.clamp(interval, pathDynamicIntervalAutoRequest.MinInterval, pathDynamicIntervalAutoRequest.MaxInterval);

                if (ElapsedTime > seeker.LastUpdateTime + interval)
                    seeker.Status = PathStatus.Requested;
            }
        }
    }
}