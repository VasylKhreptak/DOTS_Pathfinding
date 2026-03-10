using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Plugins.ObjectPoolSystem
{
    public interface IObjectPools<in T> where T : Enum
    {
        public event Action<GameObject> OnEnabledObject;
        public event Action<GameObject> OnDisabledObject;
        public event Action<GameObject> OnDestroyedObject;
        public event Action OnCleared;

        public UniTask Initialize(CancellationToken token = default);

        public IObjectPool Get(T key);

        public void Clear();
    }
}