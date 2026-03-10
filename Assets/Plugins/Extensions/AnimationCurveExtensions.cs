using UnityEngine;

namespace Plugins.Extensions
{
    public static class AnimationCurveExtensions
    {
        public static float Evaluate(this AnimationCurve curve, float min, float max, float i) => curve.Evaluate(i) * (max - min) + min;

        public static float Evaluate(this AnimationCurve curve, float @in, float minIn, float maxIn, float minOut, float maxOut) =>
            curve.Evaluate(minOut, maxOut, @in.Remap(minIn, maxIn, 0f, 1f));
    }
}