using System;
using Entities.Authoring.Pathfinding;
using Entities.Components;
using Pathfinding;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
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
        private const int InitialQueriesCount = 256;
        private const int MaxPathIterations = 128;

        private NativeList<NavMeshQuery> _navMeshQueries;
        private NativeQueue<int> _freeNavMeshQueryIndices;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TickCount>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();

            _navMeshQueries = new NativeList<NavMeshQuery>(InitialQueriesCount, Allocator.Persistent);
            _freeNavMeshQueryIndices = new NativeQueue<int>(Allocator.Persistent);

            for (int i = 0; i < _navMeshQueries.Length; i++)
                _navMeshQueries[i] = CreateNavMeshQuery();

            for (int i = 0; i < _navMeshQueries.Length; i++)
                _freeNavMeshQueryIndices.Enqueue(i);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            for (int i = 0; i < _navMeshQueries.Length; i++)
                _navMeshQueries[i].Dispose();

            _navMeshQueries.Dispose();
            _freeNavMeshQueryIndices.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EnsureQueriesSize(ref state);
            state.Dependency = AssignQuerryIndices(ref state, state.Dependency);
            state.Dependency = ProcessPathCalculation(ref state, state.Dependency);
            state.Dependency = ReturnFreeIndices(ref state, state.Dependency);
            NavMeshWorld.GetDefaultWorld().AddDependency(state.Dependency);
        }

        [BurstCompile]
        private NavMeshQuery CreateNavMeshQuery() => new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.Persistent, 65535);

        [BurstCompile]
        private void EnsureQueriesSize(ref SystemState state)
        {
            int pathfindersCount = SystemAPI.QueryBuilder().WithAll<PathFinder>().Build().CalculateEntityCount();

            if (_navMeshQueries.Capacity < pathfindersCount)
                _navMeshQueries.Capacity = pathfindersCount;

            int itemsToAdd = pathfindersCount - _navMeshQueries.Length;

            for (int i = 0; i < itemsToAdd; i++)
            {
                _navMeshQueries.Add(CreateNavMeshQuery());
                _freeNavMeshQueryIndices.Enqueue(_navMeshQueries.Length - 1);
            }
        }

        [BurstCompile]
        private JobHandle AssignQuerryIndices(ref SystemState state, JobHandle dependency)
        {
            EntityCommandBuffer endSimulationECB = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            AssignQueryIndicesJob assignQueryIndicesJob = new AssignQueryIndicesJob()
            {
                FreeIndices = _freeNavMeshQueryIndices,
                CommandBuffer = endSimulationECB
            };

            return assignQueryIndicesJob.Schedule(dependency);
        }

        [BurstCompile]
        private unsafe JobHandle ProcessPathCalculation(ref SystemState state, JobHandle dependency)
        {
            TickCount tickCount = SystemAPI.GetSingleton<TickCount>();

            ProcessPathCalculationJob processPathCalculationJob = new ProcessPathCalculationJob()
            {
                ElapsedTime = (float)state.WorldUnmanaged.Time.ElapsedTime,
                TickCount = tickCount,
                NavMeshQueriesPtr = _navMeshQueries.GetUnsafePtr()
            };
            return processPathCalculationJob.ScheduleParallel(dependency);
        }

        [BurstCompile]
        private JobHandle ReturnFreeIndices(ref SystemState state, JobHandle dependency)
        {
            EntityCommandBuffer endSimulationECB = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            ReturnFreeIndicesJob returnFreeIndicesJob = new ReturnFreeIndicesJob()
            {
                FreeIndices = _freeNavMeshQueryIndices.AsParallelWriter(),
                CommandBuffer = endSimulationECB.AsParallelWriter()
            };

            return returnFreeIndicesJob.ScheduleParallel(dependency);
        }

        [BurstCompile]
        [WithAll(typeof(PathFinder))]
        [WithNone(typeof(PathFinderQuerryIndex))]
        private partial struct AssignQueryIndicesJob : IJobEntity
        {
            public NativeQueue<int> FreeIndices;
            public EntityCommandBuffer CommandBuffer;

            public void Execute(Entity entity)
            {
                PathFinderQuerryIndex pathFinderQuerryIndex = new PathFinderQuerryIndex()
                {
                    Value = FreeIndices.Dequeue()
                };

                CommandBuffer.AddComponent(entity, pathFinderQuerryIndex);
            }
        }

        [BurstCompile]
        private unsafe partial struct ProcessPathCalculationJob : IJobEntity
        {
            public float ElapsedTime;
            public TickCount TickCount;

            [NativeDisableUnsafePtrRestriction] public NavMeshQuery* NavMeshQueriesPtr;

            public void Execute(in LocalToWorld localToWorld, in Agent agent, in Destination destination, ref PathFinder pathFinder,
                DynamicBuffer<PathWaypoint> pathWaypoints, in PathFinderQuerryIndex pathFinderQuerryIndex)
            {
                NavMeshQuery query = NavMeshQueriesPtr[pathFinderQuerryIndex.Value];

                if (pathFinder.Status == PathStatus.Requested)
                {
                    pathFinder.RequestStartPosition = localToWorld.Position;
                    pathFinder.RequestEndPosition = destination.Value;

                    if (math.distancesq(localToWorld.Position, destination.Value) < 0.0001f)
                    {
                        pathWaypoints.Clear();
                        pathWaypoints.Add(new PathWaypoint() { Value = pathFinder.RequestStartPosition });
                        pathWaypoints.Add(new PathWaypoint() { Value = pathFinder.RequestEndPosition });
                        pathFinder.LastCalculationTickCount = TickCount.Value;
                        pathFinder.LastCalculationTime = ElapsedTime;
                        pathFinder.QueryStatus = PathQueryStatus.Success;
                        pathFinder.Status = PathStatus.Success;
                        return;
                    }

                    float3 extents = new float3(10000);

                    NavMeshLocation startLocation = query.MapLocation(pathFinder.RequestStartPosition, extents, agent.AgentID);

                    if (query.IsValid(startLocation) == false)
                    {
                        pathWaypoints.Clear();
                        pathFinder.LastCalculationTickCount = TickCount.Value;
                        pathFinder.LastCalculationTime = ElapsedTime;
                        pathFinder.QueryStatus = PathQueryStatus.Failure;
                        pathFinder.Status = PathStatus.Failure;
                        return;
                    }

                    NavMeshLocation endLocation = query.MapLocation(pathFinder.RequestEndPosition, extents, agent.AgentID);

                    if (query.IsValid(endLocation) == false)
                    {
                        pathWaypoints.Clear();
                        pathFinder.LastCalculationTickCount = TickCount.Value;
                        pathFinder.LastCalculationTime = ElapsedTime;
                        pathFinder.QueryStatus = PathQueryStatus.Failure;
                        pathFinder.Status = PathStatus.Failure;
                        return;
                    }

                    PathQueryStatus status = query.BeginFindPath(startLocation, endLocation);

                    if (status != PathQueryStatus.InProgress && status != PathQueryStatus.Success)
                    {
                        pathWaypoints.Clear();
                        pathFinder.LastCalculationTickCount = TickCount.Value;
                        pathFinder.LastCalculationTime = ElapsedTime;
                        pathFinder.QueryStatus = status;
                        pathFinder.Status = PathStatus.Failure;
                        return;
                    }

                    pathFinder.Status = PathStatus.InProgress;
                    pathFinder.QueryStatus = PathQueryStatus.InProgress;
                    pathFinder.NavMeshStartPosition = startLocation.position;
                    pathFinder.NavMeshEndPosition = endLocation.position;
                    return;
                }

                if (pathFinder.Status == PathStatus.InProgress)
                {
                    PathQueryStatus status = query.UpdateFindPath(MaxPathIterations, out _);

                    if (status != PathQueryStatus.InProgress && status != PathQueryStatus.Success)
                    {
                        pathWaypoints.Clear();
                        pathFinder.LastCalculationTickCount = TickCount.Value;
                        pathFinder.LastCalculationTime = ElapsedTime;
                        pathFinder.Status = PathStatus.Failure;
                        pathFinder.QueryStatus = status;
                        return;
                    }

                    if (status == PathQueryStatus.InProgress)
                        return;

                    status = query.EndFindPath(out int pathSize);

                    if (status != PathQueryStatus.Success)
                    {
                        pathWaypoints.Clear();
                        pathFinder.LastCalculationTickCount = TickCount.Value;
                        pathFinder.LastCalculationTime = ElapsedTime;
                        pathFinder.Status = PathStatus.Failure;
                        pathFinder.QueryStatus = status;
                        return;
                    }

                    if (pathSize < 2)
                    {
                        pathWaypoints.Clear();
                        pathWaypoints.Add(new PathWaypoint { Value = pathFinder.RequestStartPosition });
                        pathWaypoints.Add(new PathWaypoint { Value = pathFinder.RequestEndPosition });
                        pathFinder.LastCalculationTickCount = TickCount.Value;
                        pathFinder.LastCalculationTime = ElapsedTime;
                        pathFinder.Status = PathStatus.Success;
                        pathFinder.QueryStatus = PathQueryStatus.Success;
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
                            pathFinder.NavMeshStartPosition,
                            pathFinder.NavMeshEndPosition,
                            polygonIds,
                            pathSize,
                            ref result,
                            ref flags,
                            ref vertexSize,
                            ref straightPathCount,
                            pathSize);

                    if ((status & PathQueryStatus.Success) == 0)
                    {
                        pathWaypoints.Clear();
                        pathFinder.LastCalculationTickCount = TickCount.Value;
                        pathFinder.LastCalculationTime = ElapsedTime;
                        pathFinder.Status = PathStatus.Failure;
                        pathFinder.QueryStatus = status;
                        return;
                    }

                    pathWaypoints.Clear();

                    for (int i = 0; i < result.Length; i++)
                    {
                        NavMeshLocation location = result[i];

                        if (location.position == Vector3.zero)
                            continue;

                        PathWaypoint waypoint = new PathWaypoint
                        {
                            Value = location.position
                        };

                        pathWaypoints.Add(waypoint);
                    }

                    DisposeTempCollections();
                    pathFinder.LastCalculationTickCount = TickCount.Value;
                    pathFinder.LastCalculationTime = ElapsedTime;
                    pathFinder.Status = PathStatus.Success;
                    pathFinder.QueryStatus = PathQueryStatus.Success;
                }
            }
        }

        [BurstCompile]
        [WithNone(typeof(LocalTransform))]
        private partial struct ReturnFreeIndicesJob : IJobEntity
        {
            public NativeQueue<int>.ParallelWriter FreeIndices;
            public EntityCommandBuffer.ParallelWriter CommandBuffer;

            public void Execute([EntityIndexInQuery] int querryIndex, in PathFinderQuerryIndex pathFinderQuerryIndex, Entity entity)
            {
                FreeIndices.Enqueue(pathFinderQuerryIndex.Value);
                CommandBuffer.RemoveComponent<PathFinderQuerryIndex>(querryIndex, entity);
            }
        }
    }
}