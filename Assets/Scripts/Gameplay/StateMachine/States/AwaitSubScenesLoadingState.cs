using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Gameplay.StateMachine.States.Core;
using Infrastructure.Services.Log.Core;
using Infrastructure.StateMachine.Main.Core;
using Infrastructure.StateMachine.Main.States.Core;
using Unity.Entities;
using Unity.Scenes;

namespace Gameplay.StateMachine.States
{
    public class AwaitSubScenesLoadingState : IGameplayState, IState
    {
        private readonly ILogService _logService;
        private readonly IStateMachine<IGameplayState> _stateMachine;
        private readonly IReadOnlyList<SubScene> _subScenes;

        public AwaitSubScenesLoadingState(ILogService logService, IStateMachine<IGameplayState> stateMachine, IReadOnlyList<SubScene> subScenes)
        {
            _logService = logService;
            _stateMachine = stateMachine;
            _subScenes = subScenes;
        }

        public void Enter()
        {
            _logService.Log("Gameplay.AwaitSubSceneLoadingState.Enter");

            UniTask
                .WaitUntil(IsAllSubScenesLoaded)
                .ContinueWith(() => _stateMachine.Enter<InitializeObjectPoolsState>())
                .Forget();
        }

        private bool IsAllSubScenesLoaded()
        {
            World world = World.DefaultGameObjectInjectionWorld;
            WorldUnmanaged worldUnmanaged = world.Unmanaged;

            foreach (SubScene subScene in _subScenes)
            {
                if (subScene.SceneGUID == default)
                    continue;

                Entity sceneEntity = SceneSystem.GetSceneEntity(worldUnmanaged, subScene.SceneGUID);

                if (!SceneSystem.IsSceneLoaded(worldUnmanaged, sceneEntity))
                    return false;
            }

            return true;
        }
    }
}