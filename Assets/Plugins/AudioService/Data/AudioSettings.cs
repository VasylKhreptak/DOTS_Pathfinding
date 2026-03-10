using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;

namespace Plugins.AudioService.Data
{
    [Serializable]
    public class AudioSettings
    {
        public AudioMixerGroup AudioMixerGroup;
        public bool Mute;
        public bool BypassEffects;
        public bool BypassListenerEffects;
        public bool BypassReverbZones;
        public bool Loop;
        [Range(0, 256)] public int Priority = 128;
        [Range(0, 1)] public float Volume = 1f;
        [Range(-3, 3)] public float Pitch = 1f;
        [Range(-1, 1)] public float StereoPan;
        [Range(0, 1)] public float SpatialBlend = 1f;
        [Range(0, 1.1f)] public float ReverbZoneMix = 1f;
        [Range(0, 5f)] public float DopplerLevel = 1f;
        [Range(0, 360)] public float Spread;
        public AudioRolloffMode RolloffMode = AudioRolloffMode.Logarithmic;
        [MinValue(0)] [MaxValue(nameof(MaxDistance))] public float MinDistance = 1f;
        [MinValue(0)] public float MaxDistance = 500f;
    }
}