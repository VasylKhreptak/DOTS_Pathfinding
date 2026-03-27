using System;
using Entities.Authoring.Pathfinding;
using Entities.Components;
using Pathfinding;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;
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
        private NativeArray<NavMeshQuery> _navMeshQueries;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TickCount>();

            _navMeshQueries = new NativeArray<NavMeshQuery>(JobsUtility.MaxJobThreadCount, Allocator.Persistent);

            for (int i = 0; i < _navMeshQueries.Length; i++)
                _navMeshQueries[i] = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.Persistent, 10000);
        }

        public void OnDestroy(ref SystemState state)
        {
            for (int i = 0; i < _navMeshQueries.Length; i++)
                _navMeshQueries[i].Dispose();

            _navMeshQueries.Dispose();
        }

        [BurstCompile]
        public unsafe void OnUpdate(ref SystemState state)
        {
            TickCount tickCount = SystemAPI.GetSingleton<TickCount>();

            PathfindingJob job = new PathfindingJob
            {
                ElapsedTime = (float)state.WorldUnmanaged.Time.ElapsedTime,
                TickCount = tickCount,
                NavMeshQueriesPtr = (NavMeshQuery*)_navMeshQueries.GetUnsafePtr()
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
            NavMeshWorld.GetDefaultWorld().AddDependency(state.Dependency);
        }

        [BurstCompile]
        private unsafe partial struct PathfindingJob : IJobEntity
        {
            public float ElapsedTime;
            public TickCount TickCount;

            [NativeDisableUnsafePtrRestriction] public NavMeshQuery* NavMeshQueriesPtr;

            [NativeSetThreadIndex] private int _threadIndex;

            public void Execute(in LocalToWorld localToWorld, in Destination destination, ref PathFinder pathFinder, DynamicBuffer<PathWaypoint> waypointsBuffer,
                in Agent agent)
            {
                if (pathFinder.ForceCalculate || ElapsedTime > pathFinder.LastCalculationTime + pathFinder.CalculateInterval)
                {
                    pathFinder.LastCalculationTime = ElapsedTime;
                    pathFinder.LastCalculationTickCount = TickCount.Value;
                    pathFinder.ForceCalculate = false;
                }
                else
                    return;

                if (math.distancesq(localToWorld.Position, destination.Value) < 0.0001f)
                {
                    waypointsBuffer.Clear();
                    waypointsBuffer.Add(new PathWaypoint() { Value = localToWorld.Position });
                    waypointsBuffer.Add(new PathWaypoint() { Value = destination.Value });
                    return;
                }

                NavMeshQuery query = NavMeshQueriesPtr[_threadIndex];

                float3 extents = new float3(10000);

                NavMeshLocation startLocation = query.MapLocation(localToWorld.Position, extents, agent.AgentID);

                if (query.IsValid(startLocation) == false)
                {
                    waypointsBuffer.Clear();
                    return;
                }

                NavMeshLocation endLocation = query.MapLocation(destination.Value, extents, agent.AgentID);

                if (!query.IsValid(endLocation))
                {
                    waypointsBuffer.Clear();
                    return;
                }

                float3 extentsTmp = new float3(5f);
                NavMeshLocation startLocationTmp = query.MapLocation(localToWorld.Position, extentsTmp, agent.AgentID);
                NavMeshLocation endLocationTmp = query.MapLocation(destination.Value, extentsTmp, agent.AgentID);

                if (query.IsValid(startLocationTmp) == false && query.IsValid(endLocationTmp) == false)
                {
                    waypointsBuffer.Clear();
                    waypointsBuffer.Add(new PathWaypoint() { Value = localToWorld.Position });
                    waypointsBuffer.Add(new PathWaypoint() { Value = destination.Value });
                    return;
                }

                PathQueryStatus status = query.BeginFindPath(startLocation, endLocation);

                if (status != PathQueryStatus.InProgress && status != PathQueryStatus.Success)
                {
                    waypointsBuffer.Clear();
                    return;
                }

                status = query.UpdateFindPath(10000, out int pathSize);

                if (status != PathQueryStatus.Success)
                {
                    waypointsBuffer.Clear();
                    return;
                }

                status = query.EndFindPath(out pathSize);

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

                query.GetPathResult(polygonIds);

                status = PathUtils
                    .FindStraightPath(query,
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