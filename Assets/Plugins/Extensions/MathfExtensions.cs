using UnityEngine;

namespace Plugins.Extensions
{
    public static class MathfExtensions
    {
        public static bool Probability(float probability) => Random.value < Mathf.Clamp01(probability);
    }
}