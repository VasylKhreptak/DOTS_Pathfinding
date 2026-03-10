using System;
using Infrastructure.Data.Models.Persistent.Core;
using Infrastructure.Services.Log.Core;
using Infrastructure.Services.SaveLoad.Core;
using Infrastructure.Services.Settings.Core;
using Infrastructure.StateMachine.Game.States.Core;
using Infrastructure.StateMachine.Main.Core;
using Infrastructure.StateMachine.Main.States.Core;

namespace Infrastructure.StateMachine.Game.States
{
    public class SaveDataState : IGameState, IPayloadedState<Action>
    {
        private const string Key = "Data";

        private readonly IPersistentDataModel _persistentDataModel;
        private readonly IStateMachine<IGameState> _gameStateMachine;
        private readonly ILogService _logService;
        private readonly ISaveLoadService _saveLoadService;
        private readonly ISettingsService _settingsService;

        public SaveDataState(IPersistentDataModel persistentDataModel, IStateMachine<IGameState> gameStateMachine, ILogService logService,
            ISaveLoadService saveLoadService, ISettingsService settingsService)
        {
            _persistentDataModel = persistentDataModel;
            _gameStateMachine = gameStateMachine;
            _logService = logService;
            _saveLoadService = saveLoadService;
            _settingsService = settingsService;
        }

        public void Enter(Action onComplete)
        {
            _logService.Log("Game.SaveDataState.Enter");

            UpdatePersistentData();

            _saveLoadService.Save(Key, _persistentDataModel.Data);

            _logService.Log("Saved local data");

            _gameStateMachine.Enter<LoopState>();
            onComplete?.Invoke();
        }

        private void UpdatePersistentData()
        {
            _persistentDataModel.Data.SettingsData = _settingsService.GetSettings();
        }
    }
}