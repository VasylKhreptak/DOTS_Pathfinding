using Unity.Entities;
using UnityEngine;

namespace Entities.Authoring.Pathfinding.Modifiers
{
    public class RadiusModifierAuthoring : MonoBehaviour
    {
        [SerializeField] private float _radius = 1f;
        [SerializeField] private float _detail = 10f;

        private class RadiusModifierBaker : Baker<RadiusModifierAuthoring>
        {
            public override void Bake(RadiusModifierAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new RadiusModifier
                {
                    Radius = authoring._radius,
                    Detail = authoring._detail
                });
            }
        }
    }

    public struct RadiusModifier : IComponentData
    {
        public float Radius;
        public float Detail;
    }
}