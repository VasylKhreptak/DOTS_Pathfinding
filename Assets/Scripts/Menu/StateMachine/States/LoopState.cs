using Infrastructure.Services.Log.Core;
using Infrastructure.StateMachine.Main.States.Core;
using Menu.StateMachine.States.Core;

namespace Menu.StateMachine.States
{
    public class LoopState : IMenuState, IState
    {
        private readonly ILogService _logService;

        public LoopState(ILogService logService)
        {
            _logService = logService;
        }

        public void Enter()
        {
            _logService.Log("Menu.LoopState.Enter");
        }
    }
}