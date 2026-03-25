using Entities.Systems;
using Entities.Systems.Pathdinding;
using Entities.Systems.Pathdinding.Editor;
using Entities.Systems.Pathfinding.Modifiers;
using Gameplay.StateMachine.States.Core;
using Gameplay.Systems;
using Gameplay.Systems.Audio;
using Gameplay.Systems.Groups;
using Gameplay.Systems.PhysicsAdditions;
using Infrastructure.Services.Log.Core;
using Infrastructure.StateMachine.Main.Core;
using Infrastructure.StateMachine.Main.States.Core;
using Unity.Entities;
using VContainer;
using NavMeshBakeSystem = Entities.Systems.Pathdinding.NavMeshBakeSystem;
using PathfindingSystem = Entities.Systems.Pathfinding.PathfindingSystem;

namespace Gameplay.StateMachine.States
{
    public class CreateSystemsState : IGameplayState, IState
    {
        private readonly IStateMachine<IGameplayState> _gameplayStateMachine;
        private readonly ILogService _logService;
        private readonly IObjectResolver _objectResolver;

        public CreateSystemsState(IStateMachine<IGameplayState> gameplayStateMachine, ILogService logService, IObjectResolver objectResolver)
        {
            _gameplayStateMachine = gameplayStateMachine;
            _logService = logService;
            _objectResolver = objectResolver;
        }

        public void Enter()
        {
            _logService.Log("Gameplay.CreateSystemsState.Enter");

            CreateSystemGroups();
            CreateSystems();

            _gameplayStateMachine.Enter<FinalizeLoadingState>();
        }

        private void CreateSystemGroups()
        {
            CreateSystemManaged<EarlyUpdateSystemGroup, InitializationSystemGroup>().Enabled = false;
            CreateSystemManaged<FixedUpdateSystemGroup, FixedStepSimulationSystemGroup>().Enabled = false;
            CreateSystemManaged<UpdateSystemGroup, SimulationSystemGroup>().Enabled = false;
            CreateSystemManaged<LateUpdateSystemGroup, LateSimulationSystemGroup>().Enabled = false;
        }

        private void CreateSystems()
        {
            CreateEarlyUpdateSystems();
            CreateFixedUpdateSystems();
            CreateUpdateSystems();
            CreateLateUpdateSystems();
        }

        private void CreateEarlyUpdateSystems()
        {
            CreateSystem<TickCountSystem, EarlyUpdateSystemGroup>();
            CreateSystem<RandomInitializationSystem, EarlyUpdateSystemGroup>();
        }

        private void CreateFixedUpdateSystems()
        {
            CreateSystem<RigidbodyConstraintsSystem, FixedUpdateSystemGroup>();
        }

        private void CreateUpdateSystems()
        {
            CreateSystemManaged<PrefabLibrarySystem, UpdateSystemGroup>();

            CreateAudioSystems();
            CreateUISystems();
        }

        private void CreateAudioSystems()
        {
            CreateSystemManaged<AudioSourceSystem, UpdateSystemGroup>();

            //also systems that plays audio
        }

        private void CreateUISystems()
        {
            //managed systems that updates UI inside world tick timing
        }

        private void CreateLateUpdateSystems()
        {
            CreateSystemManaged<NavMeshBakeSystem, LateUpdateSystemGroup>();
            CreateSystemManaged<NavMeshObstacleSystem, LateUpdateSystemGroup>();
            CreateSystem<PathfindingSystem, LateUpdateSystemGroup>();
            CreateSystem<SmoothModifierSystem, LateUpdateSystemGroup>();
#if UNITY_EDITOR
            CreateSystemManaged<PathDrawSystem, LateUpdateSystemGroup>();
#endif
        }

        private void CreateSystem<T, TGroup>() where T : unmanaged, ISystem where TGroup : ComponentSystemGroup
        {
            World world = World.DefaultGameObjectInjectionWorld;
            SystemHandle system = world.CreateSystem<T>();

            TGroup systemGroup = world.GetExistingSystemManaged<TGroup>();
            systemGroup.AddSystemToUpdateList(system);
        }

        private T CreateSystemManaged<T, TGroup>() where T : ComponentSystemBase, new() where TGroup : ComponentSystemGroup
        {
            World world = World.DefaultGameObjectInjectionWorld;
            T system = world.CreateSystemManaged<T>();

            _objectResolver.Inject(system);

            TGroup systemGroup = world.GetExistingSystemManaged<TGroup>();
            systemGroup.AddSystemToUpdateList(system);

            return system;
        }
    }
}