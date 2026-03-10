using System;
using Cysharp.Threading.Tasks;
using Infrastructure.Services.Input.Core;
using Infrastructure.Services.Log.Core;
using Infrastructure.Services.Window.Core;
using Infrastructure.StateMachine.Game.States.Core;
using Infrastructure.StateMachine.Main.Core;
using Infrastructure.StateMachine.Main.States.Core;
using Infrastructure.UI.Windows.LoadingScreen.Core;

namespace Infrastructure.StateMachine.Game.States
{
    public class ReloadState : IGameState, IState
    {
        private readonly IStateMachine<IGameState> _stateMachine;
        private readonly ILogService _logService;
        private readonly IInputService _inputService;
        private readonly IWindowService _windowService;

        public ReloadState(IStateMachine<IGameState> stateMachine, ILogService logService, IInputService inputService, IWindowService windowService)
        {
            _stateMachine = stateMachine;
            _logService = logService;
            _inputService = inputService;
            _windowService = windowService;
        }

        public void Enter()
        {
            _logService.Log("Game.ReloadState.Enter");

            _inputService.SetActive(false);

            _windowService
                .GetOrCreateWindow(WindowID.LoadingScreen)
                .ContinueWith(window =>
                {
                    ILoadingScreen loadingScreen = (ILoadingScreen)window;

                    loadingScreen
                        .Show()
                        .ContinueWith(() => _stateMachine.Enter<SaveDataState, Action>(() => _stateMachine.Enter<BootstrapState>()))
                        .Forget();
                })
                .Forget();
        }
    }
}