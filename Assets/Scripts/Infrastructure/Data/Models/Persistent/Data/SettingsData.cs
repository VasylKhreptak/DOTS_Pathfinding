namespace Infrastructure.Data.Models.Persistent.Data
{
    public class SettingsData
    {
        public float VibrationIntensity = 0.5f;
        public float MasterVolume = 1f;
        public float MusicVolume = 1f;
        public float SoundVolume = 1f;

        public SettingsData() { }

        public SettingsData(SettingsData settingsData)
        {
            VibrationIntensity = settingsData.VibrationIntensity;
            MasterVolume = settingsData.MasterVolume;
            MusicVolume = settingsData.MusicVolume;
            SoundVolume = settingsData.SoundVolume;
        }
    }
}