using Unity.Mathematics;

namespace Plugins.Extensions
{
    public static class Float4X4Extensions
    {
        public static float3 GetPosition(this float4x4 matrix) => matrix.c3.xyz;

        public static float4x4 SetPosition(this float4x4 matrix, float3 position)
        {
            matrix.c3 = new float4(position, 1f);
            return matrix;
        }

        public static quaternion GetRotation(this float4x4 matrix)
        {
            return math.quaternion(new float3x3(math.normalize(matrix.c0.xyz),
                math.normalize(matrix.c1.xyz),
                math.normalize(matrix.c2.xyz)));
        }

        public static float4x4 SetRotation(this float4x4 matrix, quaternion rotation)
        {
            float3 scale = matrix.GetScale();
            float3x3 rotMatrix = math.float3x3(rotation);
            matrix.c0.xyz = rotMatrix.c0 * scale.x;
            matrix.c1.xyz = rotMatrix.c1 * scale.y;
            matrix.c2.xyz = rotMatrix.c2 * scale.z;
            return matrix;
        }

        public static float3 GetScale(this float4x4 matrix)
        {
            return new float3(math.length(matrix.c0.xyz),
                math.length(matrix.c1.xyz),
                math.length(matrix.c2.xyz));
        }

        public static float4x4 SetScale(this float4x4 matrix, float3 scale)
        {
            matrix.c0.xyz = math.normalize(matrix.c0.xyz) * scale.x;
            matrix.c1.xyz = math.normalize(matrix.c1.xyz) * scale.y;
            matrix.c2.xyz = math.normalize(matrix.c2.xyz) * scale.z;
            return matrix;
        }

        public static float3 GetRight(this float4x4 matrix) => matrix.c0.xyz;

        public static float3 GetUp(this float4x4 matrix) => matrix.c1.xyz;

        public static float3 GetForward(this float4x4 matrix) => matrix.c2.xyz;
    }
}