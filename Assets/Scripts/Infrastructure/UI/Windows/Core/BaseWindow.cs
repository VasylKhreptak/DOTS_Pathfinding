using Cysharp.Threading.Tasks;
using Infrastructure.Services.Window.Core;
using Infrastructure.Services.Window.Core.EventHandlers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Infrastructure.UI.Windows.Core
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseWindow : SerializedMonoBehaviour, IWindow, IWindowActiveEventHandler, IWindowInactiveEventHandler
    {
        [Header("Base Window References")]
        [SerializeField] private RectTransform _rootRectTransform;
        [SerializeField] private CanvasGroup _rootCanvasGroup;
        [SerializeField] private CanvasGroup _contentCanvasGroup;

        protected CanvasGroup ContentCanvasGroup => _contentCanvasGroup;

        protected RectTransform ContentRectTransform { get; private set; }

        [ShowInInspector] [ReadOnly] public bool IsActive { get; private set; }

        public RectTransform RootRectTransform => _rootRectTransform;
        public bool IsInteractable => _contentCanvasGroup.interactable && _rootCanvasGroup.interactable;

        #region MonoBehaivour

        protected virtual void OnValidate()
        {
            _rootRectTransform = GetComponent<RectTransform>();
            _rootCanvasGroup = GetComponent<CanvasGroup>();
        }

        protected virtual void Awake()
        {
            ContentRectTransform = (RectTransform)_contentCanvasGroup.transform;

            _rootCanvasGroup.interactable = false;
        }

        #endregion

        public abstract UniTask Show();

        public abstract UniTask Hide();

        public virtual void OnBecameActive()
        {
            _rootCanvasGroup.interactable = true;
            IsActive = true;
        }

        public virtual void OnBecameInactive()
        {
            _rootCanvasGroup.interactable = false;
            IsActive = false;
        }
    }
}