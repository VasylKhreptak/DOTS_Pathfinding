using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Infrastructure.VContainer.Tools
{
    [DefaultExecutionOrder(-4999)]
    public class AutoInject : MonoBehaviour
    {
        [Inject]
        public void Construct() => _injected = true;

        private bool _injected;

        private void Awake()
        {
            if (_injected)
            {
                Destroy(this);
                return;
            }

            TryInject();
        }

        private void TryInject()
        {
            LifetimeScope scope = FindScope();

            if (scope != null && scope.Container != null)
                scope.Container.InjectGameObject(gameObject);

            Destroy(this);
        }

        private LifetimeScope FindScope()
        {
            LifetimeScope scope = GetComponentInParent<LifetimeScope>();

            if (scope != null)
                return scope;

            scope = FindScopeForScene(SceneManager.GetActiveScene());

            if (scope != null)
                return scope;

            return VContainerSettings.Instance.GetOrCreateRootLifetimeScopeInstance();
        }

        private LifetimeScope FindScopeForScene(Scene scene)
        {
            foreach (GameObject rootGameObject in scene.GetRootGameObjects())
            {
                if (rootGameObject.TryGetComponent(out LifetimeScope lifetimeScope))
                    return lifetimeScope;
            }

            return null;
        }
    }
}