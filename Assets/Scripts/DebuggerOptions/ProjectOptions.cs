using System;
using System.ComponentModel;
using DebuggerOptions.Core;
using Infrastructure.Services.Vibration.Core;
using Infrastructure.StateMachine.Game.States;
using Infrastructure.StateMachine.Game.States.Core;
using Infrastructure.StateMachine.Main.Core;

namespace DebuggerOptions
{
    public class ProjectOptions : BaseOptions
    {
        private const string Category = "Project";

        private readonly IStateMachine<IGameState> _stateMachine;
        private readonly IVibrationService _vibrationService;

        public ProjectOptions(IStateMachine<IGameState> stateMachine, IVibrationService vibrationService)
        {
            _stateMachine = stateMachine;
            _vibrationService = vibrationService;
        }

        [Category(Category)]
        public void Reload() => _stateMachine.Enter<ReloadState>();

        [Category(Category)]
        public void VibrateHigh() => _vibrationService.Vibrate(VibrationPreset.High);

        [Category(Category)]
        public void VibrateMedium() => _vibrationService.Vibrate(VibrationPreset.Medium);

        [Category(Category)]
        public void SaveData() => _stateMachine.Enter<SaveDataState, Action>(null);
    }
}