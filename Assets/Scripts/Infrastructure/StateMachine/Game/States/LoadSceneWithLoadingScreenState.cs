using System;
using Cysharp.Threading.Tasks;
using Infrastructure.Services.Log.Core;
using Infrastructure.Services.Window.Core;
using Infrastructure.StateMachine.Game.States.Core;
using Infrastructure.StateMachine.Main.Core;
using Infrastructure.StateMachine.Main.States.Core;
using Infrastructure.UI.Windows.LoadingScreen.Core;

namespace Infrastructure.StateMachine.Game.States
{
    public class LoadSceneWithLoadingScreenState : IGameState, IPayloadedState<LoadSceneWithLoadingScreenState.Payload>
    {
        private readonly IStateMachine<IGameState> _stateMachine;
        private readonly ILogService _logService;
        private readonly IWindowService _windowService;

        public LoadSceneWithLoadingScreenState(IStateMachine<IGameState> stateMachine, ILogService logService, IWindowService windowService)
        {
            _stateMachine = stateMachine;
            _logService = logService;
            _windowService = windowService;
        }

        public void Enter(Payload payload)
        {
            _logService.Log($"Game.LoadSceneWithLoadingScreenAsyncState.Enter: {payload.SceneName}");

            _windowService
                .GetOrCreateWindow(WindowID.LoadingScreen)
                .ContinueWith(window =>
                {
                    ILoadingScreen loadingScreen = (ILoadingScreen)window;

                    loadingScreen
                        .Show()
                        .ContinueWith(() =>
                        {
                            payload.OnAfterTransitionScreenShow?.Invoke();

                            payload.OnComplete += () =>
                            {
                                payload.OnBeforeTransitionScreenHide?.Invoke();
                                loadingScreen.Hide();
                            };

                            _stateMachine.Enter<LoadSceneState, LoadSceneState.Payload>(payload);
                        })
                        .Forget();
                })
                .Forget();
        }

        public class Payload : LoadSceneState.Payload
        {
            public Action OnAfterTransitionScreenShow;
            public Action OnBeforeTransitionScreenHide;
        }
    }
}