namespace Infrastructure.Services.Input.Core
{
    public interface IInputAction<out T>
    {
        public bool Enabled { get; set; }

        public T Value { get; }
    }
}