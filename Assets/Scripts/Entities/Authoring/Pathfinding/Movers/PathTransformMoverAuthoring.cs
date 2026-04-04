using Unity.Entities;
using UnityEngine;

namespace Entities.Authoring.Pathfinding.Movers
{
    public class PathTransformMoverAuthoring : PathMoverAuthoring
    {
        [SerializeField] private bool _canMove = true;
        [SerializeField] private float _maxSpeed = 1f;
        [SerializeField] private float _acceleration = 1f;
        [SerializeField] private float _deceleration = 1f;
        [SerializeField] private bool _enableRotation = true;
        [SerializeField] private float _maxRotationSpeed = 360f;
        [SerializeField] private float _angularAcceleration = 180f;
        [SerializeField] private bool _slowWhenNotFacingTarget = true;
        [SerializeField] private float _pickNextWaypointDistance = 2;
        [SerializeField] private float _endReachedDistance = 0.2f;
        [SerializeField] private float _slowdownDistance = 0.6f;

        private void OnValidate()
        {
            _maxSpeed = Mathf.Max(0f, _maxSpeed);
            _acceleration = Mathf.Max(0f, _acceleration);
            _deceleration = Mathf.Max(0f, _deceleration);
            _maxRotationSpeed = Mathf.Max(0f, _maxRotationSpeed);
            _angularAcceleration = Mathf.Max(0f, _angularAcceleration);
            _pickNextWaypointDistance = Mathf.Max(0f, _pickNextWaypointDistance);
            _endReachedDistance = Mathf.Max(0f, _endReachedDistance);
            _slowdownDistance = Mathf.Max(_endReachedDistance, _slowdownDistance);
        }

        private class PathTransformMoverBaker : Baker<PathTransformMoverAuthoring>
        {
            public override void Bake(PathTransformMoverAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new PathTransformMover
                {
                    CanMove = authoring._canMove,
                    MaxSpeed = authoring._maxSpeed,
                    Acceleration = authoring._acceleration,
                    Deceleration = authoring._deceleration,
                    EnableRotation = authoring._enableRotation,
                    MaxRotationSpeed = authoring._maxRotationSpeed,
                    AngularAcceleration = authoring._angularAcceleration,
                    SlowWhenNotFacingTarget = authoring._slowWhenNotFacingTarget,
                    PickNextWaypointDistance = authoring._pickNextWaypointDistance,
                    EndReachedDistance = authoring._endReachedDistance,
                    SlowdownDistance = authoring._slowdownDistance
                });
            }
        }
    }

    public struct PathTransformMover : IComponentData
    {
        public bool CanMove;
        public float MaxSpeed;
        public float Acceleration;
        public float Deceleration;
        public bool EnableRotation;
        public float MaxRotationSpeed;
        public float AngularAcceleration;
        public bool SlowWhenNotFacingTarget;
        public float PickNextWaypointDistance;
        public float EndReachedDistance;
        public float SlowdownDistance;

        public float CurrentSpeed;
        public float CurrentRotationSpeed;
    }
}