using Infrastructure.EntryPoints.Core;
using UnityEngine;
using VContainer.Unity;

namespace Infrastructure.EntryPoints
{
    public class GameRunner : MonoBehaviour, IEntryPoint
    {
        #region MonoBehaviour

        private void Start() => Enter();

        #endregion

        public void Enter()
        {
            if (FindFirstObjectByType<LifetimeScope>() != null)
            {
                Destroy(gameObject);
                return;
            }

            VContainerSettings.Instance.GetOrCreateRootLifetimeScopeInstance().Build();
        }
    }
}