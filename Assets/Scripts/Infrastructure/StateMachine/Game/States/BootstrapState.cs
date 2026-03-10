using Cysharp.Threading.Tasks;
using Infrastructure.Data.Models.Static.Core;
using Infrastructure.Services.AsyncScene.Core;
using Infrastructure.Services.Log.Core;
using Infrastructure.Services.Window.Core;
using Infrastructure.StateMachine.Game.States.Core;
using Infrastructure.StateMachine.Main.Core;
using Infrastructure.StateMachine.Main.States.Core;
using Infrastructure.UI.Windows.LoadingScreen.Core;

namespace Infrastructure.StateMachine.Game.States
{
    public class BootstrapState : IState, IGameState
    {
        private readonly IStateMachine<IGameState> _stateMachine;
        private readonly ILogService _logService;
        private readonly IAsyncSceneService _sceneService;
        private readonly IStaticDataModel _staticDataModel;
        private readonly IWindowService _windowService;

        public BootstrapState(IStateMachine<IGameState> stateMachine, ILogService logService,
            IAsyncSceneService sceneService, IStaticDataModel staticDataModel, IWindowService windowService)
        {
            _stateMachine = stateMachine;
            _logService = logService;
            _sceneService = sceneService;
            _staticDataModel = staticDataModel;
            _windowService = windowService;
        }

        public void Enter()
        {
            _logService.Log("Game.BootstrapState.Enter");

            _windowService
                .GetOrCreateWindow(WindowID.LoadingScreen)
                .ContinueWith(window =>
                {
                    ILoadingScreen loadingScreen = (ILoadingScreen)window;

                    loadingScreen.ShowInstantly();

                    _sceneService.Load(_staticDataModel.Config.BootstrapScene).ContinueWith(() => _stateMachine.Enter<LoadDataState>()).Forget();
                })
                .Forget();
        }
    }
}