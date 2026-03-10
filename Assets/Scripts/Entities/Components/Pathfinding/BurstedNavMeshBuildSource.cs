using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Entities.Components.Pathfinding
{
    public struct BurstedNavMeshBuildSource
    {
        public float4x4 TransformMatrix;
        public float3 Size;
        public NavMeshBuildSourceShape Shape;
        public int Area;
        public UnityObjectRef<Mesh> MeshReference;
        public bool GenerateLinks;

        public static implicit operator NavMeshBuildSource(BurstedNavMeshBuildSource source)
        {
            return new NavMeshBuildSource
            {
                transform = source.TransformMatrix,
                size = source.Size,
                shape = source.Shape,
                area = source.Area,
                sourceObject = source.MeshReference.Value,
                generateLinks = source.GenerateLinks
            };
        }
    }
}