using System;
using Entities.Authoring.Pathfinding;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.AI;

namespace Entities.Systems.Pathdinding
{
    [DisableAutoCreation]
    public partial struct PathfindingSystem : ISystem
    {
        [Obsolete("Obsolete")]
        public void OnUpdate(ref SystemState state)
        {
            foreach ((RefRO<LocalToWorld> localToWorld, RefRO<Destination> destination, RefRW<PathFinder> pathFinder, DynamicBuffer<PathWaypoint> waypointsBuffer,
                         RefRO<Agent> agent) in
                     SystemAPI.Query<RefRO<LocalToWorld>, RefRO<Destination>, RefRW<PathFinder>, DynamicBuffer<PathWaypoint>, RefRO<Agent>>())
            {
                if (state.WorldUnmanaged.Time.ElapsedTime > pathFinder.ValueRO.LastCalculationTime + pathFinder.ValueRO.CalculateInterval)
                    pathFinder.ValueRW.LastCalculationTime = (float)state.WorldUnmanaged.Time.ElapsedTime;
                else
                    return;

                using NavMeshQuery query = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.TempJob, 10000);

                float3 extents = new float3(1);

                NavMeshLocation startLocation = query.MapLocation(localToWorld.ValueRO.Position, extents, agent.ValueRO.AgentID);
                NavMeshLocation endLocation = query.MapLocation(destination.ValueRO.Value, extents, agent.ValueRO.AgentID);

                if (query.IsValid(startLocation) == false || query.IsValid(endLocation) == false)
                {
                    Debug.LogError("Is valid false");
                    waypointsBuffer.Clear();
                    return;
                }

                PathQueryStatus status = query.BeginFindPath(startLocation, endLocation);

                if (status != PathQueryStatus.InProgress)
                {
                    Debug.LogError("Status:  " + status);
                    waypointsBuffer.Clear();
                    return;
                }

                status = query.UpdateFindPath(10000, out int pathSize);

                if (status != PathQueryStatus.Success)
                {
                    Debug.LogError("Status:  " + status);
                    waypointsBuffer.Clear();
                    return;
                }

                status = query.EndFindPath(out pathSize);

                if ((status & PathQueryStatus.Success) == 0)
                {
                    Debug.LogError("Status:  " + status);
                    waypointsBuffer.Clear();
                    return;
                }

                NativeArray<NavMeshLocation> result = new NativeArray<NavMeshLocation>(pathSize, Allocator.Temp);
                NativeArray<StraightPathFlags> flags = new NativeArray<StraightPathFlags>(pathSize, Allocator.Temp);
                NativeArray<float> vertexSize = new NativeArray<float>(pathSize, Allocator.Temp);
                NativeArray<PolygonId> polygonIds = new NativeArray<PolygonId>(pathSize + 1, Allocator.Temp);

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

                flags.Dispose();
                vertexSize.Dispose();
                polygonIds.Dispose();

                if (status != PathQueryStatus.Success)
                {
                    Debug.LogError("Status:  " + status);
                    waypointsBuffer.Clear();
                    result.Dispose();
                    return;
                }

                waypointsBuffer.Clear();

                for (int i = 0; i < result.Length; i++)
                {
                    NavMeshLocation location = result[i];

                    if (location.position == Vector3.zero)
                        continue;

                    PathWaypoint waypoint = new PathWaypoint()
                    {
                        Value = location.position
                    };

                    waypointsBuffer.Add(waypoint);
                }

                result.Dispose();
            }
        }
    }
}