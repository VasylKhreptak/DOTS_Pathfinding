using System;
using Entities.Authoring.Pathfinding;
using Entities.Components;
using Entities.Components.Pathfinding;
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
        private NativeList<NavMeshQuery> _navMeshQueries;
        private NativeQueue<int> _freeNavMeshQueryIndices;
        private NativeArray<int> _pathFindersCountParallelCounter;
        private NativeArray<int> _requestedPathsParallelCounter;
        private NativeArray<int> _inProgressPathsParallelCounter;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<PathfindingSettings>() == false)
                state.EntityManager.CreateSingleton(PathfindingSettings.Default);

            PathfindingSettings settings = SystemAPI.GetSingleton<PathfindingSettings>();

            _navMeshQueries = new NativeList<NavMeshQuery>(settings.InitialNavMeshQueriesBufferSize, Allocator.Persistent);
            _freeNavMeshQueryIndices = new NativeQueue<int>(Allocator.Persistent);

            _pathFindersCountParallelCounter = new NativeArray<int>(JobsUtility.ThreadIndexCount, Allocator.Persistent);
            _requestedPathsParallelCounter = new NativeArray<int>(JobsUtility.ThreadIndexCount, Allocator.Persistent);
            _inProgressPathsParallelCounter = new NativeArray<int>(JobsUtility.ThreadIndexCount, Allocator.Persistent);

            for (int i = 0; i < _navMeshQueries.Length; i++)
                _navMeshQueries[i] = CreateNavMeshQuery(settings.PathNodePoolSize);

            for (int i = 0; i < _navMeshQueries.Length; i++)
                _freeNavMeshQueryIndices.Enqueue(i);

            if (SystemAPI.HasSingleton<PathfindingSystemData>() == false)
                state.EntityManager.CreateSingleton<PathfindingSystemData>();

            state.RequireForUpdate<TickCount>();
            state.RequireForUpdate<PathfindingSystemData>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PathfindingSettings>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            for (int i = 0; i < _navMeshQueries.Length; i++)
                _navMeshQueries[i].Dispose();

            _navMeshQueries.Dispose();
            _freeNavMeshQueryIndices.Dispose();

            _pathFindersCountParallelCounter.Dispose();
            _requestedPathsParallelCounter.Dispose();
            _inProgressPathsParallelCounter.Dispose();

            if (SystemAPI.TryGetSingletonEntity<PathfindingSystemData>(out Entity entity))
                state.EntityManager.DestroyEntity(entity);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EnsureQueriesSize(ref state);
            state.Dependency = AssignQuerryIndices(ref state, state.Dependency);
            state.Dependency = ProcessPathCalculation(ref state, state.Dependency);
            state.Dependency = ReturnFreeIndices(ref state, state.Dependency);
            state.Dependency = UpdatePathfindingSystemData(ref state, state.Dependency);
        }

        [BurstCompile]
        private NavMeshQuery CreateNavMeshQuery(int pathNodePoolSize) => new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.Persistent, pathNodePoolSize);

        [BurstCompile]
        private void EnsureQueriesSize(ref SystemState state)
        {
            int pathfindersCount = SystemAPI.QueryBuilder().WithAll<PathFinder>().Build().CalculateEntityCount();

            if (_navMeshQueries.Capacity < pathfindersCount)
                _navMeshQueries.Capacity = pathfindersCount;

            int itemsToAdd = pathfindersCount - _navMeshQueries.Length;

            PathfindingSettings settings = SystemAPI.GetSingleton<PathfindingSettings>();

            for (int i = 0; i < itemsToAdd; i++)
            {
                _navMeshQueries.Add(CreateNavMeshQuery(settings.PathNodePoolSize));
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
            PathfindingSettings settings = SystemAPI.GetSingleton<PathfindingSettings>();

            ProcessPathCalculationJob processPathCalculationJob = new ProcessPathCalculationJob()
            {
                ElapsedTime = (float)state.WorldUnmanaged.Time.ElapsedTime,
                TickCount = tickCount,
                SystemData = SystemAPI.GetSingleton<PathfindingSystemData>(),
                Settings = settings,
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
        private JobHandle UpdatePathfindingSystemData(ref SystemState state, JobHandle dependency)
        {
            JobHandle updatePathFindersCountHandle = CalculatePathFindersCount(ref state, dependency);
            JobHandle updateRequestedPathsCountHandle = CalculateRequestedPathsCount(ref state, dependency);
            JobHandle updateInProgressPathsCountHandle = CalculateInProgressPathsCount(ref state, dependency);
            JobHandle calculationJobsHandle =
                JobHandle.CombineDependencies(updatePathFindersCountHandle, updateRequestedPathsCountHandle, updateInProgressPathsCountHandle);
            return UpdateSystemData(ref state, calculationJobsHandle);
        }

        [BurstCompile]
        private JobHandle CalculatePathFindersCount(ref SystemState state, JobHandle dependency)
        {
            for (int i = 0; i < _pathFindersCountParallelCounter.Length; i++)
                _pathFindersCountParallelCounter[i] = 0;

            CalculatePathfindersCountJob calculatePathfindersCountJob = new CalculatePathfindersCountJob()
            {
                ParallelCounter = _pathFindersCountParallelCounter
            };

            return calculatePathfindersCountJob.ScheduleParallel(dependency);
        }

        [BurstCompile]
        private JobHandle CalculateRequestedPathsCount(ref SystemState state, JobHandle dependency)
        {
            for (int i = 0; i < _requestedPathsParallelCounter.Length; i++)
                _requestedPathsParallelCounter[i] = 0;

            CalculateRequestedPathsCountJob calculateRequestedPathsCountJob = new CalculateRequestedPathsCountJob()
            {
                ParallelCounter = _requestedPathsParallelCounter
            };

            return calculateRequestedPathsCountJob.ScheduleParallel(dependency);
        }

        [BurstCompile]
        private JobHandle CalculateInProgressPathsCount(ref SystemState state, JobHandle dependency)
        {
            for (int i = 0; i < _inProgressPathsParallelCounter.Length; i++)
                _inProgressPathsParallelCounter[i] = 0;

            CalculateInProgressPathsCountJob calculateInProgressPathsCountJob = new CalculateInProgressPathsCountJob()
            {
                ParallelCounter = _inProgressPathsParallelCounter
            };

            return calculateInProgressPathsCountJob.ScheduleParallel(dependency);
        }

        [BurstCompile]
        private JobHandle UpdateSystemData(ref SystemState state, JobHandle dependency)
        {
            UpdateSystemDataJob updateSystemDataJob = new UpdateSystemDataJob()
            {
                PathFindersCounter = _pathFindersCountParallelCounter,
                RequestedPathsCounter = _requestedPathsParallelCounter,
                InProgressPathsCounter = _inProgressPathsParallelCounter
            };

            return updateSystemDataJob.ScheduleParallel(dependency);
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
            public PathfindingSystemData SystemData;
            public PathfindingSettings Settings;

            [NativeDisableUnsafePtrRestriction] public NavMeshQuery* NavMeshQueriesPtr;

            public void Execute(in LocalToWorld localToWorld, in Agent agent, in Destination destination, ref PathFinder pathFinder,
                DynamicBuffer<PathWaypoint> pathWaypoints, in PathFinderQuerryIndex pathFinderQuerryIndex)
            {
                NavMeshQuery query = NavMeshQueriesPtr[pathFinderQuerryIndex.Value];

                if (pathFinder.Status == PathStatus.Requested && SystemData.SkipNewRequests == false)
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
                        pathFinder.Status = PathStatus.Failure;
                        return;
                    }

                    NavMeshLocation endLocation = query.MapLocation(pathFinder.RequestEndPosition, extents, agent.AgentID);

                    if (query.IsValid(endLocation) == false)
                    {
                        pathWaypoints.Clear();
                        pathFinder.LastCalculationTickCount = TickCount.Value;
                        pathFinder.LastCalculationTime = ElapsedTime;
                        pathFinder.Status = PathStatus.Failure;
                        return;
                    }

                    PathQueryStatus status = query.BeginFindPath(startLocation, endLocation);

                    if (status != PathQueryStatus.InProgress && status != PathQueryStatus.Success)
                    {
                        pathWaypoints.Clear();
                        pathFinder.LastCalculationTickCount = TickCount.Value;
                        pathFinder.LastCalculationTime = ElapsedTime;
                        pathFinder.Status = PathStatus.Failure;
                        return;
                    }

                    pathFinder.Status = PathStatus.InProgress;
                    pathFinder.NavMeshStartPosition = startLocation.position;
                    pathFinder.NavMeshEndPosition = endLocation.position;
                    return;
                }

                if (pathFinder.Status == PathStatus.InProgress)
                {
                    PathQueryStatus status = query.UpdateFindPath(Settings.MaxPathIterations, out _);

                    if (status != PathQueryStatus.InProgress && status != PathQueryStatus.Success)
                    {
                        pathWaypoints.Clear();
                        pathFinder.LastCalculationTickCount = TickCount.Value;
                        pathFinder.LastCalculationTime = ElapsedTime;
                        pathFinder.Status = PathStatus.Failure;
                        return;
                    }

                    if (status == PathQueryStatus.InProgress)
                        return;

                    status = query.EndFindPath(out int pathSize);

                    if ((status & PathQueryStatus.Success) == 0)
                    {
                        pathWaypoints.Clear();
                        pathFinder.LastCalculationTickCount = TickCount.Value;
                        pathFinder.LastCalculationTime = ElapsedTime;
                        pathFinder.Status = PathStatus.Failure;
                        return;
                    }

                    if (pathSize < 2)
                    {
                        pathWaypoints.Clear();
                        pathFinder.LastCalculationTickCount = TickCount.Value;
                        pathFinder.LastCalculationTime = ElapsedTime;
                        pathFinder.Status = PathStatus.Failure;
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

        [BurstCompile]
        private partial struct CalculatePathfindersCountJob : IJobEntity
        {
            [NativeDisableParallelForRestriction] public NativeArray<int> ParallelCounter;

            [NativeSetThreadIndex] private int _threadIndex;

            public void Execute(in PathFinder pathFinder) => ParallelCounter[_threadIndex]++;
        }

        [BurstCompile]
        private partial struct CalculateRequestedPathsCountJob : IJobEntity
        {
            [NativeDisableParallelForRestriction] public NativeArray<int> ParallelCounter;

            [NativeSetThreadIndex] private int _threadIndex;

            public void Execute(in PathFinder pathFinder)
            {
                if (pathFinder.Status == PathStatus.Requested)
                    ParallelCounter[_threadIndex]++;
            }
        }

        [BurstCompile]
        private partial struct CalculateInProgressPathsCountJob : IJobEntity
        {
            [NativeDisableParallelForRestriction] public NativeArray<int> ParallelCounter;

            [NativeSetThreadIndex] private int _threadIndex;

            public void Execute(in PathFinder pathFinder)
            {
                if (pathFinder.Status == PathStatus.InProgress)
                    ParallelCounter[_threadIndex]++;
            }
        }

        [BurstCompile]
        private partial struct UpdateSystemDataJob : IJobEntity
        {
            [ReadOnly] public NativeArray<int> PathFindersCounter;
            [ReadOnly] public NativeArray<int> RequestedPathsCounter;
            [ReadOnly] public NativeArray<int> InProgressPathsCounter;

            public void Execute(ref PathfindingSystemData systemData)
            {
                int count = 0;
                for (int i = 0; i < PathFindersCounter.Length; i++)
                    count += PathFindersCounter[i];
                systemData.PathFindersCount = count;

                count = 0;
                for (int i = 0; i < RequestedPathsCounter.Length; i++)
                    count += RequestedPathsCounter[i];
                systemData.RequestedPathsCount = count;

                count = 0;
                for (int i = 0; i < InProgressPathsCounter.Length; i++)
                    count += InProgressPathsCounter[i];
                systemData.InProgressPathsCount = count;
            }
        }
    }
}