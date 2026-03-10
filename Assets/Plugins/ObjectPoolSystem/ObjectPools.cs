using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Plugins.ObjectPoolSystem
{
    public class ObjectPools<T> : IObjectPools<T>, IDisposable where T : Enum
    {
        private readonly Dictionary<T, IObjectPool> _pools = new Dictionary<T, IObjectPool>();

        public event Action<GameObject> OnEnabledObject;
        public event Action<GameObject> OnDisabledObject;
        public event Action<GameObject> OnDestroyedObject;
        public event Action OnCleared;

        public ObjectPools(ObjectPoolPreference<T>[] objectPoolPreferences)
        {
            foreach (ObjectPoolPreference<T> preference in objectPoolPreferences)
            {
                ObjectPool objectPool = new ObjectPool(preference.CreateFunc, preference.InitialSize, preference.MaxSize);
                _pools.Add(preference.Key, objectPool);

                objectPool.OnEnabledObject += InvokeOnEnabledObjectEvent;
                objectPool.OnDisabledObject += InvokeOnDisabledObjectEvent;
                objectPool.OnDestroyedObject += InvokeOnDestroyedObjectEvent;
            }
        }

        public async UniTask Initialize(CancellationToken token = default)
        {
            List<UniTask> tasks = new List<UniTask>(_pools.Count);

            foreach (IObjectPool pool in _pools.Values)
                tasks.Add(pool.Initialize(token));

            await UniTask.WhenAll(tasks);
        }

        public IObjectPool Get(T key) => _pools[key];

        public void Clear()
        {
            foreach (IObjectPool pool in _pools.Values)
            {
                pool.Clear();
            }

            OnCleared?.Invoke();
        }

        public void Dispose() => Clear();

        private void InvokeOnEnabledObjectEvent(GameObject gameObject) => OnEnabledObject?.Invoke(gameObject);

        private void InvokeOnDisabledObjectEvent(GameObject gameObject) => OnDisabledObject?.Invoke(gameObject);

        private void InvokeOnDestroyedObjectEvent(GameObject gameObject) => OnDestroyedObject?.Invoke(gameObject);
    }
}