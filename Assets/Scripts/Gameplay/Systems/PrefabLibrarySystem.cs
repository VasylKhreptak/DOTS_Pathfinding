using System;
using Gameplay.Components;
using Unity.Entities;

namespace Gameplay.Systems
{
    [DisableAutoCreation]
    public partial class PrefabLibrarySystem : SystemBase
    {
        public Entity GetPrefab(PrefabType type)
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            Entity prefabsContainerEntity = SystemAPI.GetSingletonEntity<PrefabsContainer>();

            DynamicBuffer<PrefabElement> buffer = entityManager.GetBuffer<PrefabElement>(prefabsContainerEntity);

            foreach (PrefabElement element in buffer)
            {
                if (element.Type == type)
                    return element.Entity;
            }

            throw new SystemException($"Prefab of type {type} not found in the PrefabsContainer.");
        }

        protected override void OnUpdate() { }
    }
}