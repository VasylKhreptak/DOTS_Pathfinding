using System;
using Unity.Entities;
using UnityEngine;

namespace Entities.Authoring.Pathfinding
{
    public class PathfindingSettingsAuthoring : MonoBehaviour
    {
        [Tooltip(
            "Initial size of the NavMesh queries buffer. This buffer is used to store the results of NavMesh queries, such as pathfinding. If the buffer is too small, it will be resized at runtime.")]
        [SerializeField] private int InitialNavMeshQueriesBufferSize = 32;
        [Tooltip(
            "Maximum number of iterations for pathfinding algorithms per one PathFinder per frame. This is used to prevent long pathfinding calculations from blocking the main thread. If the pathfinding algorithm exceeds this number of iterations, it will yield and continue in the next frame.")]
        [SerializeField] private int MaxPathIterations = 64;
        [Tooltip(
            "Size of the path nodes pool. This pool is used to store the nodes of the paths calculated by the pathfinding algorithms. If the pool is too small, path may not be calculated. If the pool is too large, it may waste memory.")]
        [SerializeField] private ushort PathNodePoolSize = 1024;

        private void OnValidate()
        {
            InitialNavMeshQueriesBufferSize = Math.Max(0, InitialNavMeshQueriesBufferSize);
            MaxPathIterations = Math.Max(1, MaxPathIterations);
            PathNodePoolSize = Math.Clamp(PathNodePoolSize, (ushort)128, ushort.MaxValue);
        }

        private class PathfindingSettingsBaker : Baker<PathfindingSettingsAuthoring>
        {
            public override void Bake(PathfindingSettingsAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new PathfindingSettings
                {
                    InitialNavMeshQueriesBufferSize = authoring.InitialNavMeshQueriesBufferSize,
                    MaxPathIterations = authoring.MaxPathIterations,
                    PathNodePoolSize = authoring.PathNodePoolSize
                });
            }
        }
    }

    public struct PathfindingSettings : IComponentData
    {
        public int InitialNavMeshQueriesBufferSize;
        public int MaxPathIterations;
        public int PathNodePoolSize;

        public static PathfindingSettings Default =>
            new PathfindingSettings
            {
                InitialNavMeshQueriesBufferSize = 32,
                MaxPathIterations = 64,
                PathNodePoolSize = 1024
            };
    }
}