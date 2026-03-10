using System;
using Cysharp.Threading.Tasks;
using Infrastructure.Services.AsyncScene.Core;
using Infrastructure.Services.Log.Core;
using Infrastructure.StateMachine.Game.States.Core;
using Infrastructure.StateMachine.Main.Core;
using Infrastructure.StateMachine.Main.States.Core;

namespace Infrastructure.StateMachine.Game.States
{
    public class LoadSceneState : IPayloadedState<LoadSceneState.Payload>, IGameState
    {
        private readonly IStateMachine<IGameState> _gameStateMachine;
        private readonly IAsyncSceneService _sceneService;
        private readonly ILogService _logService;

        public LoadSceneState(IStateMachine<IGameState> gameStateMachine, IAsyncSceneService sceneService, ILogService logService)
        {
            _gameStateMachine = gameStateMachine;
            _sceneService = sceneService;
            _logService = logService;
        }

        public void Enter(Payload payload)
        {
            _logService.Log($"Game.LoadSceneState.Enter: {payload.SceneName}");

            _sceneService
                .Load(payload.SceneName, payload.Progress)
                .ContinueWith(() =>
                {
                    _gameStateMachine.Enter<LoopState>();

                    payload.OnComplete?.Invoke();
                })
                .Forget();
        }

        public class Payload
        {
            public string SceneName;
            public Action OnComplete;
            public IProgress<float> Progress;
        }
    }
}