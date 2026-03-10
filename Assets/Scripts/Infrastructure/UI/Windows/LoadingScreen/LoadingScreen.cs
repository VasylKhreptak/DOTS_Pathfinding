using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Infrastructure.Tools;
using Infrastructure.UI.Windows.Core;
using Infrastructure.UI.Windows.LoadingScreen.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.UI.Windows.LoadingScreen
{
    public class LoadingScreen : BaseNavigationalWindow, ILoadingScreen
    {
        [Header("References")]
        [SerializeField] private Slider _progressSlider;

        [Header("Preferences")]
        [SerializeField] private float _duration;
        [SerializeField] private Ease _ease;

        private readonly AutoResetCancellationTokenSource _cts = new AutoResetCancellationTokenSource();

        #region MonoBehaviour

        protected override void OnValidate()
        {
            base.OnValidate();

            _progressSlider ??= GetComponentInChildren<Slider>();
        }

        protected override void Awake()
        {
            base.Awake();

            Disable();
        }

        private void OnDestroy() => _cts.Cancel();

        #endregion

        public override UniTask Show()
        {
            _cts.Cancel();
            SetProgress(0f);
            gameObject.SetActive(true);
            return SetAlphaTask(1f, _cts.Token);
        }

        public override UniTask Hide()
        {
            _cts.Cancel();
            return SetAlphaTask(0f, _cts.Token).ContinueWith(() => Destroy(gameObject));
        }

        public void ShowInstantly()
        {
            _cts.Cancel();
            ContentCanvasGroup.alpha = 1f;
            SetProgress(0f);
            gameObject.SetActive(true);
        }

        public void HideInstantly()
        {
            _cts.Cancel();
            ContentCanvasGroup.alpha = 0f;
            Destroy(gameObject);
        }

        public void SetProgress(float progress) => _progressSlider.value = progress;

        private void Disable()
        {
            _cts.Cancel();
            ContentCanvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        private UniTask SetAlphaTask(float alpha, CancellationToken token) =>
            ContentCanvasGroup
                .DOFade(alpha, _duration)
                .SetEase(_ease)
                .SetUpdate(true)
                .Play()
                .WithCancellation(token);
    }
}