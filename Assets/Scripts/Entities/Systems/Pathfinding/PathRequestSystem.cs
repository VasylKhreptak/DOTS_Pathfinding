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
            MakePathRequestsOnTimeJob job = new MakePathRequestsOnTimeJob()
            {
                ElapsedTime = (float)state.WorldUnmanaged.Time.ElapsedTime
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct MakePathRequestsOnTimeJob : IJobEntity
        {
            public float ElapsedTime;

            public void Execute(ref PathFinder pathFinder)
            {
                if (pathFinder.Status != PathStatus.InProgress && ElapsedTime > pathFinder.LastCalculationTime + pathFinder.CalculateInterval)
                    pathFinder.Status = PathStatus.Requested;
            }
        }
    }
}