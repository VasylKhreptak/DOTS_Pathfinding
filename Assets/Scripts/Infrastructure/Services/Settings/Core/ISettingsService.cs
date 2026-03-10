using Infrastructure.Data.Models.Persistent.Data;

namespace Infrastructure.Services.Settings.Core
{
    public interface ISettingsService
    {
        public float VibrationIntensity { get; set; }
        public float MasterVolume { get; set; }
        public float MusicVolume { get; set; }
        public float SoundVolume { get; set; }

        public void ApplySettings(SettingsData settingsData);

        public SettingsData GetSettings();
    }
}