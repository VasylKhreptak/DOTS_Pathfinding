using Unity.Entities;
using UnityEngine;

namespace Entities.Authoring.Pathfinding.AutoRequest
{
    public class MinAutoRequestIntervalAuthoring : MonoBehaviour
    {
        [SerializeField] private float _value = 0.3f;

        private void OnValidate() => _value = Mathf.Max(_value, 0.0f);

        private class Baker : Baker<MinAutoRequestIntervalAuthoring>
        {
            public override void Bake(MinAutoRequestIntervalAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new MinAutoRequestInterval { Value = authoring._value });
            }
        }
    }

    public struct MinAutoRequestInterval : IComponentData
    {
        public float Value;
    }
}