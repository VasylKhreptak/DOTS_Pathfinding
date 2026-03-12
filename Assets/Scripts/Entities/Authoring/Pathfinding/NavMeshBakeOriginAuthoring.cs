using Unity.Entities;
using UnityEngine;

namespace Entities.Authoring.Pathfinding
{
    public class NavMeshBakeOriginAuthoring : MonoBehaviour
    {
        private class NavMeshBakeOriginBaker : Baker<NavMeshBakeOriginAuthoring>
        {
            public override void Bake(NavMeshBakeOriginAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<NavMeshBakeOriginTag>(entity);
            }
        }
    }

    public struct NavMeshBakeOriginTag : IComponentData { }
}