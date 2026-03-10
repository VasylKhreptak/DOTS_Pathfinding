using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.Extensions;
using Infrastructure.Services.Input.Core;
using Infrastructure.Services.Window.Core;
using Infrastructure.Tools;
using Infrastructure.UI.Popups.Core;
using VContainer;

namespace Infrastructure.UI.Popups
{
    public class ContinuationPopup : FadePopup, IContinuationPopup
    {
        private IInputService _inputService;
        private IWindowService _windowService;

        [Inject]
        public void Construct(IInputService inputService, IWindowService windowService)
        {
            _inputService = inputService;
            _windowService = windowService;
        }

        private readonly AutoResetCancellationTokenSource _inputCts = new AutoResetCancellationTokenSource();
        private readonly UniTaskCompletionSource _continuationTaskSource = new UniTaskCompletionSource();

        public UniTask ContinueTask => _continuationTaskSource.Task;

        public override async UniTask Show()
        {
            _inputCts.Cancel();

            await base.Show();

            WaitUntilContinue(_inputCts.Token).ContinueWith(() => _continuationTaskSource.TrySetResult()).Forget();
        }

        public override async UniTask Hide()
        {
            _inputCts.Cancel();

            await base.Hide();
        }

        private async UniTask WaitUntilContinue(CancellationToken token)
        {
            await UniTask.Yield(token).SuppressCancellationThrow();

            while (!token.IsCancellationRequested)
            {
                if (_inputService.UI.Submit.Value && ReferenceEquals(_windowService.GetTopWindowIncludingParents(), this))
                {
                    await UniTask.Yield(token).SuppressCancellationThrow();
                    return;
                }

                await UniTask.Yield(token).SuppressCancellationThrow();
            }
        }
    }
}