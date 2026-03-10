using Gameplay.StateMachine.States.Core;
using Infrastructure.Services.Log.Core;
using Infrastructure.StateMachine.Main.Core;
using Infrastructure.StateMachine.Main.States.Core;

namespace Gameplay.StateMachine.States
{
    public class SetupUIState : IGameplayState, IState
    {
        private readonly IStateMachine<IGameplayState> _stateMachine;
        private readonly ILogService _logService;

        public SetupUIState(IStateMachine<IGameplayState> stateMachine, ILogService logService)
        {
            _stateMachine = stateMachine;
            _logService = logService;
        }

        public void Enter()
        {
            _logService.Log("Gameplay.SetupUIState.Enter");

            //create initial UI (HUD, etc.) and then enter next state

            _stateMachine.Enter<CreateSystemsState>();
        }
    }
}