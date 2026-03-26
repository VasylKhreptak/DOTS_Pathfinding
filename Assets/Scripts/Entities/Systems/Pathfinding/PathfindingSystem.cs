using System;
using Entities.Authoring.Pathfinding;
using Entities.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.AI;

namespace Entities.Systems.Pathfinding
{
    [DisableAutoCreation]
    public partial struct PathfindingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TickCount>();
        }

        [Obsolete("Obsolete")]
        public void OnUpdate(ref SystemState state)
        {
            TickCount tickCount = SystemAPI.GetSingleton<TickCount>();

            foreach ((RefRO<LocalToWorld> localToWorld, RefRO<Destination> destination, RefRW<PathFinder> pathFinder, DynamicBuffer<PathWaypoint> waypointsBuffer,
                         RefRO<Agent> agent) in
                     SystemAPI.Query<RefRO<LocalToWorld>, RefRO<Destination>, RefRW<PathFinder>, DynamicBuffer<PathWaypoint>, RefRO<Agent>>())
            {
                if (state.WorldUnmanaged.Time.ElapsedTime > pathFinder.ValueRO.LastCalculationTime + pathFinder.ValueRO.CalculateInterval)
                {
                    pathFinder.ValueRW.LastCalculationTime = (float)state.WorldUnmanaged.Time.ElapsedTime;
                    pathFinder.ValueRW.LastCalculationTickCount = tickCount.Value;
                }
                else
                    continue;

                NavMeshQuery query = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), state.WorldUpdateAllocator, 10000);

                float3 extents = new float3(10000);

                NavMeshLocation startLocation = query.MapLocation(localToWorld.ValueRO.Position, extents, agent.ValueRO.AgentID);
                NavMeshLocation endLocation = query.MapLocation(destination.ValueRO.Value, extents, agent.ValueRO.AgentID);

                if (!query.IsValid(startLocation) || !query.IsValid(endLocation))
                {
                    Debug.LogError("Is valid false");
                    waypointsBuffer.Clear();
                    continue;
                }

                PathQueryStatus status = query.BeginFindPath(startLocation, endLocation);

                if (status != PathQueryStatus.InProgress && status != PathQueryStatus.Success)
                {
                    Debug.LogError("Status:  " + status);
                    waypointsBuffer.Clear();
                    continue;
                }

                status = query.UpdateFindPath(10000, out int pathSize);

                if (status != PathQueryStatus.Success)
                {
                    Debug.LogError("Status:  " + status);
                    waypointsBuffer.Clear();
                    continue;
                }

                status = query.EndFindPath(out pathSize);

                if ((status & PathQueryStatus.Success) == 0)
                {
                    Debug.LogError("Status:  " + status);
                    waypointsBuffer.Clear();
                    continue;
                }

                if (pathSize < 2)
                {
                    waypointsBuffer.Clear();
                    waypointsBuffer.Add(new PathWaypoint { Value = startLocation.position });
                    waypointsBuffer.Add(new PathWaypoint { Value = endLocation.position });
                    continue;
                }

                NativeArray<NavMeshLocation> result = CollectionHelper.CreateNativeArray<NavMeshLocation>(pathSize, state.WorldUpdateAllocator);
                NativeArray<StraightPathFlags> flags = CollectionHelper.CreateNativeArray<StraightPathFlags>(pathSize, state.WorldUpdateAllocator);
                NativeArray<float> vertexSize = CollectionHelper.CreateNativeArray<float>(pathSize, state.WorldUpdateAllocator);
                NativeArray<PolygonId> polygonIds = CollectionHelper.CreateNativeArray<PolygonId>(pathSize + 1, state.WorldUpdateAllocator);

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
                    Debug.LogError("Status:  " + status);
                    waypointsBuffer.Clear();
                    continue;
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
            }
        }
    }
}