using Unity.Entities;
using UnityEngine;

namespace Entities.Authoring.Pathfinding.AutoRequest
{
    public class PathIntervalAutoRequestAuthoring : BaseAutoRequestAuthoring
    {
        [SerializeField] private float _value = 1f;

        private void OnValidate() => _value = Mathf.Max(_value, 0f);

        private class PathIntervalAutoRequestBaker : Baker<PathIntervalAutoRequestAuthoring>
        {
            public override void Bake(PathIntervalAutoRequestAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new PathIntervalAutoRequest
                {
                    Value = authoring._value
                });
            }
        }
    }

    public struct PathIntervalAutoRequest : IComponentData
    {
        public float Value;
    }
}