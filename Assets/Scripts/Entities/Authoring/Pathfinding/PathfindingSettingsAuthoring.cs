using System;
using Unity.Entities;
using UnityEngine;

namespace Entities.Authoring.Pathfinding
{
    public class PathfindingSettingsAuthoring : MonoBehaviour
    {
        [Tooltip("Size of the NavMesh queries buffer. This buffer is used to store the results of NavMesh queries, such as pathfinding.")]
        [SerializeField] private int NavMeshQueriesBufferSize = 32;
        [Tooltip(
            "Maximum number of iterations for pathfinding algorithms per NavMeshQuery per frame. This is used to prevent long pathfinding calculations from blocking the main thread. If the pathfinding algorithm exceeds this number of iterations, it will yield and continue in the next frame.")]
        [SerializeField] private int MaxPathIterations = 128;
        [Tooltip(
            "Size of the path nodes pool. This pool is used to store the nodes of the paths calculated by the pathfinding algorithms. If the pool is too small, path may not be calculated. If the pool is too large, it may waste memory.")]
        [SerializeField] private ushort PathNodePoolSize = 1024 * 4;

        private void OnValidate()
        {
            NavMeshQueriesBufferSize = Math.Max(0, NavMeshQueriesBufferSize);
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
                    NavMeshQueriesBufferSize = authoring.NavMeshQueriesBufferSize,
                    MaxPathIterations = authoring.MaxPathIterations,
                    PathNodePoolSize = authoring.PathNodePoolSize
                });
            }
        }
    }

    public struct PathfindingSettings : IComponentData
    {
        public int NavMeshQueriesBufferSize;
        public int MaxPathIterations;
        public int PathNodePoolSize;

        public static PathfindingSettings Default =>
            new PathfindingSettings
            {
                NavMeshQueriesBufferSize = 32,
                MaxPathIterations = 64,
                PathNodePoolSize = 1024 * 4
            };
    }
}