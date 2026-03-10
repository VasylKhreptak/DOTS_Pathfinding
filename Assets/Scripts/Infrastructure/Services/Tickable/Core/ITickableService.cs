namespace Infrastructure.Services.Tickable.Core
{
    public interface ITickableService
    {
        public void Add(ITickable tickable);

        public void Remove(ITickable tickable);
    }
}