using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.Services.Window.Core;
using Infrastructure.Tools;
using Plugins.Extensions;
using VContainer.Unity;
using Random = UnityEngine.Random;

namespace Infrastructure.Services.Window
{
    public class WindowAutoTester : IInitializable, IDisposable
    {
        private const float InitialDelay = 2f;
        private const float MinNewWindowInterval = 0.00f;
        private const float MaxNewWindowInterval = 0.5f;
        private const int WindowsToSpawn = 2;

        private readonly IWindowService _windowService;
        private readonly IReadOnlyList<WindowID> _possibleWindowIds;

        public WindowAutoTester(IWindowService windowService, IReadOnlyList<WindowID> possibleWindowIds)
        {
            _windowService = windowService;
            _possibleWindowIds = possibleWindowIds;
        }

        private readonly AutoResetCancellationTokenSource _cts = new AutoResetCancellationTokenSource();

        public void Initialize() => StartTesting(_cts.Token).Forget();

        public void Dispose() => _cts.Cancel();

        private async UniTask StartTesting(CancellationToken cancellationToken)
        {
            await UniTask.WaitForSeconds(InitialDelay, cancellationToken: cancellationToken);

            for (int i = 0; i < WindowsToSpawn; i++)
            {
                ShowRandomWindow(cancellationToken);

                await UniTask.WaitForSeconds(Random.Range(MinNewWindowInterval, MaxNewWindowInterval), cancellationToken: cancellationToken);
            }
        }

        private void ShowRandomWindow(CancellationToken token)
        {
            WindowID randomWindowId = _possibleWindowIds.Random();

            _windowService.CreateWindow(randomWindowId, token).ContinueWith(window => window.Show()).Forget();
        }
    }
}