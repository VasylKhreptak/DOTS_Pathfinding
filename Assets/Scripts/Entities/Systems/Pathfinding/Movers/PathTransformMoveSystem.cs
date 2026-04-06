using Entities.Authoring.Pathfinding;
using Entities.Authoring.Pathfinding.Movers;
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
        public void OnUpdate(ref SystemState state)
        {
            HandleMovementJob handleMovementJob = new HandleMovementJob()
            {
                DeltaTime = state.WorldUnmanaged.Time.DeltaTime
            };

            state.Dependency = handleMovementJob.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct HandleMovementJob : IJobEntity
        {
            public float DeltaTime;

            public void Execute(ref LocalTransform localTransform, DynamicBuffer<PathWaypoint> pathWaypoints, ref PathTransformMover mover, in Destination destination,
                ref Agent agent)
            {
                float3 transformForward = localTransform.Forward();
                float3 moveDirection = transformForward;

                if (IsPathValid(pathWaypoints))
                {
                    float3 endOfPath = pathWaypoints[^1].Value;
                    float3 currentWaypointPosition = GetCurrentWaypoint(ref localTransform, pathWaypoints, ref mover);
                    float3 directionToWaypoint = math.normalizesafe(currentWaypointPosition - localTransform.Position);

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

                            if (mover.EnableRotation && mover.SlowWhenNotFacingTarget)
                                facingFactor = math.clamp(dot, 0f, 1f);
                        }

                        float distanceToEndOfPath = math.distance(localTransform.Position, endOfPath);

                        float distanceSlowdownFactor = math.clamp(distanceToEndOfPath / mover.SlowdownDistance, 0f, 1f);

                        mover.CurrentSpeed += mover.Acceleration * DeltaTime;
                        mover.CurrentSpeed = math.min(mover.CurrentSpeed, mover.MaxSpeed * facingFactor * distanceSlowdownFactor);
                    }
                    else
                    {
                        ApplyDeceleration(ref mover, DeltaTime);
                    }

                    agent.ReachedEndOfPath = math.distance(localTransform.Position, endOfPath) < mover.EndReachedDistance;
                }
                else
                {
                    ApplyDeceleration(ref mover, DeltaTime);
                    agent.ReachedEndOfPath = false;
                }

                localTransform.Position += moveDirection * mover.CurrentSpeed * DeltaTime;
                agent.ReachedDestination = math.distance(localTransform.Position, destination.Value) < mover.EndReachedDistance;
            }

            private bool IsPathValid(DynamicBuffer<PathWaypoint> pathWaypoints) => pathWaypoints.IsEmpty == false && pathWaypoints.Length > 1;

            private float3 GetCurrentWaypoint(ref LocalTransform localTransform, DynamicBuffer<PathWaypoint> pathWaypoints, ref PathTransformMover mover)
            {
                float3 transformPosition = localTransform.Position;
                float3 closestWaypoint = float3.zero;
                float leastDistance = float.PositiveInfinity;
                int closestWaypointIndex = -1;

                for (int i = 0; i < pathWaypoints.Length; i++)
                {
                    float3 pathWaypoint = pathWaypoints[i].Value;
                    float distance = math.distance(transformPosition, pathWaypoint);

                    if (distance < leastDistance)
                    {
                        closestWaypoint = pathWaypoint;
                        leastDistance = distance;
                        closestWaypointIndex = i;
                    }
                }

                if (math.distance(transformPosition, closestWaypoint) < mover.PickNextWaypointDistance && closestWaypointIndex < pathWaypoints.Length - 1)
                    return pathWaypoints[closestWaypointIndex + 1].Value;

                return closestWaypoint;
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
        }
    }
}