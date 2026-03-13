using Unity.Mathematics;

namespace Plugins.Extensions
{
    public static class AABBExtensions
    {
        public static bool Overlaps(this AABB a, AABB b)
        {
            float3 distance = math.abs(a.Center - b.Center);
            float3 totalExtents = a.Extents + b.Extents;

            return math.all(distance <= totalExtents);
        }

        public static AABB ToWorld(this AABB a, float4x4 localToWorld)
        {
            float3 worldCenter = math.transform(localToWorld, a.Center);

            float3 worldExtents =
                math.abs(localToWorld.c0.xyz) * a.Extents.x +
                math.abs(localToWorld.c1.xyz) * a.Extents.y +
                math.abs(localToWorld.c2.xyz) * a.Extents.z;

            return new AABB
            {
                Center = worldCenter,
                Extents = worldExtents
            };
        }
    }
}