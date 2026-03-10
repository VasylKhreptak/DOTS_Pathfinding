using Cysharp.Threading.Tasks;
using Gameplay.ObjectPoolSystem;
using Gameplay.StateMachine.States.Core;
using Infrastructure.Services.Log.Core;
using Infrastructure.StateMachine.Main.Core;
using Infrastructure.StateMachine.Main.States.Core;
using Plugins.ObjectPoolSystem;

namespace Gameplay.StateMachine.States
{
    public class InitializeObjectPoolsState : IGameplayState, IState
    {
        private readonly IObjectPools<GameplayPoolType> _objectPools;
        private readonly IStateMachine<IGameplayState> _stateMachine;
        private readonly ILogService _logService;

        public InitializeObjectPoolsState(IObjectPools<GameplayPoolType> objectPools, IStateMachine<IGameplayState> stateMachine, ILogService logService)
        {
            _objectPools = objectPools;
            _stateMachine = stateMachine;
            _logService = logService;
        }

        public void Enter()
        {
            _logService.Log("Gameplay.InitializeObjectsPoolState.Enter");

            _objectPools
                .Initialize()
                .ContinueWith(() => { _stateMachine.Enter<SetupLevelState>(); })
                .Forget();
        }
    }
}