using Cysharp.Threading.Tasks;
using Infrastructure.Services.Log.Core;
using Infrastructure.Services.Window.Core;
using Infrastructure.StateMachine.Main.Core;
using Infrastructure.StateMachine.Main.States.Core;
using Menu.StateMachine.States.Core;

namespace Menu.StateMachine.States
{
    public class SetupUIState : IMenuState, IState
    {
        private readonly IStateMachine<IMenuState> _stateMachine;
        private readonly ILogService _logService;
        private readonly IWindowService _windowService;

        public SetupUIState(IStateMachine<IMenuState> stateMachine, ILogService logService, IWindowService windowService)
        {
            _stateMachine = stateMachine;
            _logService = logService;
            _windowService = windowService;
        }

        public void Enter()
        {
            _logService.Log("Menu.SetupUIState.Enter");

            _windowService
                .CreateWindow(WindowID.MenuWindow)
                .ContinueWith(window =>
                {
                    window.Show();
                    _stateMachine.Enter<FinalizeLoadingState>();
                })
                .Forget();
        }
    }
}