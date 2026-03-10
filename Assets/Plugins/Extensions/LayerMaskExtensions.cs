using Unity.Physics;
using UnityEngine;

namespace Plugins.Extensions
{
    public static class LayerMaskExtensions
    {
        public static bool ContainsLayer(this LayerMask layerMask, int layerID) => (layerMask.value & (1 << layerID)) > 0;

        public static CollisionFilter ToCollisionFilter(this LayerMask mask)
        {
            return new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = (uint)mask.value,
                GroupIndex = 0
            };
        }
    }
}