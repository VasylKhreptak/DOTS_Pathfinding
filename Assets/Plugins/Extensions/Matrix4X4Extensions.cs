using UnityEngine;

namespace Plugins.Extensions
{
    public static class Matrix4X4Extensions
    {
        public static Vector3 GetPosition(this Matrix4x4 matrix) => matrix.GetColumn(3);

        public static Matrix4x4 SetPosition(this Matrix4x4 matrix, Vector3 position)
        {
            matrix.SetColumn(3, new Vector4(position.x, position.y, position.z, 1));
            return matrix;
        }

        public static Quaternion GetRotation(this Matrix4x4 matrix) => matrix.rotation;

        public static Matrix4x4 SetRotation(this Matrix4x4 matrix, Quaternion rotation)
        {
            Vector3 scale = matrix.GetScale();
            matrix.SetColumn(0, (rotation * Vector3.right) * scale.x);
            matrix.SetColumn(1, (rotation * Vector3.up) * scale.y);
            matrix.SetColumn(2, (rotation * Vector3.forward) * scale.z);
            return matrix;
        }

        public static Vector3 GetScale(this Matrix4x4 matrix) => matrix.lossyScale;

        public static Matrix4x4 SetScale(this Matrix4x4 matrix, Vector3 scale)
        {
            matrix.SetColumn(0, matrix.GetColumn(0).normalized * scale.x);
            matrix.SetColumn(1, matrix.GetColumn(1).normalized * scale.y);
            matrix.SetColumn(2, matrix.GetColumn(2).normalized * scale.z);
            return matrix;
        }

        public static Vector3 GetRight(this Matrix4x4 matrix) => matrix.GetColumn(0);

        public static Vector3 GetUp(this Matrix4x4 matrix) => matrix.GetColumn(1);

        public static Vector3 GetForward(this Matrix4x4 matrix) => matrix.GetColumn(2);
    }
}