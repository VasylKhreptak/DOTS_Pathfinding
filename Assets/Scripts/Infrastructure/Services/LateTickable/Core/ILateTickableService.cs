namespace Infrastructure.Services.LateTickable.Core
{
    public interface ILateTickableService
    {
        public void Add(ILateTickable lateTickable);

        public void Remove(ILateTickable lateTickable);
    }
}