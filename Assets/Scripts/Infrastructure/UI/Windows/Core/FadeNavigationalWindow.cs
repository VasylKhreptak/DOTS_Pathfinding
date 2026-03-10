using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Infrastructure.Services.Window.Core;
using Infrastructure.Tools;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Infrastructure.UI.Windows.Core
{
    public class FadeNavigationalWindow : BaseNavigationalWindow, IWindow
    {
        [Header("Preferences")]
        [SerializeField] private float _duration = 0.5f;
        [SerializeField] private Ease _ease = Ease.InOutCubic;

        private readonly AutoResetCancellationTokenSource _cts = new AutoResetCancellationTokenSource();

        #region MonoBehaviour

        protected override void Awake()
        {
            base.Awake();

            Disable();
        }

        protected virtual void OnDestroy() => _cts.Cancel();

        #endregion

        public override async UniTask Show()
        {
            _cts.Cancel();
            ContentRectTransform.gameObject.SetActive(true);
            await SetAlphaTask(1f, _cts.Token);
            ContentCanvasGroup.interactable = true;
        }

        public override async UniTask Hide()
        {
            _cts.Cancel();
            ContentCanvasGroup.interactable = false;
            EventSystem.current.SetSelectedGameObject(null);
            await SetAlphaTask(0f, _cts.Token);
            Destroy(gameObject);
        }

        private UniTask SetAlphaTask(float alpha, CancellationToken token) =>
            ContentCanvasGroup
                .DOFade(alpha, _duration)
                .SetEase(_ease)
                .SetUpdate(true)
                .Play()
                .WithCancellation(token);

        private void Disable()
        {
            ContentCanvasGroup.interactable = false;
            ContentCanvasGroup.alpha = 0f;
            ContentRectTransform.gameObject.SetActive(false);
        }
    }
}