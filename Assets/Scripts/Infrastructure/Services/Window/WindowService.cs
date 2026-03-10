using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.Extensions;
using Infrastructure.Services.Window.Core;
using Infrastructure.Services.Window.Core.EventHandlers;
using Infrastructure.Services.Window.Factories.Core;
using UniRx;
using UniRx.Triggers;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace Infrastructure.Services.Window
{
    public class WindowService : IWindowService, IInitializable, IDisposable
    {
        private readonly IWindowFactory _windowFactory;
        private readonly SemaphoreProvider _semaphoreProvider;

        public WindowService(IWindowFactory windowFactory, LifetimeScope scope, SemaphoreProvider semaphoreProvider)
        {
            _windowFactory = windowFactory;
            _semaphoreProvider = semaphoreProvider;

            if (scope.Parent != null)
            {
                if (scope.Parent.Container.TryResolve(out IWindowService parentWindowService))
                    Parent = parentWindowService;
            }
        }

        private readonly LinkedList<WindowInfo> _windows = new LinkedList<WindowInfo>();

        public event Action OnBeforeFirstWindowCreation;

        public event Action OnBecameEmpty;

        public IWindowService Parent { get; }

        public bool IsLoadingAnyWindow { get; private set; }

        public void Initialize()
        {
            if (Parent != null)
            {
                Parent.OnBeforeFirstWindowCreation += OnParentBeforeFirstWindowCreation;
                Parent.OnBecameEmpty += OnParentBecameEmpty;
            }
        }

        public void Dispose()
        {
            DestroyAllWindows();

            if (Parent != null)
            {
                Parent.OnBeforeFirstWindowCreation -= OnParentBeforeFirstWindowCreation;
                Parent.OnBecameEmpty -= OnParentBecameEmpty;
            }
        }

        public async UniTask<IWindow> CreateWindow(WindowID windowID, CancellationToken token = default)
        {
            try
            {
                await _semaphoreProvider.SemaphoreSlim.WaitAsync(token);

                IsLoadingAnyWindow = true;

                if (_windows.Count == 0)
                    OnBeforeFirstWindowCreation?.Invoke();

                IWindow topWindow = GetTopWindow();

                if (topWindow is IWindowInactiveEventHandler inactiveEventHandler && topWindow.IsActive)
                    inactiveEventHandler.OnBecameInactive();

                IWindow window = await _windowFactory.CreateWindow(windowID, token);

                WindowInfo info = new WindowInfo
                {
                    ID = windowID,
                    Window = window,
                    DestroySubscription = window.RootRectTransform.OnDestroyAsObservable().Subscribe(_ => OnBeforeWindowDestroy(window))
                };

                _windows.AddLast(info);

                if (!this.IsLoadingAnyWindowInParents() && !this.HasAnyWindowInParents() && window is IWindowActiveEventHandler activeEventHandler)
                    activeEventHandler.OnBecameActive();

                IsLoadingAnyWindow = false;

                return window;
            }
            finally
            {
                _semaphoreProvider.SemaphoreSlim.Release();
            }
        }

        public UniTask<IWindow> GetOrCreateWindow(WindowID windowID, CancellationToken token = default)
        {
            if (TryFind(windowID, out IWindow window))
                return UniTask.FromResult(window);

            return CreateWindow(windowID, token);
        }

        public bool TryFind(WindowID windowID, out IWindow window)
        {
            for (LinkedListNode<WindowInfo> node = _windows.Last; node != null; node = node.Previous)
            {
                if (node.Value.ID == windowID)
                {
                    window = node.Value.Window;
                    return true;
                }
            }

            window = null;
            return false;
        }

        private void OnBeforeWindowDestroy(IWindow window)
        {
            LinkedListNode<WindowInfo> lastNode = _windows.Last;

            for (LinkedListNode<WindowInfo> node = _windows.Last; node != null; node = node.Previous)
            {
                WindowInfo windowInfo = node.Value;

                if (node.Value.Window != window)
                    continue;

                if (node == lastNode && !this.HasAnyWindowInParents() && !this.IsLoadingAnyWindowInParents())
                {
                    LinkedListNode<WindowInfo> previousNode = node.Previous;

                    if (window is IWindowInactiveEventHandler inactiveEventHandler)
                        inactiveEventHandler.OnBecameInactive();

                    if (previousNode != null)
                    {
                        if (previousNode.Value.Window is IWindowActiveEventHandler activeEventHandler)
                            activeEventHandler.OnBecameActive();
                    }
                }

                windowInfo.DestroySubscription.Dispose();
                _windows.Remove(windowInfo);

                break;
            }

            if (GetTopWindow() == null)
                OnBecameEmpty?.Invoke();
        }

        private void DestroyAllWindows()
        {
            for (LinkedListNode<WindowInfo> node = _windows.Last; node != null; node = node.Previous)
            {
                WindowInfo windowInfo = node.Value;

                Object.Destroy(windowInfo.Window.RootRectTransform);
            }
        }

        private void OnParentBeforeFirstWindowCreation()
        {
            IWindow topWindow = GetTopWindow();

            if (topWindow == null)
                return;

            if (topWindow is IWindowInactiveEventHandler inactiveEventHandler)
                inactiveEventHandler.OnBecameInactive();
        }

        private void OnParentBecameEmpty()
        {
            IWindow topWindow = GetTopWindow();

            if (topWindow == null)
                return;

            if (topWindow is IWindowActiveEventHandler activeEventHandler)
                activeEventHandler.OnBecameActive();
        }

        public IWindow GetTopWindow()
        {
            if (_windows.Count == 0)
                return null;

            return _windows.Last.Value.Window;
        }

        private class WindowInfo
        {
            public WindowID ID;
            public IWindow Window;
            public IDisposable DestroySubscription;
        }
    }
}