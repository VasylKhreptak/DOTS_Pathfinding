using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Infrastructure.Services.Window.Core
{
    public interface IWindowService
    {
        public event Action OnBeforeFirstWindowCreation;

        public event Action OnBecameEmpty;

        public IWindowService Parent { get; }

        public bool IsLoadingAnyWindow { get; }

        public IWindow GetTopWindow();

        public UniTask<IWindow> CreateWindow(WindowID windowID, CancellationToken token = default);

        public UniTask<IWindow> GetOrCreateWindow(WindowID windowID, CancellationToken token = default);

        public bool TryFind(WindowID windowID, out IWindow window);
    }
}