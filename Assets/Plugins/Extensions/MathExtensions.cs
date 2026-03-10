using Unity.Mathematics;

namespace Plugins.Extensions
{
    public static class MathExtensions
    {
        public static float2 ClampMagnitude(float2 vector, float maxLength)
        {
            float len = math.length(vector);

            if (len > maxLength)
                return vector * (maxLength / len);

            return vector;
        }

        public static float3 ClampMagnitude(float3 vector, float maxLength)
        {
            float len = math.length(vector);

            if (len > maxLength)
                return vector * (maxLength / len);

            return vector;
        }

        public static float2 ClampMagnitude(float2 vector, float minLength, float maxLength)
        {
            float len = math.length(vector);

            if (len < minLength)
                return math.normalizesafe(vector) * minLength;

            if (len > maxLength)
                return vector * (maxLength / len);

            return vector;
        }

        public static float3 ClampMagnitude(float3 vector, float minLength, float maxLength)
        {
            float len = math.length(vector);

            if (len < minLength)
                return math.normalizesafe(vector) * minLength;

            if (len > maxLength)
                return vector * (maxLength / len);

            return vector;
        }
    }
}