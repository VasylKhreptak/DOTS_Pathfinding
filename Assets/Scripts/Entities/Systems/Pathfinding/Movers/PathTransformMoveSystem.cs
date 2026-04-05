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

                float3 endOfPath = pathWaypoints[^1].Value;
                float3 currentWaypointPosition = GetCurrentWaypoint(ref localTransform, pathWaypoints, ref mover);
                float3 transformForward = localTransform.Forward();
                float3 directionToWaypoint = math.normalizesafe(currentWaypointPosition - localTransform.Position);
                float3 moveDirection = directionToWaypoint;

                if (agent.ReachedEndOfPath == false && agent.ReachedDestination == false && mover.CanMove)
                {
                    float facingFactor = 1f;

                    if (mover.EnableRotation)
                    {
                        float3 flatDirectionToWaypoint = math.normalize(new float3(directionToWaypoint.x, 0f, directionToWaypoint.z));

                        float dot = math.dot(transformForward, flatDirectionToWaypoint);
                        float3 cross = math.cross(transformForward, flatDirectionToWaypoint);

                        float rotateSlowdownFactor = 1 - math.clamp(dot, 0f, 1f) / 6f;

                        mover.CurrentRotationSpeed += (cross.y > 0f ? 1 : -1) * mover.AngularAcceleration * DeltaTime;
                        mover.CurrentRotationSpeed *= rotateSlowdownFactor;
                        mover.CurrentRotationSpeed = math.clamp(mover.CurrentRotationSpeed, -mover.MaxRotationSpeed, mover.MaxRotationSpeed);

                        if (mover.SlowWhenNotFacingTarget)
                            facingFactor = math.clamp(dot * 1.2f, 0f, 1f);
                    }
                    else
                    {
                        mover.CurrentRotationSpeed = 0f;
                    }

                    mover.CurrentSpeed += mover.Acceleration * DeltaTime;
                    mover.CurrentSpeed *= facingFactor;
                    mover.CurrentSpeed = math.min(mover.CurrentSpeed, mover.MaxSpeed);
                }
                else
                {
                    mover.CurrentSpeed -= mover.Deceleration * DeltaTime;
                    mover.CurrentSpeed = math.max(mover.CurrentSpeed, 0f);
                    mover.CurrentRotationSpeed = 0f;
                }

                localTransform.Position += moveDirection * mover.CurrentSpeed * DeltaTime;
                localTransform.Rotation = math.mul(localTransform.Rotation, quaternion.RotateY(math.radians(mover.CurrentRotationSpeed) * DeltaTime));

                agent.ReachedEndOfPath = math.distance(localTransform.Position, endOfPath) < mover.EndReachedDistance;
                agent.ReachedDestination = math.distance(localTransform.Position, destination.Value) < mover.EndReachedDistance;
            }

            private float3 GetCurrentWaypoint(ref LocalTransform localTransform,
                DynamicBuffer<PathWaypoint> pathWaypoints,
                ref PathTransformMover mover)
            {
                float3 position = localTransform.Position;

                float leastDistance = float.PositiveInfinity;
                int closestIndex = -1;

                for (int i = 0; i < pathWaypoints.Length; i++)
                {
                    float distance = math.distance(position,
                        pathWaypoints[i].Value);

                    if (distance < leastDistance)
                    {
                        leastDistance = distance;
                        closestIndex = i;
                    }
                }

                if (closestIndex == -1)
                    return position;

                float remainingDistance = mover.PickNextWaypointDistance;

                for (int i = closestIndex; i < pathWaypoints.Length - 1; i++)
                {
                    float3 start = pathWaypoints[i].Value;
                    float3 end = pathWaypoints[i + 1].Value;

                    float segmentLength = math.distance(start, end);

                    if (remainingDistance <= segmentLength)
                    {
                        float t = remainingDistance / segmentLength;

                        return math.lerp(start, end, t);
                    }

                    remainingDistance -= segmentLength;
                }

                return pathWaypoints[closestIndex].Value;
            }
        }
    }
}