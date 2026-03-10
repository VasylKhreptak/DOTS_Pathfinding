using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Common
{
    public abstract class BaseButton : SerializedMonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button _button;

        #region MonoBehaviour

        protected virtual void OnValidate() => _button ??= GetComponent<Button>();

        protected virtual void OnEnable() => _button.onClick.AddListener(OnClick);

        protected virtual void OnDisable() => _button.onClick.RemoveListener(OnClick);

        #endregion

        protected abstract void OnClick();
    }
}