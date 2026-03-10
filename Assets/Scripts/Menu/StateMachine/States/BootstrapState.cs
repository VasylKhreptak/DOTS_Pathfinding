using Infrastructure.Services.Input.Core;
using Infrastructure.Services.Log.Core;
using Infrastructure.Services.TimeScale.Core;
using Infrastructure.StateMachine.Main.Core;
using Infrastructure.StateMachine.Main.States.Core;
using Menu.StateMachine.States.Core;

namespace Menu.StateMachine.States
{
    public class BootstrapState : IMenuState, IState
    {
        private readonly IStateMachine<IMenuState> _stateMachine;
        private readonly ILogService _logService;
        private readonly ITimeScaleService _timeScaleService;
        private readonly IInputService _inputService;

        public BootstrapState(IStateMachine<IMenuState> stateMachine, ILogService logService, ITimeScaleService timeScaleService, IInputService inputService)
        {
            _stateMachine = stateMachine;
            _logService = logService;
            _timeScaleService = timeScaleService;
            _inputService = inputService;
        }

        public void Enter()
        {
            _logService.Log("Menu.BootstrapState.Enter");
            _timeScaleService.ResumeTime();
            _inputService.SetActive(false);
            _stateMachine.Enter<SetupUIState>();
        }
    }
}