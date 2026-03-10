using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Plugins.ObjectPoolSystem
{
    public class ObjectPool : IObjectPool, IDisposable
    {
        private readonly HashSet<PooledObject> _totalPool = new HashSet<PooledObject>();
        private readonly HashSet<PooledObject> _activePool = new HashSet<PooledObject>();
        private readonly HashSet<PooledObject> _inactivePool = new HashSet<PooledObject>();

        private readonly int _initialSize;
        private readonly int _maxSize;

        private readonly CancellationTokenSource _disposeCts = new CancellationTokenSource();
        private readonly Func<CancellationToken, UniTask<GameObject>> _createFunc;

        public int TotalCount => _totalPool.Count;
        public int ActiveCount => _activePool.Count;
        public int InactiveCount => _inactivePool.Count;

        public event Action<GameObject> OnEnabledObject;
        public event Action<GameObject> OnDisabledObject;
        public event Action<GameObject> OnDestroyedObject;
        public event Action OnCleared;

        public ObjectPool(Func<CancellationToken, UniTask<GameObject>> createFunc, int initialSize = 16, int maxSize = 256)
        {
            initialSize = Mathf.Max(0, initialSize);
            maxSize = Mathf.Max(initialSize, maxSize);

            _createFunc = createFunc;
            _initialSize = initialSize;
            _maxSize = maxSize;
        }

        public async UniTask<GameObject> Get(CancellationToken token)
        {
            if (_inactivePool.Count > 0)
            {
                PooledObject pooledObject = _inactivePool.First();
                pooledObject.GameObject.SetActive(true);
                return pooledObject.GameObject;
            }

            if (_totalPool.Count < _maxSize)
            {
                await Expand(token);
                return await Get(token);
            }

            PooledObject lastPooledObject = _activePool.Last();

            lastPooledObject.GameObject.SetActive(false);
            lastPooledObject.GameObject.SetActive(true);

            return lastPooledObject.GameObject;
        }

        public void Clear()
        {
            foreach (PooledObject pooledObject in _totalPool.ToList())
            {
                Object.Destroy(pooledObject.GameObject);
            }

            OnCleared?.Invoke();
        }

        public async UniTask Initialize(CancellationToken token) => await Expand(_initialSize, token);

        public async UniTask Expand(CancellationToken token)
        {
            GameObject instance = await _createFunc(token);
            instance.SetActive(false);

            PooledObject pooledObject = new PooledObject
            {
                GameObject = instance
            };

            StartObserving(pooledObject);

            _totalPool.Add(pooledObject);
            _inactivePool.Add(pooledObject);
        }

        public async UniTask Expand(int count, CancellationToken token)
        {
            for (int i = 0; i < count; i++)
            {
                await Expand(token);
            }
        }

        private void StartObserving(PooledObject pooledObject)
        {
            StopObserving(pooledObject);

            pooledObject.GameObject.OnEnableAsObservable().Subscribe(_ => OnEnabled(pooledObject)).AddTo(pooledObject.Subscriptions);
            pooledObject.GameObject.OnDisableAsObservable().Subscribe(_ => OnDisabled(pooledObject)).AddTo(pooledObject.Subscriptions);
            pooledObject.GameObject.OnDestroyAsObservable().Subscribe(_ => OnDestroyed(pooledObject)).AddTo(pooledObject.Subscriptions);
        }

        private void StopObserving(PooledObject pooledObject) => pooledObject.Subscriptions.Clear();

        private void OnEnabled(PooledObject pooledObject)
        {
            _inactivePool.Remove(pooledObject);
            _activePool.Add(pooledObject);

            OnEnabledObject?.Invoke(pooledObject.GameObject);
        }

        private void OnDisabled(PooledObject pooledObject)
        {
            _activePool.Remove(pooledObject);
            _inactivePool.Add(pooledObject);

            OnDisabledObject?.Invoke(pooledObject.GameObject);
        }

        private void OnDestroyed(PooledObject pooledObject)
        {
            _totalPool.Remove(pooledObject);
            _inactivePool.Remove(pooledObject);
            _activePool.Remove(pooledObject);
            pooledObject.Subscriptions.Dispose();

            OnDestroyedObject?.Invoke(pooledObject.GameObject);
        }

        public void Dispose()
        {
            _disposeCts.Cancel();
            Clear();
        }
    }
}