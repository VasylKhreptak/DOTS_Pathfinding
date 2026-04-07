using Entities.Authoring.Pathfinding;
using Entities.Authoring.Pathfinding.Movers;
using Entities.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Entities.Systems.Pathfinding.Movers
{
    [BurstCompile]
    [DisableAutoCreation]
    public partial struct PathTransformMoveSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TickCount>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            HandleMovementJob handleMovementJob = new HandleMovementJob()
            {
                TickCount = SystemAPI.GetSingleton<TickCount>(),
                DeltaTime = state.WorldUnmanaged.Time.DeltaTime
            };

            state.Dependency = handleMovementJob.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct HandleMovementJob : IJobEntity
        {
            public TickCount TickCount;
            public float DeltaTime;

            public void Execute(ref LocalTransform localTransform, DynamicBuffer<PathWaypoint> pathWaypoints, ref PathTransformMover mover, in Destination destination,
                ref Agent agent, in Seeker seeker)
            {
                float3 transformForward = localTransform.Forward();
                float3 moveDirection = transformForward;

                float endReachedDistanceSq = mover.EndReachedDistance * mover.EndReachedDistance;

                if (IsPathValid(pathWaypoints))
                {
                    if (TickCount.Value == seeker.LastCalculationTickCount)
                    {
                        WaypointInfo waypointInfo = GetCurrentWaypointInfo(ref localTransform, pathWaypoints, ref mover);

                        mover.CurrentWaypoint = waypointInfo.Position;
                        mover.CurrentWaypointIndex = waypointInfo.Index;
                    }

                    if (math.distancesq(localTransform.Position, mover.CurrentWaypoint) < mover.PickNextWaypointDistance * mover.PickNextWaypointDistance)
                    {
                        mover.CurrentWaypointIndex = math.min(mover.CurrentWaypointIndex + 1, pathWaypoints.Length - 1);
                        mover.CurrentWaypoint = pathWaypoints[mover.CurrentWaypointIndex].Value;
                    }

                    float3 endOfPath = pathWaypoints[^1].Value;

                    if (math.distancesq(localTransform.Position, endOfPath) < endReachedDistanceSq / 5f)
                    {
                        agent.ReachedEndOfPath = true;
                        agent.ReachedDestination = math.distancesq(localTransform.Position, destination.Value) < endReachedDistanceSq;
                        mover.CurrentSpeed = 0f;
                        return;
                    }

                    float3 directionToWaypoint = math.normalizesafe(mover.CurrentWaypoint - localTransform.Position);

                    if (mover.EnableRotation)
                    {
                        moveDirection.y = directionToWaypoint.y;
                        moveDirection = math.normalizesafe(moveDirection);
                    }
                    else
                    {
                        moveDirection = directionToWaypoint;
                    }

                    if (mover.CanMove)
                    {
                        float facingFactor = 1f;

                        if (mover.EnableRotation)
                        {
                            float3 flatDirectionToWaypoint = math.normalize(new float3(directionToWaypoint.x, 0f, directionToWaypoint.z));

                            float dot = math.dot(transformForward, flatDirectionToWaypoint);

                            quaternion targetRotation = quaternion.LookRotationSafe(flatDirectionToWaypoint, math.up());

                            float rotateSlowdownFactor = 1 - math.clamp(dot / 1.1f, 0f, 1f);

                            localTransform.Rotation = RotateTowards(localTransform.Rotation, targetRotation, mover.RotationSpeed * DeltaTime * rotateSlowdownFactor);

                            if (mover.SlowWhenNotFacingTarget)
                                facingFactor = math.clamp(dot, 0f, 1f);
                        }

                        float distanceToEndOfPathSq = math.distancesq(localTransform.Position, endOfPath);
                        float slowdownDistanceSq = mover.SlowdownDistance * mover.SlowdownDistance;
                        float distanceSlowdownFactor = math.clamp(distanceToEndOfPathSq / slowdownDistanceSq, 0f, 1f);

                        mover.CurrentSpeed += mover.Acceleration * DeltaTime;
                        mover.CurrentSpeed = math.min(mover.CurrentSpeed, mover.MaxSpeed * facingFactor * distanceSlowdownFactor);
                    }
                    else
                    {
                        ApplyDeceleration(ref mover, DeltaTime);
                    }

                    localTransform.Position += moveDirection * mover.CurrentSpeed * DeltaTime;
                    agent.ReachedEndOfPath = math.distancesq(localTransform.Position, endOfPath) < endReachedDistanceSq;
                    agent.ReachedDestination = math.distancesq(localTransform.Position, destination.Value) < endReachedDistanceSq;
                    return;
                }

                ApplyDeceleration(ref mover, DeltaTime);
                localTransform.Position += moveDirection * mover.CurrentSpeed * DeltaTime;
                agent.ReachedEndOfPath = false;
                agent.ReachedDestination = math.distancesq(localTransform.Position, destination.Value) < endReachedDistanceSq;
            }

            private bool IsPathValid(DynamicBuffer<PathWaypoint> pathWaypoints) => pathWaypoints.IsEmpty == false && pathWaypoints.Length > 1;

            private WaypointInfo GetCurrentWaypointInfo(ref LocalTransform localTransform, DynamicBuffer<PathWaypoint> pathWaypoints, ref PathTransformMover mover)
            {
                WaypointInfo waypointInfo = new WaypointInfo();

                float3 transformPosition = localTransform.Position;
                float3 closestWaypoint = float3.zero;
                float leastSquaredDistance = float.PositiveInfinity;
                int closestWaypointIndex = -1;

                for (int i = 0; i < pathWaypoints.Length; i++)
                {
                    float3 pathWaypoint = pathWaypoints[i].Value;
                    float distance = math.distancesq(transformPosition, pathWaypoint);

                    if (distance < leastSquaredDistance)
                    {
                        closestWaypoint = pathWaypoint;
                        leastSquaredDistance = distance;
                        closestWaypointIndex = i;
                    }
                }

                float pickNextDistanceSq = mover.PickNextWaypointDistance * mover.PickNextWaypointDistance;

                if (math.distancesq(transformPosition, closestWaypoint) < pickNextDistanceSq && closestWaypointIndex < pathWaypoints.Length - 1)
                {
                    waypointInfo.Position = pathWaypoints[closestWaypointIndex + 1].Value;
                    waypointInfo.Index = closestWaypointIndex + 1;
                    return waypointInfo;
                }

                waypointInfo.Position = closestWaypoint;
                waypointInfo.Index = closestWaypointIndex;
                return waypointInfo;
            }

            private quaternion RotateTowards(quaternion from, quaternion to, float maxDegreesDelta)
            {
                float angle = math.angle(from, to);
                return math.slerp(from, to, math.min(1f, math.radians(maxDegreesDelta) / angle));
            }

            private void ApplyDeceleration(ref PathTransformMover mover, float deltaTime)
            {
                mover.CurrentSpeed -= mover.Deceleration * deltaTime;
                mover.CurrentSpeed = math.max(mover.CurrentSpeed, 0f);
            }

            private struct WaypointInfo
            {
                public float3 Position;
                public int Index;
            }
        }
    }
}