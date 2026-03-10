using Gameplay.Services.SystemsState.Core;
using Gameplay.Systems.Groups;
using Unity.Entities;

namespace Gameplay.Services.SystemsState
{
    public class SystemsStateService : ISystemsStateService
    {
        public void EnableSystems()
        {
            World world = World.DefaultGameObjectInjectionWorld;

            world.GetExistingSystemManaged<EarlyUpdateSystemGroup>().Enabled = true;
            world.GetExistingSystemManaged<FixedUpdateSystemGroup>().Enabled = true;
            world.GetExistingSystemManaged<UpdateSystemGroup>().Enabled = true;
        }

        public void DisableSystems()
        {
            World world = World.DefaultGameObjectInjectionWorld;

            world.GetExistingSystemManaged<EarlyUpdateSystemGroup>().Enabled = false;
            world.GetExistingSystemManaged<FixedUpdateSystemGroup>().Enabled = false;
            world.GetExistingSystemManaged<UpdateSystemGroup>().Enabled = false;
        }
    }
}