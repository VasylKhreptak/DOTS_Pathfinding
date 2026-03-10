using Cysharp.Threading.Tasks;
using Infrastructure.Services.Window.Core;
using Infrastructure.Tools;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Infrastructure.Services.Window
{
    public class WindowDebugger : MonoBehaviour
    {
        private IWindowService _windowService;

        [Inject]
        public void Construct(IWindowService windowService) => _windowService = windowService;

        private readonly AutoResetCancellationTokenSource _cts = new AutoResetCancellationTokenSource();

        #region MonoBehaviour

        private void OnDestroy() => _cts.Cancel();

        #endregion

        [Button]
        private void CreateWindow(WindowID id) => _windowService.CreateWindow(id, _cts.Token).ContinueWith(window => window.Show()).Forget();

        [Button]
        private void CreateAndCancelAfterFrameDelay(WindowID id, int framesCount = 1)
        {
            CreateWindow(id);
            UniTask.DelayFrame(framesCount, cancellationToken: _cts.Token).ContinueWith(CancelCreation).Forget();
        }

        [Button]
        private void CancelCreation() => _cts.Cancel();

        [Button]
        private void DestroyTopWindow()
        {
            IWindow window = _windowService.GetTopWindow();

            if (window != null)
                Destroy(window.RootRectTransform.gameObject);
        }
    }
}