using System;
using Entities.Authoring.Pathfinding;
using Entities.Components;
using Pathfinding;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.AI;

namespace Entities.Systems.Pathfinding
{
    [DisableAutoCreation]
    [Obsolete("Obsolete")]
    public partial struct PathfindingSystem : ISystem
    {
        private const int InitialCapacity = 256;
        private const int MaxPathIterations = 128;

        private NativeList<NavMeshQuery> _navMeshQueries;
        private NativeList<PathRequest> _pathRequests;
        private NativeArray<int> _parallelCounter;

        private ComponentLookup<LocalToWorld> _localToWorldLookup;
        private ComponentLookup<PathFinder> _pathFinderLookup;
        private ComponentLookup<Agent> _agentLookup;
        private BufferLookup<PathWaypoint> _pathWaypointsLookup;
        private EntityStorageInfoLookup _entityLookup;
        private ComponentLookup<Destination> _destinationLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TickCount>();

            _navMeshQueries = new NativeList<NavMeshQuery>(InitialCapacity, Allocator.Persistent);
            _pathRequests = new NativeList<PathRequest>(InitialCapacity, Allocator.Persistent);
            _parallelCounter = new NativeArray<int>(JobsUtility.JobWorkerCount + 1, Allocator.Persistent);

            for (int i = 0; i < _navMeshQueries.Length; i++)
                _navMeshQueries[i] = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.Persistent, 1024);

            _localToWorldLookup = state.GetComponentLookup<LocalToWorld>(true);
            _pathFinderLookup = state.GetComponentLookup<PathFinder>();
            _agentLookup = state.GetComponentLookup<Agent>(true);
            _pathWaypointsLookup = state.GetBufferLookup<PathWaypoint>();
            _entityLookup = state.GetEntityStorageInfoLookup();
            _destinationLookup = state.GetComponentLookup<Destination>(true);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            for (int i = 0; i < _navMeshQueries.Length; i++)
                _navMeshQueries[i].Dispose();

            _navMeshQueries.Dispose();
            _pathRequests.Dispose();
            _parallelCounter.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            UpdateLookups(ref state);

            JobHandle makePathRequestsOnDemandHandle = MakePathRequestsOnTime(ref state, state.Dependency);
            int requestsCount = EnsureContainersCapacity(ref state, makePathRequestsOnDemandHandle);
            JobHandle updatePathRequestsHandle = UpdatePathRequests(ref state, requestsCount, state.Dependency);
            // JobHandle processPathRequestsHandle = ProcessPathRequests(updatePathRequestsHandle);
            // JobHandle cleanupPathRequestsHandle = CleanupPathRequests(ref state, updatePathRequestsHandle);
            state.Dependency = updatePathRequestsHandle;

            state.Dependency.Complete();

            Debug.LogError("Path requests count: " + _pathRequests.Length);
        }

        [BurstCompile]
        private void UpdateLookups(ref SystemState state)
        {
            _localToWorldLookup.Update(ref state);
            _pathFinderLookup.Update(ref state);
            _agentLookup.Update(ref state);
            _pathWaypointsLookup.Update(ref state);
            _entityLookup.Update(ref state);
            _destinationLookup.Update(ref state);
        }

        [BurstCompile]
        private JobHandle MakePathRequestsOnTime(ref SystemState state, JobHandle dependency)
        {
            MakePathRequestsOnTimeJob job = new MakePathRequestsOnTimeJob()
            {
                ElapsedTime = (float)state.WorldUnmanaged.Time.ElapsedTime
            };

            return job.ScheduleParallel(dependency);
        }

        [BurstCompile]
        private int EnsureContainersCapacity(ref SystemState state, JobHandle dependency)
        {
            for (int i = 0; i < _parallelCounter.Length; i++)
                _parallelCounter[i] = 0;

            CalculatePathRequestsCountJob job = new CalculatePathRequestsCountJob()
            {
                ParallelCounter = _parallelCounter
            };

            state.Dependency = job.ScheduleParallel(dependency);
            state.Dependency.Complete();

            int targetCapacity = 0;

            for (int i = 0; i < _parallelCounter.Length; i++)
                targetCapacity += _parallelCounter[i];

            if (_navMeshQueries.Capacity < targetCapacity)
            {
                _navMeshQueries.Capacity = targetCapacity;

                int itemsToAdd = targetCapacity - _navMeshQueries.Length;

                for (int i = 0; i < itemsToAdd; i++)
                    _navMeshQueries.Add(new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.Persistent, 1024));
            }

            if (_pathRequests.Capacity < targetCapacity)
                _pathRequests.Capacity = targetCapacity;

            return targetCapacity;
        }

        [BurstCompile]
        private JobHandle UpdatePathRequests(ref SystemState state, int requestsCount, JobHandle dependency)
        {
            UpdatePathRequestsJob updatePathRequestsJob = new UpdatePathRequestsJob()
            {
                PathRequests = _pathRequests,
                RequestsCount = requestsCount
            };

            return updatePathRequestsJob.Schedule(dependency);
        }

        [BurstCompile]
        private unsafe JobHandle ProcessPathRequests(JobHandle dependency)
        {
            ProcessPathRequestsJob processPathRequestsJob = new ProcessPathRequestsJob()
            {
                PathRequests = _pathRequests,
                NavMeshQueriesPtr = _navMeshQueries.GetUnsafePtr(),
                LocalToWorldLookup = _localToWorldLookup,
                PathFinderLookup = _pathFinderLookup,
                AgentLookup = _agentLookup,
                PathWaypointsLookup = _pathWaypointsLookup,
                EntityLookup = _entityLookup,
                DestinationLookup = _destinationLookup
            };

            return processPathRequestsJob.Schedule(_pathRequests.Length, 64, dependency);
        }

        private JobHandle CleanupPathRequests(ref SystemState state, JobHandle dependency)
        {
            CleanupPathRequestsJob job = new CleanupPathRequestsJob()
            {
                PathRequests = _pathRequests
            };

            return job.Schedule(dependency);
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

        [BurstCompile]
        private partial struct CalculatePathRequestsCountJob : IJobEntity
        {
            [NativeDisableParallelForRestriction] public NativeArray<int> ParallelCounter;

            [NativeSetThreadIndex] private int _threadIndex;

            public void Execute(ref PathFinder pathFinder)
            {
                if (pathFinder.Status == PathStatus.Requested)
                    ParallelCounter[_threadIndex]++;
            }
        }

        [BurstCompile]
        private partial struct UpdatePathRequestsJob : IJobEntity
        {
            public int RequestsCount;
            public NativeList<PathRequest> PathRequests;

            public void Execute(in LocalToWorld localToWorld, in Destination destination, ref PathFinder pathFinder, Entity entity)
            {
                // for (int i = 0; i < PathRequests.Length; i++)
                // {
                //     PathRequest request = PathRequests[i];
                //
                //     if (request.Entity == entity && request.Status == PathStatus.Requested)
                //     {
                //         request.StartPosition = localToWorld.Position;
                //         request.EndPosition = destination.Value;
                //         return;
                //     }
                // }

                if (pathFinder.Status == PathStatus.Requested)
                {
                    // int freeQuerryIndex = -1;
                    //
                    // for (int i = 0; i < RequestsCount; i++)
                    // {
                    //     bool isIndexFree = true;
                    //
                    //     for (int j = 0; j < PathRequests.Length; j++)
                    //     {
                    //         if (i == PathRequests[j].QueryIndex)
                    //         {
                    //             isIndexFree = false;
                    //             break;
                    //         }
                    //     }
                    //
                    //     if (isIndexFree)
                    //     {
                    //         freeQuerryIndex = i;
                    //         break;
                    //     }
                    // }

                    PathRequest pathRequest = new PathRequest()
                    {
                        StartPosition = localToWorld.Position,
                        EndPosition = destination.Value,
                        Entity = entity,
                        QueryIndex = -1,
                        Status = PathStatus.Requested
                    };

                    // PathRequests.AddNoResize(pathRequest);
                }
            }
        }

        [BurstCompile]
        private unsafe struct ProcessPathRequestsJob : IJobParallelFor
        {
            public float ElapsedTime;
            public TickCount TickCount;
            [NativeDisableParallelForRestriction] public NativeList<PathRequest> PathRequests;

            [ReadOnly] public ComponentLookup<LocalToWorld> LocalToWorldLookup;
            public ComponentLookup<PathFinder> PathFinderLookup;
            [ReadOnly] public ComponentLookup<Agent> AgentLookup;
            public BufferLookup<PathWaypoint> PathWaypointsLookup;
            public EntityStorageInfoLookup EntityLookup;
            [ReadOnly] public ComponentLookup<Destination> DestinationLookup;

            [NativeDisableUnsafePtrRestriction] public NavMeshQuery* NavMeshQueriesPtr;

            public void Execute(int index)
            {
                PathRequest pathRequest = PathRequests[index];

                if (EntityLookup.Exists(pathRequest.Entity) == false)
                    return;

                if (PathWaypointsLookup.TryGetBuffer(pathRequest.Entity, out DynamicBuffer<PathWaypoint> waypointsBuffer) == false)
                    return;

                RefRO<LocalToWorld> localToWorld = LocalToWorldLookup.GetRefRO(pathRequest.Entity);
                RefRW<PathFinder> pathfinder = PathFinderLookup.GetRefRW(pathRequest.Entity);
                RefRO<Agent> agent = AgentLookup.GetRefRO(pathRequest.Entity);
                RefRO<Destination> destination = DestinationLookup.GetRefRO(pathRequest.Entity);

                long tickCount = TickCount.Value;
                float elapsedTime = ElapsedTime;

                void UpdatePathfinderInfo(PathStatus status)
                {
                    pathfinder.ValueRW.LastCalculationTickCount = tickCount;
                    pathfinder.ValueRW.LastCalculationTime = elapsedTime;
                    pathfinder.ValueRW.Status = status;
                }

                NavMeshQuery query = NavMeshQueriesPtr[pathRequest.QueryIndex];

                if (pathRequest.Status == PathStatus.Requested)
                {
                    if (math.distancesq(localToWorld.ValueRO.Position, destination.ValueRO.Value) < 0.0001f)
                    {
                        waypointsBuffer.Clear();
                        waypointsBuffer.Add(new PathWaypoint() { Value = pathRequest.StartPosition });
                        waypointsBuffer.Add(new PathWaypoint() { Value = pathRequest.EndPosition });
                        UpdatePathfinderInfo(PathStatus.Success);
                        pathRequest.Status = PathStatus.Success;
                        PathRequests[index] = pathRequest;
                        return;
                    }

                    float3 extents = new float3(10000);

                    NavMeshLocation startLocation = query.MapLocation(pathRequest.StartPosition, extents, agent.ValueRO.AgentID);

                    if (query.IsValid(startLocation) == false)
                    {
                        waypointsBuffer.Clear();
                        UpdatePathfinderInfo(PathStatus.Failure);
                        pathRequest.Status = PathStatus.Failure;
                        PathRequests[index] = pathRequest;
                        return;
                    }

                    NavMeshLocation endLocation = query.MapLocation(pathRequest.EndPosition, extents, agent.ValueRO.AgentID);

                    if (query.IsValid(endLocation) == false)
                    {
                        waypointsBuffer.Clear();
                        UpdatePathfinderInfo(PathStatus.Failure);
                        pathRequest.Status = PathStatus.Failure;
                        PathRequests[index] = pathRequest;
                        return;
                    }

                    PathQueryStatus status = query.BeginFindPath(startLocation, endLocation);

                    if (status != PathQueryStatus.InProgress && status != PathQueryStatus.Success)
                    {
                        waypointsBuffer.Clear();
                        UpdatePathfinderInfo(PathStatus.Failure);
                        pathRequest.Status = PathStatus.Failure;
                        PathRequests[index] = pathRequest;
                        return;
                    }

                    pathfinder.ValueRW.Status = PathStatus.InProgress;
                    pathRequest.Status = PathStatus.InProgress;
                    pathRequest.NavMeshStartPosition = startLocation.position;
                    pathRequest.NavMeshEndPosition = endLocation.position;
                    PathRequests[index] = pathRequest;
                }

                if (pathRequest.Status == PathStatus.InProgress)
                {
                    PathQueryStatus status = query.UpdateFindPath(MaxPathIterations, out _);

                    if (status != PathQueryStatus.InProgress && status != PathQueryStatus.Success)
                    {
                        waypointsBuffer.Clear();
                        UpdatePathfinderInfo(PathStatus.Failure);
                        pathRequest.Status = PathStatus.Failure;
                        PathRequests[index] = pathRequest;
                        return;
                    }

                    if (status == PathQueryStatus.InProgress)
                        return;

                    status = query.EndFindPath(out int pathSize);

                    if ((status & PathQueryStatus.Success) == 0)
                    {
                        waypointsBuffer.Clear();
                        UpdatePathfinderInfo(PathStatus.Failure);
                        pathRequest.Status = PathStatus.Failure;
                        PathRequests[index] = pathRequest;
                        return;
                    }

                    if (pathSize < 2)
                    {
                        waypointsBuffer.Clear();
                        waypointsBuffer.Add(new PathWaypoint { Value = pathRequest.StartPosition });
                        waypointsBuffer.Add(new PathWaypoint { Value = pathRequest.EndPosition });
                        UpdatePathfinderInfo(PathStatus.Success);
                        pathRequest.Status = PathStatus.Success;
                        PathRequests[index] = pathRequest;
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
                            pathRequest.NavMeshStartPosition,
                            pathRequest.NavMeshEndPosition,
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
                        UpdatePathfinderInfo(PathStatus.Failure);
                        pathRequest.Status = PathStatus.Failure;
                        PathRequests[index] = pathRequest;
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
                    UpdatePathfinderInfo(PathStatus.Success);
                    pathRequest.Status = PathStatus.Success;
                    PathRequests[index] = pathRequest;
                }
            }
        }

        [BurstCompile]
        private struct CleanupPathRequestsJob : IJob
        {
            public NativeList<PathRequest> PathRequests;

            public void Execute()
            {
                for (int i = PathRequests.Length - 1; i >= 0; i--)
                    if (ShouldRemove(PathRequests[i]))
                        PathRequests.RemoveAtSwapBack(i);
            }

            private bool ShouldRemove(PathRequest request) => request.Status == PathStatus.Success || request.Status == PathStatus.Failure;
        }
    }

    [Obsolete("Obsolete")]
    public struct PathRequest
    {
        public float3 StartPosition;
        public float3 EndPosition;
        public float3 NavMeshStartPosition;
        public float3 NavMeshEndPosition;
        public Entity Entity;
        public PathStatus Status;
        public int QueryIndex;
    }
}