using Cysharp.Threading.Tasks;
using Infrastructure.Services.Window.Core;
using Infrastructure.UI.Windows.Core;
using UnityEngine;

namespace UI.Windows.Menu
{
    public class MenuInitialWindow : BaseNavigationalWindow, IWindow
    {
        [Header("Preferences")]
        [SerializeField] private GameObject _firstSelected;

        #region MonoBehaviour

        protected override void Awake()
        {
            base.Awake();

            Disable();
        }

        #endregion

        public override UniTask Show()
        {
            gameObject.SetActive(true);
            ContentCanvasGroup.interactable = true;
            SelectGameObjectIfActive(_firstSelected);
            return UniTask.CompletedTask;
        }

        public override UniTask Hide()
        {
            Destroy(gameObject);
            return UniTask.CompletedTask;
        }

        private void Disable()
        {
            gameObject.SetActive(false);
            ContentCanvasGroup.interactable = false;
        }
    }
}