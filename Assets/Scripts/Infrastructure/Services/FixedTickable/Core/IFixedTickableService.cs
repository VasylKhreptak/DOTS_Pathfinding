namespace Infrastructure.Services.FixedTickable.Core
{
    public interface IFixedTickableService
    {
        public void Add(IFixedTickable fixedTickable);

        public void Remove(IFixedTickable fixedTickable);
    }
}