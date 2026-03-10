using Unity.Mathematics;
using Unity.Physics.GraphicsIntegration;

namespace Plugins.Extensions
{
    public static class PhysicsGraphicalInterpolationBufferExtensions
    {
        public static PhysicsGraphicalInterpolationBuffer WithPosition(this PhysicsGraphicalInterpolationBuffer buffer, float3 position)
        {
            buffer.PreviousTransform.pos = position;

            return buffer;
        }
    }
}