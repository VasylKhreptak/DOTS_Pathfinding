namespace Plugins.Extensions
{
    public static class BoolExtensions
    {
        public static bool Toggle(this bool value) => !value;

        public static int ToSign(this bool value) => value ? 1 : -1;

        public static int ToInt(this bool value) => value ? 1 : 0;
    }
}