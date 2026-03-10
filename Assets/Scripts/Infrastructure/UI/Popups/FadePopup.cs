using Infrastructure.UI.Popups.Core;
using Infrastructure.UI.Windows.Core;

namespace Infrastructure.UI.Popups
{
    public class FadePopup : FadeNavigationalWindow, IPopup
    {
        #region MonoBehaviour

        protected override void Awake()
        {
            base.Awake();

            Disable();
        }

        #endregion

        private void Disable()
        {
            ContentCanvasGroup.interactable = false;
            ContentCanvasGroup.alpha = 0f;
            ContentRectTransform.gameObject.SetActive(false);
        }
    }
}