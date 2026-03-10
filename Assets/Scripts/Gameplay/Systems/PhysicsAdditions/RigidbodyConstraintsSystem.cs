using Gameplay.Components.PhysicsAdditions;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

namespace Gameplay.Systems.PhysicsAdditions
{
    [DisableAutoCreation]
    public partial struct RigidbodyConstraintsSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            ConstraintsJob constraintsJob = new ConstraintsJob();
            state.Dependency = constraintsJob.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct ConstraintsJob : IJobEntity
        {
            public void Execute(ref PhysicsVelocity velocity, in RigidbodyConstraints constraints, ref PhysicsMass mass, ref LocalTransform localTransform)
            {
                if (constraints.Position.x)
                {
                    velocity.Linear.x = 0f;
                    localTransform.Position.x = constraints.InitialPosition.x;
                }

                if (constraints.Position.y)
                {
                    velocity.Linear.y = 0f;
                    localTransform.Position.y = constraints.InitialPosition.y;
                }

                if (constraints.Position.z)
                {
                    velocity.Linear.z = 0f;
                    localTransform.Position.z = constraints.InitialPosition.z;
                }

                if (constraints.Rotation.x)
                {
                    velocity.Angular.x = 0f;
                    localTransform.Rotation.value.x = constraints.InitialRotation.value.x;
                }

                if (constraints.Rotation.y)
                {
                    velocity.Angular.y = 0f;
                    localTransform.Rotation.value.y = constraints.InitialRotation.value.y;
                }

                if (constraints.Rotation.z)
                {
                    velocity.Angular.z = 0f;
                    localTransform.Rotation.value.z = constraints.InitialRotation.value.z;
                }
            }
        }
    }
}