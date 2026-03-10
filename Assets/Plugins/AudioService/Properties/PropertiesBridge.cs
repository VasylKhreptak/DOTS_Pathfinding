using System;
using System.Collections.Generic;
using Plugins.AudioService.Properties.Core;
using Plugins.Timer;
using UnityEngine;
using UnityEngine.Audio;

namespace Plugins.AudioService.Properties
{
    public class PropertiesBridge
    {
        public PropertiesBridge(Dictionary<int, AudioService.PooledObject> activePool)
        {
            Func<int, bool> canAccess = activePool.ContainsKey;

            Timer = new ReadonlyProperty<IReadonlyTimer>(canAccess, id => activePool[id].Audio.Timer);
            Time = new Property<float>(canAccess, id => activePool[id].Audio.Time, (id, value) => activePool[id].Audio.Time = value);
            Position = new Property<Vector3>(canAccess, id => activePool[id].Audio.Position, (id, value) => activePool[id].Audio.Position = value);
            Rotation = new Property<Quaternion>(canAccess, id => activePool[id].Audio.Rotation, (id, value) => activePool[id].Audio.Rotation = value);
            Clip = new Property<AudioClip>(canAccess, id => activePool[id].Audio.Clip, (id, value) => activePool[id].Audio.Clip = value);
            AudioMixerGroup = new Property<AudioMixerGroup>(canAccess, id => activePool[id].Audio.AudioMixerGroup, (id, value) => activePool[id].Audio.AudioMixerGroup = value);
            Mute = new Property<bool>(canAccess, id => activePool[id].Audio.Mute, (id, value) => activePool[id].Audio.Mute = value);
            BypassEffects = new Property<bool>(canAccess, id => activePool[id].Audio.BypassEffects, (id, value) => activePool[id].Audio.BypassEffects = value);
            BypassListenerEffects = new Property<bool>(canAccess, id => activePool[id].Audio.BypassListenerEffects, (id, value) => activePool[id].Audio.BypassListenerEffects = value);
            BypassReverbZones = new Property<bool>(canAccess, id => activePool[id].Audio.BypassReverbZones, (id, value) => activePool[id].Audio.BypassReverbZones = value);
            Loop = new Property<bool>(canAccess, id => activePool[id].Audio.Loop, (id, value) => activePool[id].Audio.Loop = value);
            Priority = new Property<int>(canAccess, id => activePool[id].Audio.Priority, (id, value) => activePool[id].Audio.Priority = value);
            Volume = new Property<float>(canAccess, id => activePool[id].Audio.Volume, (id, value) => activePool[id].Audio.Volume = value);
            Pitch = new Property<float>(canAccess, id => activePool[id].Audio.Pitch, (id, value) => activePool[id].Audio.Pitch = value);
            StereoPan = new Property<float>(canAccess, id => activePool[id].Audio.StereoPan, (id, value) => activePool[id].Audio.StereoPan = value);
            SpatialBlend = new Property<float>(canAccess, id => activePool[id].Audio.SpatialBlend, (id, value) => activePool[id].Audio.SpatialBlend = value);
            ReverbZoneMix = new Property<float>(canAccess, id => activePool[id].Audio.ReverbZoneMix, (id, value) => activePool[id].Audio.ReverbZoneMix = value);
            DopplerLevel = new Property<float>(canAccess, id => activePool[id].Audio.DopplerLevel, (id, value) => activePool[id].Audio.DopplerLevel = value);
            Spread = new Property<float>(canAccess, id => activePool[id].Audio.Spread, (id, value) => activePool[id].Audio.Spread = value);
            RolloffMode = new Property<AudioRolloffMode>(canAccess, id => activePool[id].Audio.RolloffMode, (id, value) => activePool[id].Audio.RolloffMode = value);
            MinDistance = new Property<float>(canAccess, id => activePool[id].Audio.MinDistance, (id, value) => activePool[id].Audio.MinDistance = value);
            MaxDistance = new Property<float>(canAccess, id => activePool[id].Audio.MaxDistance, (id, value) => activePool[id].Audio.MaxDistance = value);
        }

        public IReadonlyProperty<IReadonlyTimer> Timer { get; }
        public IProperty<float> Time { get; }
        public IProperty<Vector3> Position { get; }
        public IProperty<Quaternion> Rotation { get; }
        public IProperty<AudioClip> Clip { get; }
        public IProperty<AudioMixerGroup> AudioMixerGroup { get; }
        public IProperty<bool> Mute { get; }
        public IProperty<bool> BypassEffects { get; }
        public IProperty<bool> BypassListenerEffects { get; }
        public IProperty<bool> BypassReverbZones { get; }
        public IProperty<bool> Loop { get; }
        public IProperty<int> Priority { get; }
        public IProperty<float> Volume { get; }
        public IProperty<float> Pitch { get; }
        public IProperty<float> StereoPan { get; }
        public IProperty<float> SpatialBlend { get; }
        public IProperty<float> ReverbZoneMix { get; }
        public IProperty<float> DopplerLevel { get; }
        public IProperty<float> Spread { get; }
        public IProperty<AudioRolloffMode> RolloffMode { get; }
        public IProperty<float> MinDistance { get; }
        public IProperty<float> MaxDistance { get; }
    }
}