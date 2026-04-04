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
                if (pathWaypoints.IsEmpty || pathWaypoints.Length < 2)
                    return;

                float3 endOfPath = pathWaypoints[pathWaypoints.Length - 1].Value;

                float3 transformForward = localTransform.Forward();

                float3 moveDirection = transformForward;

                if (agent.ReachedEndOfPath == false && agent.ReachedDestination == false && mover.CanMove)
                {
                    float3 currentWaypoint = GetCurrentWaypoint(ref localTransform, pathWaypoints, ref mover);
                    float3 directionToWaypoint = math.normalizesafe(currentWaypoint - localTransform.Position);
                    float facingFactor = 1f;

                    if (mover.EnableRotation)
                    {
                        moveDirection = transformForward;
                        moveDirection.y = currentWaypoint.y;
                        moveDirection = math.normalizesafe(moveDirection);

                        float dot = math.dot(transformForward, directionToWaypoint);

                        float side = math.cross(transformForward, directionToWaypoint).y;

                        float rotateSlowdownFactor = 1 - math.saturate((dot + 1f) * 0.5f);

                        mover.CurrentRotationSpeed += (side > 0f ? 1 : -1) * mover.AngularAcceleration * DeltaTime;
                        mover.CurrentRotationSpeed *= rotateSlowdownFactor;
                        mover.CurrentRotationSpeed = math.clamp(mover.CurrentRotationSpeed, -mover.MaxRotationSpeed, mover.MaxRotationSpeed);

                        if (mover.SlowWhenNotFacingTarget)
                            facingFactor = 1 - rotateSlowdownFactor;
                    }
                    else
                    {
                        mover.CurrentRotationSpeed = 0f;
                        moveDirection = directionToWaypoint;
                    }

                    mover.CurrentSpeed += mover.Acceleration * DeltaTime;
                    mover.CurrentSpeed = math.min(mover.CurrentSpeed, mover.MaxSpeed);
                    mover.CurrentSpeed *= facingFactor;
                }
                else
                {
                    mover.CurrentSpeed -= mover.Deceleration * DeltaTime;
                    mover.CurrentSpeed = math.max(mover.CurrentSpeed, 0f);
                    mover.CurrentRotationSpeed = 0f;
                }

                localTransform.Position += moveDirection * mover.CurrentSpeed * DeltaTime;
                localTransform.Rotation = math.mul(localTransform.Rotation, quaternion.RotateY(mover.CurrentRotationSpeed * DeltaTime));

                agent.ReachedEndOfPath = math.distance(localTransform.Position, endOfPath) < mover.EndReachedDistance;
                agent.ReachedDestination = math.distance(localTransform.Position, destination.Value) < mover.EndReachedDistance;
            }

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
        }
    }
}