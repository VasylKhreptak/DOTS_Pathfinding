using Gameplay.StateMachine.States.Core;
using Gameplay.Systems.Groups;
using Infrastructure.Services.Log.Core;
using Infrastructure.StateMachine.Main.States.Core;
using Plugins.Extensions;
using Unity.Entities;

namespace Gameplay.StateMachine.States
{
    public class DestroySystemsState : IGameplayState, IState
    {
        private readonly ILogService _logService;

        public DestroySystemsState(ILogService logService)
        {
            _logService = logService;
        }

        public void Enter()
        {
            _logService.Log("Gameplay.DestroySystemsState.Enter");

            World world = World.DefaultGameObjectInjectionWorld;

            InitializationSystemGroup initializationSystemGroup = world.GetExistingSystemManaged<InitializationSystemGroup>();
            SimulationSystemGroup simulationSystemGroup = world.GetExistingSystemManaged<SimulationSystemGroup>();
            FixedStepSimulationSystemGroup fixedStepSimulationSystemGroup = world.GetExistingSystemManaged<FixedStepSimulationSystemGroup>();

            EarlyUpdateSystemGroup earlyUpdateSystemGroup = world.GetExistingSystemManaged<EarlyUpdateSystemGroup>();
            UpdateSystemGroup updateSystemGroup = world.GetExistingSystemManaged<UpdateSystemGroup>();
            FixedUpdateSystemGroup fixedUpdateSystemGroup = world.GetExistingSystemManaged<FixedUpdateSystemGroup>();

            earlyUpdateSystemGroup.DestroyAllSystems();
            updateSystemGroup.DestroyAllSystems();
            fixedUpdateSystemGroup.DestroyAllSystems();

            initializationSystemGroup.RemoveSystemFromUpdateList(earlyUpdateSystemGroup);
            simulationSystemGroup.RemoveSystemFromUpdateList(updateSystemGroup);
            fixedStepSimulationSystemGroup.RemoveSystemFromUpdateList(fixedUpdateSystemGroup);

            world.DestroySystemManaged(earlyUpdateSystemGroup);
            world.DestroySystemManaged(updateSystemGroup);
            world.DestroySystemManaged(fixedUpdateSystemGroup);
        }
    }
}