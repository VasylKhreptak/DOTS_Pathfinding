using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Gameplay.Components
{
    public class PrefabsContainerAuthoring : SerializedMonoBehaviour
    {
        [Header("Preferences")]
        [SerializeField] private Dictionary<PrefabType, GameObject> _prefabs = new Dictionary<PrefabType, GameObject>();

        public class Baker : Baker<PrefabsContainerAuthoring>
        {
            public override void Bake(PrefabsContainerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                DynamicBuffer<PrefabElement> buffer = AddBuffer<PrefabElement>(entity);

                if (authoring._prefabs != null)
                {
                    foreach (KeyValuePair<PrefabType, GameObject> kvp in authoring._prefabs)
                    {
                        if (kvp.Value == null)
                        {
                            Debug.LogWarning($"Prefab for {kvp.Key} is null. Please assign a valid prefab in the inspector.");
                            continue;
                        }

                        buffer.Add(new PrefabElement(kvp.Key, GetEntity(kvp.Value, TransformUsageFlags.None)));
                    }
                }
                else
                    Debug.LogWarning("Prefabs value is null. Please assign value in the inspector.");

                AddComponent<PrefabsContainer>(entity);
            }
        }
    }

    public struct PrefabsContainer : IComponentData { }

    [InternalBufferCapacity(100)]
    public struct PrefabElement : IBufferElementData
    {
        public PrefabType Type;
        public Entity Entity;

        public PrefabElement(PrefabType type, Entity entity)
        {
            Type = type;
            Entity = entity;
        }
    }

    public enum PrefabType { }
}