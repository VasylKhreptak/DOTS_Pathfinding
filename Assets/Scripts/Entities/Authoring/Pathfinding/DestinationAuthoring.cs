using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Entities.Authoring.Pathfinding
{
    public class DestinationAuthoring : MonoBehaviour
    {
        private class DestinationBaker : Baker<DestinationAuthoring>
        {
            public override void Bake(DestinationAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new Destination());
            }
        }
    }

    public struct Destination : IComponentData
    {
        public float3 Value;
    }
}