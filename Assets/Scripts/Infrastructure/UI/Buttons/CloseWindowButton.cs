using Infrastructure.Services.Window.Core;
using UI.Common;
using UnityEngine;

namespace Infrastructure.UI.Buttons
{
    public class CloseWindowButton : BaseButton
    {
        [Header("References")]
        [SerializeField] private IWindow _window;

        #region MonoBehaviour

        protected override void OnValidate()
        {
            base.OnValidate();

            _window ??= GetComponentInParent<IWindow>(true);
        }

        #endregion

        protected override void OnClick() => _window.Hide();
    }
}