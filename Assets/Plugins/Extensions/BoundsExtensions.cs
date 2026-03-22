using UnityEngine;

namespace Plugins.Extensions
{
    public static class BoundsExtensions
    {
        public static bool Contains(this Bounds a, Bounds b)
        {
            return a.min.x <= b.min.x &&
                   a.min.y <= b.min.y &&
                   a.min.z <= b.min.z &&
                   a.max.x >= b.max.x &&
                   a.max.y >= b.max.y &&
                   a.max.z >= b.max.z;
        }
    }
}