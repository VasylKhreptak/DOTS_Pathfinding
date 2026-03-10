using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Infrastructure.Services.Instantiate.Core
{
    public interface IInstantiateService
    {
        public T Instantiate<T>(T prefab) where T : Object;

        public T Instantiate<T>(T prefab, Transform parent) where T : Object;

        public T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation) where T : Object;

        public T Instantiate<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Object;

        public UniTask<T> InstantiateAsync<T>(T prefab, CancellationToken token = default) where T : Object;

        public UniTask<T> InstantiateAsync<T>(T prefab, Transform parent, CancellationToken token = default) where T : Component;

        public UniTask<T> InstantiateAsync<T>(T prefab, Vector3 position, Quaternion rotation, CancellationToken token = default) where T : Component;
        public UniTask<T> InstantiateAsync<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent, CancellationToken token = default) where T : Component;

        public UniTask<GameObject> InstantiateAsync(GameObject prefab, Transform parent, CancellationToken token = default);
        public UniTask<GameObject> InstantiateAsync(GameObject prefab, Vector3 position, Quaternion rotation, CancellationToken token = default);
        public UniTask<GameObject> InstantiateAsync(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent, CancellationToken token = default);
    }
}