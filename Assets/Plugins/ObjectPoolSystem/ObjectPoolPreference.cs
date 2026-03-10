using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Plugins.ObjectPoolSystem
{
    [Serializable]
    public class ObjectPoolPreference<T> where T : Enum
    {
        public T Key;
        public Func<CancellationToken, UniTask<GameObject>> CreateFunc;
        public GameObject Prefab;
        public int InitialSize;
        public int MaxSize = 20;
    }
}