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
                in DynamicIntervalAutoRequest dynamicIntervalAutoRequest, ref Seeker seeker, in Destination destination)
            {
                if (ElapsedTime < seeker.LastUpdateTime + minAutoRequestInterval.Value)
                    return;

                if (seeker.Status == PathStatus.Requested || seeker.Status == PathStatus.InProgress)
                    return;

                float minDistanceSq = dynamicIntervalAutoRequest.MinDistance * dynamicIntervalAutoRequest.MinDistance;
                float maxDistanceSq = dynamicIntervalAutoRequest.MaxDistance * dynamicIntervalAutoRequest.MaxDistance;

                float distanceSq = math.distancesq(localToWorld.Position, destination.Value);

                float interval = math.remap(minDistanceSq, maxDistanceSq, dynamicIntervalAutoRequest.MinInterval, dynamicIntervalAutoRequest.MaxInterval,
                    distanceSq);

                interval = math.clamp(interval, dynamicIntervalAutoRequest.MinInterval, dynamicIntervalAutoRequest.MaxInterval);

                if (ElapsedTime > seeker.LastUpdateTime + interval)
                    seeker.Status = PathStatus.Requested;
            }
        }
    }
}