using UnityEngine;

namespace Plugins.Extensions
{
    public static class Vector3Extensions
    {
        public static Vector3 WithX(this Vector3 vector, float x) => new Vector3(x, vector.y, vector.z);

        public static Vector3 WithY(this Vector3 vector, float y) => new Vector3(vector.x, y, vector.z);

        public static Vector3 WithZ(this Vector3 vector, float z) => new Vector3(vector.x, vector.y, z);

        public static Vector3 WithVector2(this Vector3 vector, Vector2 other) => new Vector3(other.x, other.y, vector.z);

        public static bool IsCloseTo(this Vector3 vector, Vector3 other, float tolerance = 0.01f) => Vector3.Distance(vector, other) <= tolerance;
    }
}