using System;
using Cysharp.Threading.Tasks;
using Gameplay.StateMachine.States.Core;
using Infrastructure.Data.Models.Static.Core;
using Infrastructure.Extensions;
using Infrastructure.Services.Input.Core;
using Infrastructure.Services.Log.Core;
using Infrastructure.Services.Window.Core;
using Infrastructure.StateMachine.Game.States;
using Infrastructure.StateMachine.Game.States.Core;
using Infrastructure.StateMachine.Main.Core;
using Infrastructure.StateMachine.Main.States.Core;
using Infrastructure.UI.Windows.LoadingScreen.Core;

namespace Gameplay.StateMachine.States
{
    public class RestartState : IGameplayState, IState
    {
        private readonly IStateMachine<IGameState> _gameStateMachine;
        private readonly ILogService _logService;
        private readonly IStaticDataModel _staticDataModel;
        private readonly IInputService _inputService;
        private readonly IWindowService _rootWindowService;
        private readonly IStateMachine<IGameplayState> _gameplayStateMachine;

        public RestartState(IStateMachine<IGameState> gameStateMachine, ILogService logService, IStaticDataModel staticDataModel, IInputService inputService,
            IWindowService windowService, IStateMachine<IGameplayState> gameplayStateMachine)
        {
            _gameStateMachine = gameStateMachine;
            _logService = logService;
            _staticDataModel = staticDataModel;
            _inputService = inputService;
            _gameplayStateMachine = gameplayStateMachine;
            _rootWindowService = windowService.GetRootWindowService();
        }

        public void Enter()
        {
            _logService.Log("Gameplay.RestartState.Enter");

            _inputService.SetActive(false);

            _rootWindowService
                .GetOrCreateWindow(WindowID.LoadingScreen)
                .ContinueWith(window =>
                {
                    ILoadingScreen loadingScreen = (ILoadingScreen)window;

                    loadingScreen.Show()
                        .ContinueWith(() =>
                        {
                            _gameplayStateMachine.Enter<SaveDataState, Action>(() =>
                            {
                                _gameplayStateMachine.Enter<DestroySystemsState>();

                                Progress<float> progress = new Progress<float>(x => loadingScreen.SetProgress(x));

                                LoadSceneState.Payload payload = new LoadSceneState.Payload
                                {
                                    SceneName = _staticDataModel.Config.GameplayScene,
                                    Progress = progress
                                };

                                _gameStateMachine.Enter<LoadSceneState, LoadSceneState.Payload>(payload);
                            });
                        })
                        .Forget();
                })
                .Forget();
        }
    }
}