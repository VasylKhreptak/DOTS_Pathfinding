using Unity.Entities;
using UnityEngine;

namespace Entities.Authoring.Pathfinding
{
    public class NavMeshBakeSettingsAuthoring : MonoBehaviour
    {
        [Tooltip("Number of threads to use for baking. Set to 0 to use all available threads.")]
        [SerializeField] private uint _bakeThreadsCount = 1;
        [Tooltip("Interval in seconds between each bake")]
        [SerializeField] private float _bakeInterval = 2;
        [Tooltip("Range in world units around the NavMeshBakeCenter within which the NavMesh will be baked")]
        [SerializeField] private float _range = 100;
        [Tooltip("Initial size of the buffer used to store NavMesh sources. Will be resized if needed during baking.")]
        [SerializeField] private int _initialSourcesBufferSize = 1024 * 2;
        [Tooltip("Number of NavMesh sources to convert per frame")]
        [SerializeField] private int _navMeshSourceConversationBatchCount = 64;

        #region MonoBehaviour

        private void OnValidate()
        {
            _bakeInterval = Mathf.Max(_bakeInterval, 0f);
            _range = Mathf.Max(_range, 0f);
            _initialSourcesBufferSize = Mathf.Max(_initialSourcesBufferSize, 0);
            _navMeshSourceConversationBatchCount = Mathf.Max(_navMeshSourceConversationBatchCount, 0);
        }

        #endregion

        private class NavMeshBakeSettingsBaker : Baker<NavMeshBakeSettingsAuthoring>
        {
            public override void Bake(NavMeshBakeSettingsAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new NavMeshBakeSettings
                {
                    BakeThreadsCount = authoring._bakeThreadsCount,
                    BakeInterval = authoring._bakeInterval,
                    Range = authoring._range,
                    InitialSourcesBufferSize = authoring._initialSourcesBufferSize,
                    NavMeshSourceConversationBatchCount = authoring._navMeshSourceConversationBatchCount
                });
            }
        }
    }

    public struct NavMeshBakeSettings : IComponentData
    {
        public uint BakeThreadsCount;
        public float BakeInterval;
        public float Range;
        public int InitialSourcesBufferSize;
        public int NavMeshSourceConversationBatchCount;

        public static NavMeshBakeSettings Default =>
            new NavMeshBakeSettings
            {
                BakeThreadsCount = 2,
                BakeInterval = 2,
                Range = 100,
                InitialSourcesBufferSize = 1024 * 2,
                NavMeshSourceConversationBatchCount = 64
            };
    }
}