using Infrastructure.Data.Models.Persistent.Data;
using Infrastructure.Services.Settings.Core;
using Infrastructure.Services.Vibration.Core;
using Plugins.Extensions;
using UnityEngine.Audio;

namespace Infrastructure.Services.Settings
{
    public class SettingsService : ISettingsService
    {
        private const string MasterVolumeKey = "Master_Volume";
        private const string MusicVolumeKey = "Music_Volume";
        private const string SoundVolumeKey = "Sound_Volume";

        private readonly IVibrationService _vibrationService;
        private readonly AudioMixer _audioMixer;

        public SettingsService(IVibrationService vibrationService, AudioMixer audioMixer)
        {
            _vibrationService = vibrationService;
            _audioMixer = audioMixer;
        }

        private readonly SettingsData _settingsData = new SettingsData();

        public float VibrationIntensity
        {
            get => _settingsData.VibrationIntensity;
            set
            {
                _vibrationService.SetIntensity(value * value);
                _settingsData.VibrationIntensity = value;
            }
        }

        public float MasterVolume
        {
            get => _settingsData.MasterVolume;
            set
            {
                _audioMixer.SetFloat(MasterVolumeKey, AudioExtensions.ToDB(value));
                _settingsData.MasterVolume = value;
            }
        }

        public float MusicVolume
        {
            get => _settingsData.MusicVolume;
            set
            {
                _audioMixer.SetFloat(MusicVolumeKey, AudioExtensions.ToDB(value));
                _settingsData.MusicVolume = value;
            }
        }

        public float SoundVolume
        {
            get => _settingsData.SoundVolume;
            set
            {
                _audioMixer.SetFloat(SoundVolumeKey, AudioExtensions.ToDB(value));
                _settingsData.SoundVolume = value;
            }
        }

        public void ApplySettings(SettingsData settings)
        {
            VibrationIntensity = settings.VibrationIntensity;
            MasterVolume = settings.MasterVolume;
            MusicVolume = settings.MusicVolume;
            SoundVolume = settings.SoundVolume;
        }

        public SettingsData GetSettings() => new SettingsData(_settingsData);
    }
}