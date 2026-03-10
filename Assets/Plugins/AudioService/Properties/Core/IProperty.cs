namespace Plugins.AudioService.Properties.Core
{
    public interface IProperty<TValue> : IReadonlyProperty<TValue>
    {
        public void SetValue(int id, TValue value);
    }
}