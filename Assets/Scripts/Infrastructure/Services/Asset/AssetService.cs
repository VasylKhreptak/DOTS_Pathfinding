using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.Services.Asset.Core;
using Infrastructure.Services.Instantiate.Core;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Infrastructure.Services.Asset
{
    public class AssetService : IAssetService, IDisposable
    {
        private readonly IInstantiateService _instantiateService;

        public AssetService(IInstantiateService instantiateService)
        {
            _instantiateService = instantiateService;
        }

        private readonly CompositeDisposable _destroySubscriptions = new CompositeDisposable();

        public async UniTask<T> LoadAsync<T>(AssetReferenceT<T> assetReference, CancellationToken token = default) where T : Object
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(assetReference);

            await handle.ToUniTask(cancellationToken: token).SuppressCancellationThrow();

            if (token.IsCancellationRequested)
            {
                if (handle.IsDone)
                    handle.Release();
                else
                    handle.ReleaseHandleOnCompletion();

                throw new OperationCanceledException();
            }

            return handle.Result;
        }

        public void Release<T>(T asset) => Addressables.Release(asset);

        public async UniTask<T> InstantiateAsync<T>(AssetReferenceT<T> assetReference, CancellationToken token = default) where T : Component
        {
            T prefab = await LoadAsync(assetReference, token);

            try
            {
                T instance = await _instantiateService.InstantiateAsync(prefab, token);
                instance.OnDestroyAsObservable().Subscribe(_ => Release(prefab)).AddTo(_destroySubscriptions);
                return instance;
            }
            finally
            {
                Release(prefab);
            }
        }

        public async UniTask<T> InstantiateAsync<T>(AssetReferenceT<T> assetReference, Transform parent, CancellationToken token = default) where T : Component
        {
            T prefab = await LoadAsync(assetReference, token);

            try
            {
                T instance = await _instantiateService.InstantiateAsync(prefab, parent, token);
                instance.OnDestroyAsObservable().Subscribe(_ => Release(prefab)).AddTo(_destroySubscriptions);
                return instance;
            }
            catch (Exception)
            {
                Release(prefab);
                throw;
            }
        }

        public async UniTask<T> InstantiateAsync<T>(AssetReferenceT<T> assetReference, Vector3 position, Quaternion rotation, Transform parent,
            CancellationToken token = default) where T : Component
        {
            T prefab = await LoadAsync(assetReference, token);

            try
            {
                T instance = await _instantiateService.InstantiateAsync(prefab, position, rotation, parent, token);
                instance.OnDestroyAsObservable().Subscribe(_ => Release(prefab)).AddTo(_destroySubscriptions);
                return instance;
            }
            catch (Exception)
            {
                Release(prefab);
                throw;
            }
        }

        public UniTask<GameObject> InstantiateAsync(AssetReferenceT<GameObject> assetReference, CancellationToken token = default) =>
            InstantiateAsync(assetReference, null, token);

        public async UniTask<GameObject> InstantiateAsync(AssetReferenceT<GameObject> assetReference, Transform parent, CancellationToken token = default)
        {
            GameObject prefab = await LoadAsync(assetReference, token);

            try
            {
                GameObject instance = await _instantiateService.InstantiateAsync(prefab, parent, token);
                instance.OnDestroyAsObservable().Subscribe(_ => Release(prefab)).AddTo(_destroySubscriptions);
                return instance;
            }
            catch (Exception)
            {
                Release(prefab);
                throw;
            }
        }

        public UniTask<GameObject> InstantiateAsync(AssetReferenceT<GameObject> assetReference, Vector3 position, Quaternion rotation,
            CancellationToken token = default) =>
            InstantiateAsync(assetReference, position, rotation, null, token);

        public async UniTask<GameObject> InstantiateAsync(AssetReferenceT<GameObject> assetReference, Vector3 position, Quaternion rotation, Transform parent,
            CancellationToken token = default)
        {
            GameObject prefab = await LoadAsync(assetReference, token);

            try
            {
                GameObject instance = await _instantiateService.InstantiateAsync(prefab, position, rotation, parent, token);
                instance.OnDestroyAsObservable().Subscribe(_ => Release(prefab)).AddTo(_destroySubscriptions);
                return instance;
            }
            catch (Exception)
            {
                Release(prefab);
                throw;
            }
        }

        public void Dispose() => _destroySubscriptions?.Dispose();
    }
}