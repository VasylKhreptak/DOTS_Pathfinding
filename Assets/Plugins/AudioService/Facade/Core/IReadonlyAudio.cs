using Plugins.Timer;
using UnityEngine;
using UnityEngine.Audio;

namespace Plugins.AudioService.Facade.Core
{
    public interface IReadonlyAudio
    {
        public IReadonlyTimer Timer { get; }
        public float Time { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public AudioClip Clip { get; }
        public AudioMixerGroup AudioMixerGroup { get; }
        public bool Mute { get; }
        public bool BypassEffects { get; }
        public bool BypassListenerEffects { get; }
        public bool BypassReverbZones { get; }
        public bool Loop { get; }
        public bool IsPlaying { get; }
        public int Priority { get; }
        public float Volume { get; }
        public float Pitch { get; }
        public float StereoPan { get; }
        public float SpatialBlend { get; }
        public float ReverbZoneMix { get; }
        public float DopplerLevel { get; }
        public float Spread { get; }
        public AudioRolloffMode RolloffMode { get; }
        public float MinDistance { get; }
        public float MaxDistance { get; }
    }
}