namespace Plugins.Extensions
{
    public static class FloatExtensions
    {
        public static bool IsBetween(this float value, float min, float max) => value >= min && value <= max;

        public static float Remap(this float value, float from1, float to1, float from2, float to2) => (value - from1) / (to1 - from1) * (to2 - from2) + from2;

        public static bool AsProbability(this float value) => MathfExtensions.Probability(value);
    }
}