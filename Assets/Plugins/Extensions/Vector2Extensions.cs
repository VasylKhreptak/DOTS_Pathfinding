using UnityEngine;

namespace Plugins.Extensions
{
    public static class Vector2Extensions
    {
        public static Vector2 WithX(this Vector2 vector, float x) => new Vector2(x, vector.y);

        public static Vector2 WithY(this Vector2 vector, float y) => new Vector2(vector.x, y);

        public static bool IsCloseTo(this Vector2 vector, Vector2 other, float tolerance = 0.01f) => Vector2.Distance(vector, other) <= tolerance;
    }
}