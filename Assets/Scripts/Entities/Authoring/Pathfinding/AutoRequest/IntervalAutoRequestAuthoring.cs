using Unity.Entities;
using UnityEngine;

namespace Entities.Authoring.Pathfinding.AutoRequest
{
    public class IntervalAutoRequestAuthoring : BaseAutoRequestAuthoring
    {
        [SerializeField] private float _value = 1f;

        private void OnValidate() => _value = Mathf.Max(_value, 0f);

        private class IntervalAutoRequestBaker : Baker<IntervalAutoRequestAuthoring>
        {
            public override void Bake(IntervalAutoRequestAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new IntervalAutoRequest
                {
                    Value = authoring._value
                });
            }
        }
    }

    public struct IntervalAutoRequest : IComponentData
    {
        public float Value;
    }
}