using Unity.Entities;
using UnityEngine;

namespace Entities.Bakers.Pathfinding
{
    public class MeshRendererMeshReferenceBaker : Baker<MeshRenderer>
    {
        public override void Bake(MeshRenderer authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            MeshFilter meshFilter = authoring.GetComponent<MeshFilter>();

            if (meshFilter == null || meshFilter.sharedMesh == null)
                return;

            AddComponent(entity, new MeshRendererMeshReference
            {
                Value = meshFilter.sharedMesh
            });
        }
    }

    public struct MeshRendererMeshReference : IComponentData
    {
        public UnityObjectRef<Mesh> Value;
    }
}