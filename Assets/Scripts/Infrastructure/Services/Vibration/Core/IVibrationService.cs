namespace Infrastructure.Services.Vibration.Core
{
    public interface IVibrationService
    {
        public void Vibrate(VibrationPreset preset);

        public void Vibrate(float intensity, float duration);

        public void Stop();

        public void SetIntensity(float intensity);
    }
}