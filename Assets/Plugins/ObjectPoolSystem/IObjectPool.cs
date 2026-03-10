using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Plugins.ObjectPoolSystem
{
    public interface IObjectPool
    {
        public int TotalCount { get; }
        public int ActiveCount { get; }
        public int InactiveCount { get; }

        public event Action<GameObject> OnEnabledObject;
        public event Action<GameObject> OnDisabledObject;
        public event Action<GameObject> OnDestroyedObject;
        public event Action OnCleared;

        public UniTask Initialize(CancellationToken token = default);

        public UniTask<GameObject> Get(CancellationToken token = default);

        public UniTask Expand(CancellationToken token = default);

        public UniTask Expand(int count, CancellationToken token = default);

        public void Clear();
    }
}