using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Gameplay.Components.PhysicsAdditions
{
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyConstraintsAuthoring : MonoBehaviour
    {
        [SerializeField] private bool3 _position;
        [SerializeField] private bool3 _rotation;

        private class Baker : Baker<RigidbodyConstraintsAuthoring>
        {
            public override void Bake(RigidbodyConstraintsAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                Vector3 initialPosition = authoring.transform.position;
                Quaternion initialRotation = authoring.transform.rotation;

                RigidbodyConstraints constraints = new RigidbodyConstraints
                {
                    InitialPosition = initialPosition,
                    InitialRotation = initialRotation,

                    Position = authoring._position,
                    Rotation = authoring._rotation
                };

                AddComponent(entity, constraints);
            }
        }
    }

    public struct RigidbodyConstraints : IComponentData
    {
        public float3 InitialPosition;
        public quaternion InitialRotation;

        public bool3 Position;
        public bool3 Rotation;
    }
}