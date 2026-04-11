using Entities.Authoring.Pathfinding;
using Entities.Authoring.Pathfinding.AutoRequest;
using Plugins.Extensions;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.AI;
using NavMeshObstacle = Entities.Bakers.Pathfinding.NavMeshObstacle;

namespace Entities.Systems.Pathfinding.AutoRequest
{
    [BurstCompile]
    public partial struct NavMeshObstacleOverlapAutoRequestSystem : ISystem
    {
        private EntityQuery _obstacleQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _obstacleQuery = SystemAPI.QueryBuilder()
                .WithAll<NavMeshObstacle, LocalToWorld>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            NativeArray<NavMeshObstacle> obstacles = _obstacleQuery.ToComponentDataArray<NavMeshObstacle>(state.WorldUpdateAllocator);
            NativeArray<LocalToWorld> obstacleLocalToWorlds = _obstacleQuery.ToComponentDataArray<LocalToWorld>(state.WorldUpdateAllocator);

            MakeRequestsJob makeRequestsJob = new MakeRequestsJob()
            {
                ElapsedTime = (float)state.WorldUnmanaged.Time.ElapsedTime
            };

            state.Dependency = makeRequestsJob.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct MakeRequestsJob : IJobEntity
        {
            [ReadOnly] public NativeArray<NavMeshObstacle> Obstacles;
            [ReadOnly] public NativeArray<LocalToWorld> ObstacleLocalToWorlds;

            public float ElapsedTime;

            public void Execute(in MinAutoRequestInterval minAutoRequestInterval, in NavMeshObstacleOverlapAutoRequest navMeshObstacleOverlapAutoRequest,
                ref Seeker seeker, in DynamicBuffer<PathCorner> pathCorners)
            {
                if (ElapsedTime < seeker.LastUpdateTime + minAutoRequestInterval.Value)
                    return;

                if (ElapsedTime < seeker.LastUpdateTime + navMeshObstacleOverlapAutoRequest.MinInterval)
                    return;

                if (seeker.Status == PathStatus.Requested || seeker.Status == PathStatus.InProgress)
                    return;

                if (IsPathOverlappingWithNavMeshObstacles(in pathCorners))
                    seeker.Status = PathStatus.Requested;
            }

            private bool IsPathOverlappingWithNavMeshObstacles(in DynamicBuffer<PathCorner> pathCorners)
            {
                for (int j = 0; j < Obstacles.Length; j++)
                {
                    NavMeshObstacle obstacle = Obstacles[j];
                    LocalToWorld obstacleLocalToWorld = ObstacleLocalToWorlds[j];

                    AABB aabb = new AABB
                    {
                        Center = obstacle.Center,
                        Extents = obstacle.Shape == NavMeshObstacleShape.Box
                            ? obstacle.Size * 0.5f
                            : new float3(obstacle.Radius, obstacle.Height * 0.5f, obstacle.Radius)
                    }.ToWorld(obstacleLocalToWorld.Value);

                    float3 min = aabb.Center - aabb.Extents;
                    float3 max = aabb.Center + aabb.Extents;

                    for (int i = 1; i < pathCorners.Length; i++)
                    {
                        float3 startLinePosition = pathCorners[i - 1].Value;
                        float3 endLinePosition = pathCorners[i].Value;

                        if (LineIntersectsAABB(startLinePosition, endLinePosition, min, max))
                            return true;
                    }
                }

                return false;
            }

            private bool LineIntersectsAABB(float3 start, float3 end, float3 min, float3 max)
            {
                float3 dir = end - start;

                float tMin = 0f;
                float tMax = 1f;

                if (!IntersectAxis(start.x, dir.x, min.x, max.x, ref tMin, ref tMax))
                    return false;

                if (!IntersectAxis(start.y, dir.y, min.y, max.y, ref tMin, ref tMax))
                    return false;

                if (!IntersectAxis(start.z, dir.z, min.z, max.z, ref tMin, ref tMax))
                    return false;

                return true;
            }

            private bool IntersectAxis(float start, float dir, float min, float max, ref float tMin, ref float tMax)
            {
                if (math.abs(dir) < 1e-6f)
                {
                    return start >= min && start <= max;
                }

                float inv = 1f / dir;

                float t1 = (min - start) * inv;
                float t2 = (max - start) * inv;

                if (t1 > t2)
                    (t1, t2) = (t2, t1);

                tMin = math.max(tMin, t1);
                tMax = math.min(tMax, t2);

                return tMin <= tMax;
            }
        }
    }
}