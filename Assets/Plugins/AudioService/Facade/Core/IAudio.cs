using Plugins.Timer;
using UnityEngine;
using UnityEngine.Audio;
using AudioSettings = Plugins.AudioService.Data.AudioSettings;

namespace Plugins.AudioService.Facade.Core
{
    public interface IAudio : IReadonlyAudio
    {
        public void Play();
        public void Stop();
        public void Pause();
        public void Resume();
        public void ApplySettings(AudioSettings settings);

        public new IReadonlyTimer Timer { get; }
        public new float Time { get; set; }
        public new Vector3 Position { get; set; }
        public new Quaternion Rotation { get; set; }
        public new AudioClip Clip { get; set; }
        public new AudioMixerGroup AudioMixerGroup { get; set; }
        public new bool Mute { get; set; }
        public new bool BypassEffects { get; set; }
        public new bool BypassListenerEffects { get; set; }
        public new bool BypassReverbZones { get; set; }
        public new bool Loop { get; set; }
        public new bool IsPlaying { get; }
        public new int Priority { get; set; }
        public new float Volume { get; set; }
        public new float Pitch { get; set; }
        public new float StereoPan { get; set; }
        public new float SpatialBlend { get; set; }
        public new float ReverbZoneMix { get; set; }
        public new float DopplerLevel { get; set; }
        public new float Spread { get; set; }
        public new AudioRolloffMode RolloffMode { get; set; }
        public new float MinDistance { get; set; }
        public new float MaxDistance { get; set; }
    }
}