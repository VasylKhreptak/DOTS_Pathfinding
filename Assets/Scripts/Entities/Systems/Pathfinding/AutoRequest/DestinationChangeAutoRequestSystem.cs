using Entities.Authoring.Pathfinding;
using Entities.Authoring.Pathfinding.AutoRequest;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Entities.Systems.Pathfinding.AutoRequest
{
    [BurstCompile]
    [DisableAutoCreation]
    public partial struct DestinationChangeAutoRequestSystem : ISystem
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
                in DestinationChangeAutoRequest destinationChangeAutoRequest, ref Seeker seeker, in Destination destination, in Agent agent)
            {
                if (ElapsedTime < seeker.LastUpdateTime + minAutoRequestInterval.Value)
                    return;

                if (seeker.Status == PathStatus.Requested || seeker.Status == PathStatus.InProgress)
                    return;

                if (agent.ReachedEndOfPath || agent.ReachedDestination)
                    return;

                float3 lastPathUpdateDestination = seeker.RequestEndPosition;

                float minDistanceSq = destinationChangeAutoRequest.MinDistance * destinationChangeAutoRequest.MinDistance;
                float maxDistanceSq = destinationChangeAutoRequest.MaxDistance * destinationChangeAutoRequest.MaxDistance;

                float distanceSq = math.distancesq(destination.Value, lastPathUpdateDestination);

                float threshold = math.remap(minDistanceSq, maxDistanceSq, destinationChangeAutoRequest.MinThreshold, destinationChangeAutoRequest.MaxThreshold,
                    distanceSq);

                threshold = math.clamp(threshold, destinationChangeAutoRequest.MinThreshold, destinationChangeAutoRequest.MaxThreshold);

                if (distanceSq > threshold * threshold)
                    seeker.Status = PathStatus.Requested;
            }
        }
    }
}