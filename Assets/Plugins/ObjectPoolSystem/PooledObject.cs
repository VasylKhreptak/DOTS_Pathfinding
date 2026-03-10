using UniRx;
using UnityEngine;

namespace Plugins.ObjectPoolSystem
{
    public class PooledObject
    {
        public GameObject GameObject;
        public CompositeDisposable Subscriptions = new CompositeDisposable();
    }
}