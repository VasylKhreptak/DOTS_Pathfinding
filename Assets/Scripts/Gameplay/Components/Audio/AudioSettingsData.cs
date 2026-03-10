using Unity.Entities;
using UnityEngine;
using UnityEngine.Audio;
using AudioSettings = Plugins.AudioService.Data.AudioSettings;

namespace Gameplay.Components.Audio
{
    public struct AudioSettingsData
    {
        public UnityObjectRef<AudioMixerGroup> AudioMixerGroup;
        public bool Mute;
        public bool BypassEffects;
        public bool BypassListenerEffects;
        public bool BypassReverbZones;
        public bool Loop;
        public int Priority;
        public float Volume;
        public float Pitch;
        public float StereoPan;
        public float SpatialBlend;
        public float ReverbZoneMix;
        public float DopplerLevel;
        public float Spread;
        public AudioRolloffMode RolloffMode;
        public float MinDistance;
        public float MaxDistance;

        public static implicit operator AudioSettingsData(AudioSettings settings) =>
            new AudioSettingsData
            {
                AudioMixerGroup = settings.AudioMixerGroup,
                Mute = settings.Mute,
                BypassEffects = settings.BypassEffects,
                BypassListenerEffects = settings.BypassListenerEffects,
                BypassReverbZones = settings.BypassReverbZones,
                Loop = settings.Loop,
                Priority = settings.Priority,
                Volume = settings.Volume,
                Pitch = settings.Pitch,
                StereoPan = settings.StereoPan,
                SpatialBlend = settings.SpatialBlend,
                ReverbZoneMix = settings.ReverbZoneMix,
                DopplerLevel = settings.DopplerLevel,
                Spread = settings.Spread,
                RolloffMode = settings.RolloffMode,
                MinDistance = settings.MinDistance,
                MaxDistance = settings.MaxDistance
            };

        public static implicit operator AudioSettings(AudioSettingsData data) =>
            new AudioSettings
            {
                AudioMixerGroup = data.AudioMixerGroup,
                Mute = data.Mute,
                BypassEffects = data.BypassEffects,
                BypassListenerEffects = data.BypassListenerEffects,
                BypassReverbZones = data.BypassReverbZones,
                Loop = data.Loop,
                Priority = data.Priority,
                Volume = data.Volume,
                Pitch = data.Pitch,
                StereoPan = data.StereoPan,
                SpatialBlend = data.SpatialBlend,
                ReverbZoneMix = data.ReverbZoneMix,
                DopplerLevel = data.DopplerLevel,
                Spread = data.Spread,
                RolloffMode = data.RolloffMode,
                MinDistance = data.MinDistance,
                MaxDistance = data.MaxDistance
            };
    }
}