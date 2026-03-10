namespace Infrastructure.Services.TimeScale.Core
{
    public interface ITimeScaleService
    {
        public void PauseTime();

        public void ResumeTime();
    }
}