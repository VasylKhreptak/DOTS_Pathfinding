namespace Infrastructure.Services.LateTickable.Core
{
    public interface ILateTickable
    {
        public void LateTick();
    }
}