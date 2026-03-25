using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Entities.Components.Pathfinding
{
    public struct UnmanagedNavMeshBuildSource
    {
        public float4x4 TransformMatrix;
        public float3 Size;
        public NavMeshBuildSourceShape Shape;
        public int Area;
        public UnityObjectRef<Mesh> MeshReference;
        public bool GenerateLinks;

        public static implicit operator NavMeshBuildSource(UnmanagedNavMeshBuildSource source) =>
            new NavMeshBuildSource
            {
                transform = source.TransformMatrix,
                size = source.Size,
                shape = source.Shape,
                area = source.Area,
                sourceObject = source.MeshReference.Value,
                generateLinks = source.GenerateLinks
            };

        public static implicit operator UnmanagedNavMeshBuildSource(NavMeshBuildSource source) =>
            new UnmanagedNavMeshBuildSource
            {
                TransformMatrix = source.transform,
                Size = source.size,
                Shape = source.shape,
                Area = source.area,
                MeshReference = new UnityObjectRef<Mesh>
                {
                    Value = source.sourceObject as Mesh
                },
                GenerateLinks = source.generateLinks
            };
    }
}