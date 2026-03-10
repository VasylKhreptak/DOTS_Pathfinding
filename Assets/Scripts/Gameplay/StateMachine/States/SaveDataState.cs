using System;
using Gameplay.StateMachine.States.Core;
using Infrastructure.Data.Models.Persistent.Core;
using Infrastructure.Data.Models.Persistent.Data;
using Infrastructure.Services.Log.Core;
using Infrastructure.StateMachine.Game.States.Core;
using Infrastructure.StateMachine.Main.Core;
using Infrastructure.StateMachine.Main.States.Core;

namespace Gameplay.StateMachine.States
{
    public class SaveDataState : IGameplayState, IPayloadedState<Action>
    {
        private readonly IStateMachine<IGameplayState> _gameplayStateMachine;
        private readonly ILogService _logService;
        private readonly IPersistentDataModel _persistentDataModel;
        private readonly IStateMachine<IGameState> _gameStateMachine;

        public SaveDataState(IStateMachine<IGameplayState> gameplayStateMachine, ILogService logService, IPersistentDataModel persistentDataModel,
            IStateMachine<IGameState> gameStateMachine)
        {
            _gameplayStateMachine = gameplayStateMachine;
            _logService = logService;
            _persistentDataModel = persistentDataModel;
            _gameStateMachine = gameStateMachine;
        }

        public void Enter(Action onComplete)
        {
            _logService.Log("Gameplay.SaveDataState.Enter");

            UpdateGameplayData();

            _gameStateMachine.Enter<Infrastructure.StateMachine.Game.States.SaveDataState, Action>(() =>
            {
                _gameplayStateMachine.Enter<LoopState>();
                onComplete?.Invoke();
            });
        }

        private void UpdateGameplayData()
        {
            GameplayData gameplayData = new GameplayData();

            //update gameplayData

            _persistentDataModel.Data.GameplayData = gameplayData;
        }
    }
}