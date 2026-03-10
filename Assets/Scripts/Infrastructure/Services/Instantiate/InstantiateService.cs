using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.Services.Instantiate.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Infrastructure.Services.Instantiate
{
    public class InstantiateService : IInstantiateService
    {
        public T Instantiate<T>(T prefab) where T : Object => Object.Instantiate(prefab);

        public T Instantiate<T>(T prefab, Transform parent) where T : Object => Object.Instantiate(prefab, parent);

        public T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation) where T : Object => Object.Instantiate(prefab, position, rotation);

        public T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Object =>
            Object.Instantiate(prefab, position, rotation, parent);

        public async UniTask<T> InstantiateAsync<T>(T prefab, CancellationToken token = default) where T : Object
        {
            AsyncInstantiateOperation<T> instantiateOperation = Object.InstantiateAsync(prefab);

            token.Register(() => instantiateOperation.Cancel());

            await instantiateOperation.ToUniTask(cancellationToken: token).SuppressCancellationThrow();

            if (token.IsCancellationRequested)
            {
                if (instantiateOperation.Result != null)
                    Object.Destroy(instantiateOperation.Result[0]);

                throw new OperationCanceledException();
            }

            return instantiateOperation.Result[0];
        }

        public async UniTask<T> InstantiateAsync<T>(T prefab, Transform parent, CancellationToken token = default) where T : Component
        {
            GameObject instance = await InstantiateAsync(prefab.gameObject, parent, token);
            return instance.GetComponent<T>();
        }

        public async UniTask<T> InstantiateAsync<T>(T prefab, Vector3 position, Quaternion rotation, CancellationToken token = default) where T : Component
        {
            GameObject instance = await InstantiateAsync(prefab.gameObject, position, rotation, token);
            return instance.GetComponent<T>();
        }

        public async UniTask<T> InstantiateAsync<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent, CancellationToken token = default)
            where T : Component
        {
            GameObject instance = await InstantiateAsync(prefab.gameObject, position, rotation, parent, token);
            return instance.GetComponent<T>();
        }

        public UniTask<GameObject> InstantiateAsync(GameObject prefab, Transform parent, CancellationToken token = default) =>
            InstantiateAsync(prefab, prefab.transform.position, prefab.transform.rotation, parent, token);

        public UniTask<GameObject> InstantiateAsync(GameObject prefab, Vector3 position, Quaternion rotation, CancellationToken token = default) =>
            InstantiateAsync(prefab, position, rotation, null, token);

        public async UniTask<GameObject> InstantiateAsync(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent, CancellationToken token = default)
        {
            bool previousActiveState = prefab.activeSelf;

            prefab.SetActive(false);

            InstantiateParameters parameters = new InstantiateParameters
            {
                parent = parent
            };

            AsyncInstantiateOperation<GameObject> instantiateOperation = Object.InstantiateAsync(prefab, position, rotation, parameters);

            prefab.SetActive(previousActiveState);

            token.Register(() => instantiateOperation.Cancel());

            await instantiateOperation.ToUniTask(cancellationToken: token).SuppressCancellationThrow();

            if (token.IsCancellationRequested)
            {
                if (instantiateOperation.Result != null)
                    Object.Destroy(instantiateOperation.Result[0]);

                throw new OperationCanceledException();
            }

            instantiateOperation.Result[0].SetActive(previousActiveState);
            return instantiateOperation.Result[0];
        }
    }
}