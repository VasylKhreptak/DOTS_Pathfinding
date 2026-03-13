using Unity.Entities;
using UnityEngine;

namespace Entities.Authoring.Pathfinding
{
    public class NavMeshBakeCenterAuthoring : MonoBehaviour
    {
        private class Baker : Baker<NavMeshBakeCenterAuthoring>
        {
            public override void Bake(NavMeshBakeCenterAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<NavMeshBakeCenterTag>(entity);
            }
        }
    }

    public struct NavMeshBakeCenterTag : IComponentData { }
}