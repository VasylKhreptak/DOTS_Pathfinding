namespace Plugins.AudioService.Properties.Core
{
    public interface IReadonlyProperty<out TOut>
    {
        public bool IsAccessible(int id);
        public TOut GetValue(int id);
    }
}