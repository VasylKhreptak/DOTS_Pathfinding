using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Infrastructure.UI.Windows.Core
{
    public abstract class BaseNavigationalWindow : BaseWindow
    {
        private GameObject _lastSelectedGameObject;

        public abstract override UniTask Show();

        public abstract override UniTask Hide();

        public override void OnBecameActive()
        {
            base.OnBecameActive();

            EventSystem.current.SetSelectedGameObject(_lastSelectedGameObject);
        }

        public override void OnBecameInactive()
        {
            base.OnBecameInactive();

            _lastSelectedGameObject = EventSystem.current?.currentSelectedGameObject;
        }

        public void SelectGameObjectIfActive(GameObject gameObject)
        {
            if (IsActive)
                EventSystem.current.SetSelectedGameObject(gameObject);

            _lastSelectedGameObject = gameObject;
        }
    }
}