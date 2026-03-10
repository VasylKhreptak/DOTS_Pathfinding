using Unity.Entities;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Gameplay.Components
{
    [DisallowMultipleComponent]
    public class RandomComponentAuthoring : MonoBehaviour
    {
        private class RandomComponentBaker : Baker<RandomComponentAuthoring>
        {
            public override void Bake(RandomComponentAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent<RandomComponent>(entity);
                AddComponent<RandomNeedsInitializationFlag>(entity);
            }
        }
    }

    public struct RandomComponent : IComponentData
    {
        public Random Value;
    }

    public struct RandomNeedsInitializationFlag : IComponentData, IEnableableComponent { }
}