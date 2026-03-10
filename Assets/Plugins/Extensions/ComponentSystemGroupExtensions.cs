using System.Linq;
using Unity.Collections;
using Unity.Entities;

namespace Plugins.Extensions
{
    public static class ComponentSystemGroupExtensions
    {
        public static void DestroyAllSystems(this ComponentSystemGroup systemGroup)
        {
            World world = systemGroup.World;

            foreach (ComponentSystemBase system in systemGroup.ManagedSystems.ToList())
            {
                if (system is ComponentSystemGroup componentSystemGroup)
                    componentSystemGroup.DestroyAllSystems();

                systemGroup.RemoveSystemFromUpdateList(system);
                world.DestroySystemManaged(system);
            }

            NativeList<SystemHandle> unmanagedSystems = systemGroup.GetUnmanagedSystems();

            foreach (SystemHandle system in unmanagedSystems)
            {
                systemGroup.RemoveSystemFromUpdateList(system);
                world.DestroySystem(system);
            }

            unmanagedSystems.Dispose();
        }
    }
}