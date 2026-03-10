using Unity.Entities;
using Unity.Transforms;
using IComponentData = Unity.Entities.IComponentData;

namespace Plugins.Extensions
{
    public static class EntityManagerExtensions
    {
        public static bool HasComponentInParent<T>(this EntityManager em, Entity entity, out Entity componentEntity)
            where T : unmanaged, IComponentData
        {
            componentEntity = Entity.Null;

            Entity current = entity;

            while (current != Entity.Null)
            {
                if (em.HasComponent<T>(current))
                {
                    componentEntity = current;
                    return true;
                }

                if (!em.HasComponent<Parent>(current))
                    break;

                current = em.GetComponentData<Parent>(current).Value;
            }

            return false;
        }

        public static bool HasComponentInParent<T>(Entity entity, ref ComponentLookup<T> componentLookup, ref ComponentLookup<Parent> parentLookup,
            out Entity componentEntity) where T : unmanaged, IComponentData
        {
            componentEntity = Entity.Null;
            Entity current = entity;

            while (current != Entity.Null)
            {
                if (componentLookup.HasComponent(current))
                {
                    componentEntity = current;
                    return true;
                }

                if (!parentLookup.HasComponent(current))
                    break;

                current = parentLookup[current].Value;
            }

            return false;
        }
    }
}