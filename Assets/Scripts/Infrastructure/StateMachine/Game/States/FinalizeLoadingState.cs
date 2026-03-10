using System;
using Infrastructure.Data.Models.Static.Core;
using Infrastructure.Services.Log.Core;
using Infrastructure.Services.Window.Core;
using Infrastructure.StateMachine.Game.States.Core;
using Infrastructure.StateMachine.Main.Core;
using Infrastructure.StateMachine.Main.States.Core;
using Infrastructure.UI.Windows.LoadingScreen.Core;

namespace Infrastructure.StateMachine.Game.States
{
    public class FinalizeLoadingState : IGameState, IState
    {
        private readonly IStateMachine<IGameState> _stateMachine;
        private readonly IStaticDataModel _staticDataModel;
        private readonly ILogService _logService;
        private readonly IWindowService _windowService;

        public FinalizeLoadingState(IStateMachine<IGameState> stateMachine, IStaticDataModel staticDataModel, ILogService logService, IWindowService windowService)
        {
            _stateMachine = stateMachine;
            _staticDataModel = staticDataModel;
            _logService = logService;
            _windowService = windowService;
        }

        public void Enter()
        {
            _logService.Log("Game.FinalizeLoadingState.Enter");

            if (!_windowService.TryFind(WindowID.LoadingScreen, out IWindow window))
                return;

            ILoadingScreen loadingScreen = (ILoadingScreen)window;

            Progress<float> progress = new Progress<float>(x => loadingScreen.SetProgress(x));

            LoadSceneState.Payload payload = new LoadSceneState.Payload
            {
                SceneName = _staticDataModel.Config.MenuScene,
                Progress = progress
            };

            _stateMachine.Enter<LoadSceneState, LoadSceneState.Payload>(payload);
        }
    }
}