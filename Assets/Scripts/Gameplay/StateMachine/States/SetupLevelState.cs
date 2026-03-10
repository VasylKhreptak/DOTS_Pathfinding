using Gameplay.StateMachine.States.Core;
using Infrastructure.Services.Log.Core;
using Infrastructure.StateMachine.Main.Core;
using Infrastructure.StateMachine.Main.States.Core;

namespace Gameplay.StateMachine.States
{
    public class SetupLevelState : IGameplayState, IState
    {
        private readonly IStateMachine<IGameplayState> _stateMachine;
        private readonly ILogService _logService;

        public SetupLevelState(IStateMachine<IGameplayState> stateMachine, ILogService logService)
        {
            _stateMachine = stateMachine;
            _logService = logService;
        }

        public void Enter()
        {
            _logService.Log("Gameplay.LoadLevelState.Enter");

            //for example, create player entity here using PrefabLibrarySystem

            _stateMachine.Enter<SetupUIState>();
        }
    }
}