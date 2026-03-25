using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Entities.Authoring.Pathfinding
{
    public class PathAuthoring : MonoBehaviour
    {
        private class PathBaker : Baker<PathAuthoring>
        {
            public override void Bake(PathAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddBuffer<PathWaypoint>(entity);
            }
        }
    }

    [InternalBufferCapacity(256)]
    public struct PathWaypoint : IBufferElementData
    {
        public float3 Value;
    }
}