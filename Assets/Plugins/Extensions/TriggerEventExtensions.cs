using Unity.Entities;
using Unity.Physics;

namespace Plugins.Extensions
{
    public static class TriggerEventExtensions
    {
        public static bool TryFindEntities<T1, T2>(this TriggerEvent triggerEvent,
            ComponentLookup<T1> lookup1, ComponentLookup<T2> lookup2, out Entity entity1, out Entity entity2)
            where T1 : unmanaged, IComponentData
            where T2 : unmanaged, IComponentData
        {
            entity1 = Entity.Null;
            entity2 = Entity.Null;

            if (lookup1.HasComponent(triggerEvent.EntityA) && lookup2.HasComponent(triggerEvent.EntityB))
            {
                entity1 = triggerEvent.EntityA;
                entity2 = triggerEvent.EntityB;
                return true;
            }

            if (lookup1.HasComponent(triggerEvent.EntityB) && lookup2.HasComponent(triggerEvent.EntityA))
            {
                entity1 = triggerEvent.EntityB;
                entity2 = triggerEvent.EntityA;
                return true;
            }

            return false;
        }
    }
}