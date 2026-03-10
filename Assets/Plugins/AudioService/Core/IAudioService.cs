using System;
using Plugins.AudioService.Data;
using Plugins.AudioService.Facade.Core;
using Plugins.AudioService.Properties;
using UnityEngine;
using AudioSettings = Plugins.AudioService.Data.AudioSettings;

namespace Plugins.AudioService.Core
{
    public interface IAudioService
    {
        public PropertiesBridge Properties { get; }

        public int Play(AudioClip clip, Vector3 position, Quaternion rotation, AudioSettings settings);
        public int Play(AudioClip clip, Vector3 position, AudioSettings settings);
        public int Play(AudioClip clip, AudioSettings settings);
        public int Play(AudioClip clip, Vector3 position);
        public int Play(SoundConfig sound, Vector3 position, Quaternion rotation);
        public int Play(SoundConfig sound, Vector3 position);
        public int Play(SoundConfig sound);
        public void Pause(int id);
        public void PauseAll();
        public void PauseAll(Func<IReadonlyAudio, bool> predicate);
        public void Resume(int id);
        public void ResumeAll();
        public void ResumeAll(Func<IReadonlyAudio, bool> predicate);
        public void Stop(int id);
        public void StopAll();
        public void StopAll(Func<IReadonlyAudio, bool> predicate);
        public bool IsActive(int id);
        public int ActiveAudiosCount();
        public int ActiveAudiosCount(Func<IReadonlyAudio, bool> predicate);
        public void ApplySettings(int id, AudioSettings settings);
    }
}