using Infrastructure.Services.Log.Core;
using Infrastructure.StateMachine.Game.States.Core;
using Infrastructure.StateMachine.Main.States.Core;

namespace Infrastructure.StateMachine.Game.States
{
    public class LoopState : IState, IGameState
    {
        private readonly ILogService _logService;

        public LoopState(ILogService logService)
        {
            _logService = logService;
        }

        public void Enter() => _logService.Log("Game.LoopState.Enter");
    }
}