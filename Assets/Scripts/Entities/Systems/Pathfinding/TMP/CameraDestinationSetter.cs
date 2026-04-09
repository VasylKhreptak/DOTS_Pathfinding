using Entities.Authoring.Pathfinding;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using UnityEngine.InputSystem;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Entities.Systems.Pathfinding.TMP
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    public partial class CameraDestinationSetter : SystemBase
    {
        protected override void OnCreate()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            RequireForUpdate<PhysicsWorldSingleton>();
        }

        protected override void OnUpdate()
        {
            if (Mouse.current.leftButton.isPressed == false)
                return;

            if (Camera.main == null)
                return;

            Transform cameraTransform = Camera.main.transform;

            RaycastInput input = new RaycastInput()
            {
                Start = cameraTransform.position,
                End = cameraTransform.position + cameraTransform.forward * 10000f,
                Filter = CollisionFilter.Default
            };

            PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

            if (physicsWorld.CastRay(input, out RaycastHit hit))
            {
                float3 destination = hit.Position;

                SetDestinationsJob job = new SetDestinationsJob()
                {
                    Value = destination
                };

                Dependency = job.ScheduleParallel(Dependency);
            }
        }

        [BurstCompile]
        [WithAll(typeof(Agent))]
        private partial struct SetDestinationsJob : IJobEntity
        {
            public float3 Value;

            public void Execute(ref Destination destination)
            {
                destination.Value = Value;
            }
        }
    }
}