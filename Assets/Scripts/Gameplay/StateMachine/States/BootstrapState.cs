using Gameplay.StateMachine.States.Core;
using Infrastructure.Services.Input.Core;
using Infrastructure.Services.Log.Core;
using Infrastructure.Services.TimeScale.Core;
using Infrastructure.StateMachine.Main.Core;
using Infrastructure.StateMachine.Main.States.Core;

namespace Gameplay.StateMachine.States
{
    public class BootstrapState : IGameplayState, IState
    {
        private readonly IStateMachine<IGameplayState> _stateMachine;
        private readonly ILogService _logService;
        private readonly ITimeScaleService _timeScaleService;
        private readonly IInputService _inputService;

        public BootstrapState(IStateMachine<IGameplayState> stateMachine, ILogService logService, ITimeScaleService timeScaleService, IInputService inputService)
        {
            _stateMachine = stateMachine;
            _logService = logService;
            _timeScaleService = timeScaleService;
            _inputService = inputService;
        }

        public void Enter()
        {
            _logService.Log("Gameplay.BootstrapState.Enter");
            _inputService.SetActive(false);
            _timeScaleService.PauseTime();
            _stateMachine.Enter<AwaitSubScenesLoadingState>();
        }
    }
}