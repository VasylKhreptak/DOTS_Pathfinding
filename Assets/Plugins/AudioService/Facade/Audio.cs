using System;
using Plugins.AudioService.Facade.Core;
using Plugins.Timer;
using UniRx;
using UnityEngine;
using UnityEngine.Audio;
using AudioSettings = Plugins.AudioService.Data.AudioSettings;

namespace Plugins.AudioService.Facade
{
    public class Audio : IAudio
    {
        private readonly AudioSource _source;
        private readonly GameObject _gameObject;
        private readonly Transform _transform;
        private readonly ITimer _timer;

        public Audio(AudioSource source)
        {
            _source = source;
            _gameObject = _source.gameObject;
            _transform = _gameObject.transform;
            _timer = new Timer.Timer();
            _timer.UpdateMethod = UpdateMethod.UnscaledDeltaTime;
            StartObservingTimer();
        }

        #region Properties

        public IReadonlyTimer Timer => _timer;

        public float Time
        {
            get => _source.time;
            set
            {
                value = Mathf.Clamp(value, 0, Clip.length - 0.01f);

                _source.time = value;
                _timer.SetTime(TimeSpan.FromSeconds(value));
            }
        }

        public Vector3 Position
        {
            get => _transform.position;
            set => _transform.position = value;
        }

        public Quaternion Rotation
        {
            get => _transform.rotation;
            set => _transform.rotation = value;
        }

        public AudioClip Clip
        {
            get => _source.clip;
            set => _source.clip = value;
        }

        public AudioMixerGroup AudioMixerGroup
        {
            get => _source.outputAudioMixerGroup;
            set => _source.outputAudioMixerGroup = value;
        }

        public bool Mute
        {
            get => _source.mute;
            set => _source.mute = value;
        }

        public bool BypassEffects
        {
            get => _source.bypassEffects;
            set => _source.bypassEffects = value;
        }

        public bool BypassListenerEffects
        {
            get => _source.bypassListenerEffects;
            set => _source.bypassListenerEffects = value;
        }

        public bool BypassReverbZones
        {
            get => _source.bypassReverbZones;
            set => _source.bypassReverbZones = value;
        }

        public bool Loop
        {
            get => _source.loop;
            set => _source.loop = value;
        }

        public bool IsPlaying => _source.isPlaying;

        public int Priority
        {
            get => _source.priority;
            set => _source.priority = value;
        }

        public float Volume
        {
            get => _source.volume;
            set => _source.volume = value;
        }

        public float Pitch
        {
            get => _source.pitch;
            set
            {
                _timer.SetTimeScale(value);
                _source.pitch = value;
            }
        }

        public float StereoPan
        {
            get => _source.panStereo;
            set => _source.panStereo = value;
        }

        public float SpatialBlend
        {
            get => _source.spatialBlend;
            set => _source.spatialBlend = value;
        }

        public float ReverbZoneMix
        {
            get => _source.reverbZoneMix;
            set => _source.reverbZoneMix = value;
        }

        public float DopplerLevel
        {
            get => _source.dopplerLevel;
            set => _source.dopplerLevel = value;
        }

        public float Spread
        {
            get => _source.spread;
            set => _source.spread = value;
        }

        public AudioRolloffMode RolloffMode
        {
            get => _source.rolloffMode;
            set => _source.rolloffMode = value;
        }

        public float MinDistance
        {
            get => _source.minDistance;
            set => _source.minDistance = value;
        }

        public float MaxDistance
        {
            get => _source.maxDistance;
            set => _source.maxDistance = value;
        }

        #endregion

        public void Play()
        {
            _gameObject.SetActive(true);
            _source.Play();
            _timer.Start(TimeSpan.FromSeconds(Clip.length));
            _timer.SetTimeScale(Pitch);
        }

        public void Stop()
        {
            _source.Stop();
            _gameObject.SetActive(false);
            _timer.Stop();
        }

        public void Pause()
        {
            _source.Pause();
            _timer.Pause();
        }

        public void Resume()
        {
            _source.UnPause();
            _timer.Resume();
        }

        public void ApplySettings(AudioSettings settings)
        {
            AudioMixerGroup = settings.AudioMixerGroup;
            Mute = settings.Mute;
            BypassEffects = settings.BypassEffects;
            BypassListenerEffects = settings.BypassListenerEffects;
            BypassReverbZones = settings.BypassReverbZones;
            Loop = settings.Loop;
            Priority = settings.Priority;
            Volume = settings.Volume;
            Pitch = settings.Pitch;
            StereoPan = settings.StereoPan;
            SpatialBlend = settings.SpatialBlend;
            ReverbZoneMix = settings.ReverbZoneMix;
            DopplerLevel = settings.DopplerLevel;
            Spread = settings.Spread;
            RolloffMode = settings.RolloffMode;
            MinDistance = settings.MinDistance;
            MaxDistance = settings.MaxDistance;
        }

        private void StartObservingTimer()
        {
            _timer.OnCompleted.Subscribe(_ =>
            {
                if (Loop == false)
                    Stop();
                else
                {
                    _timer.Start(TimeSpan.FromSeconds(Clip.length));
                    _timer.SetTimeScale(Pitch);
                }
            });
        }
    }
}