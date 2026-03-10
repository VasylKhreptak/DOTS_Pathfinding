using Infrastructure.Data.Models.Persistent.Core;
using Infrastructure.Services.Log.Core;
using Infrastructure.Services.Settings.Core;
using Infrastructure.StateMachine.Game.States.Core;
using Infrastructure.StateMachine.Main.Core;
using Infrastructure.StateMachine.Main.States.Core;
using UnityEngine;
using Screen = UnityEngine.Device.Screen;

namespace Infrastructure.StateMachine.Game.States
{
    public class SetupApplicationState : IState, IGameState
    {
        private readonly IStateMachine<IGameState> _gameStateMachine;
        private readonly ILogService _logService;
        private readonly ISettingsService _settingsService;
        private readonly IPersistentDataModel _persistentDataModel;

        public SetupApplicationState(IStateMachine<IGameState> gameStateMachine, ILogService logService, ISettingsService settingsService,
            IPersistentDataModel persistentDataModel)
        {
            _gameStateMachine = gameStateMachine;
            _logService = logService;
            _settingsService = settingsService;
            _persistentDataModel = persistentDataModel;
        }

        public void Enter()
        {
            _logService.Log("Game.SetupApplicationState.Enter");

            DisableSleepTimeout();

            _settingsService.ApplySettings(_persistentDataModel.Data.SettingsData);

            _gameStateMachine.Enter<FinalizeLoadingState>();
        }

        private void DisableSleepTimeout() => Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
}