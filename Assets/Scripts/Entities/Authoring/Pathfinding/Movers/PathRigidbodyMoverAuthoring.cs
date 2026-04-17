using Gameplay.Components.PhysicsAdditions;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Entities.Authoring.Pathfinding.Movers
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(RigidbodyConstraintsAuthoring))]
    public class PathRigidbodyMoverAuthoring : PathMoverAuthoring
    {
        [SerializeField] private bool _canMove = true;
        [SerializeField] private float _forcePerKg = 5f;
        [SerializeField] private float _desiredSpeed = 1f;
        [SerializeField] private bool _enableRotation = true;
        [SerializeField] private float _rotationSpeed = 360f;
        [SerializeField] private bool _slowWhenNotFacingTarget = true;
        [SerializeField] private float _pickNextWaypointDistance = 2;
        [SerializeField] private float _endReachedDistance = 0.2f;
        [SerializeField] private float _slowdownDistance = 0.6f;

        private void OnValidate()
        {
            _forcePerKg = Mathf.Max(0f, _forcePerKg);
            _desiredSpeed = Mathf.Max(0f, _desiredSpeed);
            _rotationSpeed = Mathf.Max(0f, _rotationSpeed);
            _pickNextWaypointDistance = Mathf.Max(0f, _pickNextWaypointDistance);
            _endReachedDistance = Mathf.Max(0f, _endReachedDistance);
            _slowdownDistance = Mathf.Max(_endReachedDistance, _slowdownDistance);
        }

        private class PathRigidbodyMoverBaker : Baker<PathRigidbodyMoverAuthoring>
        {
            public override void Bake(PathRigidbodyMoverAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new PathRigidbodyMover
                {
                    CanMove = authoring._canMove,
                    ForcePerKg = authoring._forcePerKg,
                    DesiredSpeed = authoring._desiredSpeed,
                    EnableRotation = authoring._enableRotation,
                    RotationSpeed = authoring._rotationSpeed,
                    SlowWhenNotFacingTarget = authoring._slowWhenNotFacingTarget,
                    PickNextWaypointDistance = authoring._pickNextWaypointDistance,
                    EndReachedDistance = authoring._endReachedDistance,
                    SlowdownDistance = authoring._slowdownDistance
                });
            }
        }
    }

    public struct PathRigidbodyMover : IComponentData
    {
        public bool CanMove;
        public float ForcePerKg;
        public float DesiredSpeed;
        public bool EnableRotation;
        public float RotationSpeed;
        public bool SlowWhenNotFacingTarget;
        public float PickNextWaypointDistance;
        public float EndReachedDistance;
        public float SlowdownDistance;

        public float CurrentForce;
        public float3 CurrentWaypoint;
        public int CurrentWaypointIndex;
    }
}