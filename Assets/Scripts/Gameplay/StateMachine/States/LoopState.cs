using Gameplay.StateMachine.States.Core;
using Infrastructure.Services.Log.Core;
using Infrastructure.StateMachine.Main.States.Core;

namespace Gameplay.StateMachine.States
{
    public class LoopState : IGameplayState, IState
    {
        private readonly ILogService _logService;

        public LoopState(ILogService logService)
        {
            _logService = logService;
        }

        public void Enter() => _logService.Log("Gameplay.GameplayLoopState.Enter");
    }
}