using System;
using Entities.Authoring.Pathfinding;
using Entities.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.AI;

namespace Entities.Systems.Pathfinding
{
    [BurstCompile]
    [DisableAutoCreation]
    [Obsolete("Obsolete")]
    public partial struct PathfindingSystem : ISystem
    {
        private NavMeshQuery _navMeshQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TickCount>();

            _navMeshQuery = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), state.WorldUpdateAllocator, 10000);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            _navMeshQuery.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            TickCount tickCount = SystemAPI.GetSingleton<TickCount>();

            PathfindingJob job = new PathfindingJob()
            {
                ElapsedTime = (float)state.WorldUnmanaged.Time.ElapsedTime,
                TickCount = tickCount,
                NavMeshQuery = _navMeshQuery
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct PathfindingJob : IJobEntity
        {
            public float ElapsedTime;
            public TickCount TickCount;
            [ReadOnly] public NativeArray<NavMeshQuery> _navMeshQueries;

            public void Execute(in LocalToWorld localToWorld, in Destination destination, ref PathFinder pathFinder, DynamicBuffer<PathWaypoint> waypointsBuffer,
                in Agent agent)
            {
                if (ElapsedTime > pathFinder.LastCalculationTime + pathFinder.CalculateInterval)
                {
                    pathFinder.LastCalculationTime = ElapsedTime;
                    pathFinder.LastCalculationTickCount = TickCount.Value;
                }
                else
                    return;

                float3 extents = new float3(10000);

                NavMeshLocation startLocation = NavMeshQuery.MapLocation(localToWorld.Position, extents, agent.AgentID);
                NavMeshLocation endLocation = NavMeshQuery.MapLocation(destination.Value, extents, agent.AgentID);

                if (!NavMeshQuery.IsValid(startLocation) || !NavMeshQuery.IsValid(endLocation))
                {
                    waypointsBuffer.Clear();
                    return;
                }

                PathQueryStatus status = NavMeshQuery.BeginFindPath(startLocation, endLocation);

                if (status != PathQueryStatus.InProgress && status != PathQueryStatus.Success)
                {
                    waypointsBuffer.Clear();
                    return;
                }

                status = NavMeshQuery.UpdateFindPath(10000, out int pathSize);

                if (status != PathQueryStatus.Success)
                {
                    waypointsBuffer.Clear();
                    return;
                }

                status = NavMeshQuery.EndFindPath(out pathSize);

                if ((status & PathQueryStatus.Success) == 0)
                {
                    waypointsBuffer.Clear();
                    return;
                }

                if (pathSize < 2)
                {
                    waypointsBuffer.Clear();
                    waypointsBuffer.Add(new PathWaypoint { Value = startLocation.position });
                    waypointsBuffer.Add(new PathWaypoint { Value = endLocation.position });
                    return;
                }

                NativeArray<NavMeshLocation> result = new NativeArray<NavMeshLocation>(pathSize, Allocator.Temp);
                NativeArray<StraightPathFlags> flags = new NativeArray<StraightPathFlags>(pathSize, Allocator.Temp);
                NativeArray<float> vertexSize = new NativeArray<float>(pathSize, Allocator.Temp);
                NativeArray<PolygonId> polygonIds = new NativeArray<PolygonId>(pathSize + 1, Allocator.Temp);

                void DisposeTempCollections()
                {
                    result.Dispose();
                    flags.Dispose();
                    vertexSize.Dispose();
                    polygonIds.Dispose();
                }

                int straightPathCount = 0;

                NavMeshQuery.GetPathResult(polygonIds);

                status = PathUtils
                    .FindStraightPath(NavMeshQuery,
                        startLocation.position,
                        endLocation.position,
                        polygonIds,
                        pathSize,
                        ref result,
                        ref flags,
                        ref vertexSize,
                        ref straightPathCount,
                        pathSize);

                if (status != PathQueryStatus.Success)
                {
                    waypointsBuffer.Clear();
                    DisposeTempCollections();
                    return;
                }

                waypointsBuffer.Clear();

                for (int i = 0; i < result.Length; i++)
                {
                    NavMeshLocation location = result[i];

                    if (location.position == Vector3.zero)
                        continue;

                    PathWaypoint waypoint = new PathWaypoint
                    {
                        Value = location.position
                    };

                    waypointsBuffer.Add(waypoint);
                }

                DisposeTempCollections();
            }
        }
    }
}