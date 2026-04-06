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
using Seeker = Entities.Authoring.Pathfinding.Seeker;

namespace Entities.Systems.Pathfinding
{
    [DisableAutoCreation]
    [Obsolete("Obsolete")]
    public partial struct PathfindingSystem : ISystem
    {
        private NativeArray<NavMeshQuery> _navMeshQueries;
        private NativeQueue<int> _freeNavMeshQueryIndices;
        private NativeArray<int> _seekersParallelCounter;
        private NativeArray<int> _requestedPathsParallelCounter;
        private NativeArray<int> _inProgressPathsParallelCounter;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            if (!SystemAPI.HasSingleton<PathfindingSettings>())
                state.EntityManager.CreateSingleton(PathfindingSettings.Default);

            PathfindingSettings settings = SystemAPI.GetSingleton<PathfindingSettings>();

            _navMeshQueries = new NativeArray<NavMeshQuery>(settings.NavMeshQueriesBufferSize, Allocator.Persistent);
            _freeNavMeshQueryIndices = new NativeQueue<int>(Allocator.Persistent);

            _seekersParallelCounter = new NativeArray<int>(JobsUtility.ThreadIndexCount, Allocator.Persistent);
            _requestedPathsParallelCounter = new NativeArray<int>(JobsUtility.ThreadIndexCount, Allocator.Persistent);
            _inProgressPathsParallelCounter = new NativeArray<int>(JobsUtility.ThreadIndexCount, Allocator.Persistent);

            for (int i = 0; i < settings.NavMeshQueriesBufferSize; i++)
            {
                _navMeshQueries[i] = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.Persistent, settings.PathNodePoolSize);
                _freeNavMeshQueryIndices.Enqueue(i);
            }

            if (!SystemAPI.HasSingleton<PathfindingSystemData>())
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
            {
                _navMeshQueries[i].Dispose();
            }

            _navMeshQueries.Dispose();
            _freeNavMeshQueryIndices.Dispose();

            _seekersParallelCounter.Dispose();
            _requestedPathsParallelCounter.Dispose();
            _inProgressPathsParallelCounter.Dispose();

            if (SystemAPI.TryGetSingletonEntity<PathfindingSystemData>(out Entity entity))
                state.EntityManager.DestroyEntity(entity);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = CreateSeekerQueryIndexComponents(ref state, state.Dependency);
            state.Dependency = AssignQuerryIndices(ref state, state.Dependency);
            state.Dependency = ProcessPathCalculation(ref state, state.Dependency);
            state.Dependency = ReturnFreeIndices(ref state, state.Dependency);
            state.Dependency = UpdatePathfindingSystemData(ref state, state.Dependency);
        }

        [BurstCompile]
        private JobHandle CreateSeekerQueryIndexComponents(ref SystemState state, JobHandle dependency)
        {
            EntityCommandBuffer endSimulationECB = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            CreateSeekerQueryIndexComponentsJob job = new CreateSeekerQueryIndexComponentsJob()
            {
                CommandBuffer = endSimulationECB.AsParallelWriter()
            };

            return job.ScheduleParallel(dependency);
        }

        [BurstCompile]
        private JobHandle AssignQuerryIndices(ref SystemState state, JobHandle dependency)
        {
            AssignQueryIndicesJob assignQueryIndicesJob = new AssignQueryIndicesJob
            {
                FreeIndices = _freeNavMeshQueryIndices,
            };

            return assignQueryIndicesJob.Schedule(dependency);
        }

        [BurstCompile]
        private unsafe JobHandle ProcessPathCalculation(ref SystemState state, JobHandle dependency)
        {
            TickCount tickCount = SystemAPI.GetSingleton<TickCount>();
            PathfindingSettings settings = SystemAPI.GetSingleton<PathfindingSettings>();

            ProcessPathCalculationJob processPathCalculationJob = new ProcessPathCalculationJob
            {
                ElapsedTime = (float)state.WorldUnmanaged.Time.ElapsedTime,
                TickCount = tickCount,
                SystemData = SystemAPI.GetSingleton<PathfindingSystemData>(),
                Settings = settings,
                NavMeshQueriesPtr = (NavMeshQuery*)_navMeshQueries.GetUnsafePtr()
            };

            return processPathCalculationJob.ScheduleParallel(dependency);
        }

        [BurstCompile]
        private JobHandle ReturnFreeIndices(ref SystemState state, JobHandle dependency)
        {
            ReturnFreeIndicesFromStatus returnFreeIndicesFromStatusJob = new ReturnFreeIndicesFromStatus()
            {
                FreeIndices = _freeNavMeshQueryIndices.AsParallelWriter()
            };

            JobHandle freeIndicesFromStatusHandle = returnFreeIndicesFromStatusJob.ScheduleParallel(dependency);

            EntityCommandBuffer endSimulationECB = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            ReturnFreeIndicesFromDestroyedEntitiesJob returnFreeIndicesFromDestroyedEntitiesJob = new ReturnFreeIndicesFromDestroyedEntitiesJob
            {
                FreeIndices = _freeNavMeshQueryIndices.AsParallelWriter(),
                CommandBuffer = endSimulationECB.AsParallelWriter()
            };

            JobHandle freeIndicesFromDestroyedEntitiesHandle = returnFreeIndicesFromDestroyedEntitiesJob.ScheduleParallel(dependency);

            return JobHandle.CombineDependencies(freeIndicesFromDestroyedEntitiesHandle, freeIndicesFromStatusHandle);
        }

        [BurstCompile]
        private JobHandle UpdatePathfindingSystemData(ref SystemState state, JobHandle dependency)
        {
            JobHandle updateSeekersCountHandle = CalculateSeekersCount(ref state, dependency);
            JobHandle updateRequestedPathsCountHandle = CalculateRequestedPathsCount(ref state, dependency);
            JobHandle updateInProgressPathsCountHandle = CalculateInProgressPathsCount(ref state, dependency);
            JobHandle calculationJobsHandle = JobHandle.CombineDependencies(updateSeekersCountHandle, updateRequestedPathsCountHandle, updateInProgressPathsCountHandle);
            return UpdateSystemData(ref state, calculationJobsHandle);
        }

        [BurstCompile]
        private JobHandle CalculateSeekersCount(ref SystemState state, JobHandle dependency)
        {
            for (int i = 0; i < _seekersParallelCounter.Length; i++)
            {
                _seekersParallelCounter[i] = 0;
            }

            CalculateSeekersCountJob calculateSeekersCountJob = new CalculateSeekersCountJob
            {
                ParallelCounter = _seekersParallelCounter
            };

            return calculateSeekersCountJob.ScheduleParallel(dependency);
        }

        [BurstCompile]
        private JobHandle CalculateRequestedPathsCount(ref SystemState state, JobHandle dependency)
        {
            for (int i = 0; i < _requestedPathsParallelCounter.Length; i++)
            {
                _requestedPathsParallelCounter[i] = 0;
            }

            CalculateRequestedPathsCountJob calculateRequestedPathsCountJob = new CalculateRequestedPathsCountJob
            {
                ParallelCounter = _requestedPathsParallelCounter
            };

            return calculateRequestedPathsCountJob.ScheduleParallel(dependency);
        }

        [BurstCompile]
        private JobHandle CalculateInProgressPathsCount(ref SystemState state, JobHandle dependency)
        {
            for (int i = 0; i < _inProgressPathsParallelCounter.Length; i++)
            {
                _inProgressPathsParallelCounter[i] = 0;
            }

            CalculateInProgressPathsCountJob calculateInProgressPathsCountJob = new CalculateInProgressPathsCountJob
            {
                ParallelCounter = _inProgressPathsParallelCounter
            };

            return calculateInProgressPathsCountJob.ScheduleParallel(dependency);
        }

        [BurstCompile]
        private JobHandle UpdateSystemData(ref SystemState state, JobHandle dependency)
        {
            UpdateSystemDataJob updateSystemDataJob = new UpdateSystemDataJob
            {
                SeekerCounter = _seekersParallelCounter,
                RequestedPathsCounter = _requestedPathsParallelCounter,
                InProgressPathsCounter = _inProgressPathsParallelCounter
            };

            return updateSystemDataJob.ScheduleParallel(dependency);
        }

        [BurstCompile]
        [WithAll(typeof(Seeker))]
        [WithNone(typeof(SeekerQuerryIndex))]
        private partial struct CreateSeekerQueryIndexComponentsJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter CommandBuffer;

            public void Execute([EntityIndexInQuery] int indexInQuery, Entity entity)
            {
                SeekerQuerryIndex seekerQuerryIndex = new SeekerQuerryIndex
                {
                    Value = -1
                };

                CommandBuffer.AddComponent(indexInQuery, entity, seekerQuerryIndex);
            }
        }

        [BurstCompile]
        private partial struct AssignQueryIndicesJob : IJobEntity
        {
            public NativeQueue<int> FreeIndices;

            public void Execute(in Seeker seeker, ref SeekerQuerryIndex seekerQuerryIndex)
            {
                if (FreeIndices.Count == 0)
                    return;

                if (seeker.Status != PathStatus.Requested)
                    return;

                if (seekerQuerryIndex.Value != -1)
                    return;

                seekerQuerryIndex.Value = FreeIndices.Dequeue();
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

            public void Execute(in LocalToWorld localToWorld, in Agent agent, in Destination destination, ref Seeker seeker,
                DynamicBuffer<PathWaypoint> pathWaypoints, in SeekerQuerryIndex seekerQuerryIndex)
            {
                if (seekerQuerryIndex.Value == -1)
                    return;

                NavMeshQuery query = NavMeshQueriesPtr[seekerQuerryIndex.Value];

                if (seeker.Status == PathStatus.Requested && !SystemData.SkipNewRequests)
                {
                    seeker.RequestStartPosition = localToWorld.Position;
                    seeker.RequestEndPosition = destination.Value;

                    if (math.distancesq(localToWorld.Position, destination.Value) < 0.01f)
                    {
                        pathWaypoints.Clear();
                        pathWaypoints.Add(new PathWaypoint { Value = seeker.RequestStartPosition });
                        pathWaypoints.Add(new PathWaypoint { Value = seeker.RequestEndPosition });
                        seeker.LastCalculationTickCount = TickCount.Value;
                        seeker.LastCalculationTime = ElapsedTime;
                        seeker.Status = PathStatus.Success;
                        return;
                    }

                    NavMeshLocation startLocation = query.MapLocation(seeker.RequestStartPosition, new float3(seeker.StartPositionSnappingDistance), agent.AgentID);

                    if (!query.IsValid(startLocation))
                    {
                        pathWaypoints.Clear();
                        seeker.LastCalculationTickCount = TickCount.Value;
                        seeker.LastCalculationTime = ElapsedTime;
                        seeker.Status = PathStatus.Failure;
                        return;
                    }

                    NavMeshLocation endLocation = query.MapLocation(seeker.RequestEndPosition, new float3(seeker.DestinationPositionSnappingDistance), agent.AgentID);

                    if (!query.IsValid(endLocation))
                    {
                        pathWaypoints.Clear();
                        seeker.LastCalculationTickCount = TickCount.Value;
                        seeker.LastCalculationTime = ElapsedTime;
                        seeker.Status = PathStatus.Failure;
                        return;
                    }

                    PathQueryStatus status = query.BeginFindPath(startLocation, endLocation);

                    if (status != PathQueryStatus.InProgress && status != PathQueryStatus.Success)
                    {
                        pathWaypoints.Clear();
                        seeker.LastCalculationTickCount = TickCount.Value;
                        seeker.LastCalculationTime = ElapsedTime;
                        seeker.Status = PathStatus.Failure;
                        return;
                    }

                    seeker.Status = PathStatus.InProgress;
                    seeker.NavMeshStartPosition = startLocation.position;
                    seeker.NavMeshEndPosition = endLocation.position;
                    return;
                }

                if (seeker.Status == PathStatus.InProgress)
                {
                    PathQueryStatus status = query.UpdateFindPath(Settings.MaxPathIterations, out var _);

                    if (status != PathQueryStatus.InProgress && status != PathQueryStatus.Success)
                    {
                        pathWaypoints.Clear();
                        seeker.LastCalculationTickCount = TickCount.Value;
                        seeker.LastCalculationTime = ElapsedTime;
                        seeker.Status = PathStatus.Failure;
                        return;
                    }

                    if (status == PathQueryStatus.InProgress)
                        return;

                    status = query.EndFindPath(out int pathSize);

                    if ((status & PathQueryStatus.Success) == 0)
                    {
                        pathWaypoints.Clear();
                        seeker.LastCalculationTickCount = TickCount.Value;
                        seeker.LastCalculationTime = ElapsedTime;
                        seeker.Status = PathStatus.Failure;
                        return;
                    }

                    if (pathSize < 2)
                    {
                        pathWaypoints.Clear();
                        seeker.LastCalculationTickCount = TickCount.Value;
                        seeker.LastCalculationTime = ElapsedTime;
                        seeker.Status = PathStatus.Failure;
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
                            seeker.NavMeshStartPosition,
                            seeker.NavMeshEndPosition,
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
                        seeker.LastCalculationTickCount = TickCount.Value;
                        seeker.LastCalculationTime = ElapsedTime;
                        seeker.Status = PathStatus.Failure;
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
                    seeker.LastCalculationTickCount = TickCount.Value;
                    seeker.LastCalculationTime = ElapsedTime;
                    seeker.Status = PathStatus.Success;
                }
            }
        }

        [BurstCompile]
        private partial struct ReturnFreeIndicesFromStatus : IJobEntity
        {
            public NativeQueue<int>.ParallelWriter FreeIndices;

            public void Execute(in Seeker seeker, ref SeekerQuerryIndex seekerQuerryIndex)
            {
                if (seekerQuerryIndex.Value == -1)
                    return;

                if (seeker.Status == PathStatus.Success || seeker.Status == PathStatus.Failure)
                {
                    FreeIndices.Enqueue(seekerQuerryIndex.Value);
                    seekerQuerryIndex.Value = -1;
                }
            }
        }

        [BurstCompile]
        [WithNone(typeof(LocalTransform))]
        private partial struct ReturnFreeIndicesFromDestroyedEntitiesJob : IJobEntity
        {
            public NativeQueue<int>.ParallelWriter FreeIndices;
            public EntityCommandBuffer.ParallelWriter CommandBuffer;

            public void Execute([EntityIndexInQuery] int querryIndex, in SeekerQuerryIndex seekerQuerryIndex, Entity entity)
            {
                if (seekerQuerryIndex.Value != -1)
                    FreeIndices.Enqueue(seekerQuerryIndex.Value);

                CommandBuffer.RemoveComponent<SeekerQuerryIndex>(querryIndex, entity);
            }
        }

        [BurstCompile]
        private partial struct CalculateSeekersCountJob : IJobEntity
        {
            [NativeDisableParallelForRestriction] public NativeArray<int> ParallelCounter;

            [NativeSetThreadIndex] private int _threadIndex;

            public void Execute(in Seeker seeker) => ParallelCounter[_threadIndex]++;
        }

        [BurstCompile]
        private partial struct CalculateRequestedPathsCountJob : IJobEntity
        {
            [NativeDisableParallelForRestriction] public NativeArray<int> ParallelCounter;

            [NativeSetThreadIndex] private int _threadIndex;

            public void Execute(in Seeker seeker)
            {
                if (seeker.Status == PathStatus.Requested)
                    ParallelCounter[_threadIndex]++;
            }
        }

        [BurstCompile]
        private partial struct CalculateInProgressPathsCountJob : IJobEntity
        {
            [NativeDisableParallelForRestriction] public NativeArray<int> ParallelCounter;

            [NativeSetThreadIndex] private int _threadIndex;

            public void Execute(in Seeker seeker)
            {
                if (seeker.Status == PathStatus.InProgress)
                    ParallelCounter[_threadIndex]++;
            }
        }

        [BurstCompile]
        private partial struct UpdateSystemDataJob : IJobEntity
        {
            [ReadOnly] public NativeArray<int> SeekerCounter;
            [ReadOnly] public NativeArray<int> RequestedPathsCounter;
            [ReadOnly] public NativeArray<int> InProgressPathsCounter;

            public void Execute(ref PathfindingSystemData systemData)
            {
                int count = 0;
                for (int i = 0; i < SeekerCounter.Length; i++)
                {
                    count += SeekerCounter[i];
                }

                systemData.SeekersCount = count;

                count = 0;
                for (int i = 0; i < RequestedPathsCounter.Length; i++)
                {
                    count += RequestedPathsCounter[i];
                }

                systemData.RequestedPathsCount = count;

                count = 0;
                for (int i = 0; i < InProgressPathsCounter.Length; i++)
                {
                    count += InProgressPathsCounter[i];
                }

                systemData.InProgressPathsCount = count;
            }
        }
    }
}