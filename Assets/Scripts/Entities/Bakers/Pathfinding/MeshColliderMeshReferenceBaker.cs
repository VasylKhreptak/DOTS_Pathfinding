using Unity.Entities;
using UnityEngine;

namespace Entities.Bakers
{
    public class MeshColliderMeshReferenceBaker : Baker<MeshCollider>
    {
        public override void Bake(MeshCollider authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            if (authoring.sharedMesh == null)
                return;

            AddComponent(entity, new MeshColliderMeshReference
            {
                Value = authoring.sharedMesh
            });
        }
    }

    public struct MeshColliderMeshReference : IComponentData
    {
        public UnityObjectRef<Mesh> Value;
    }
}