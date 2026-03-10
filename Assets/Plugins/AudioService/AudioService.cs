using System;
using System.Collections.Generic;
using System.Linq;
using Plugins.AudioService.Core;
using Plugins.AudioService.Data;
using Plugins.AudioService.Facade;
using Plugins.AudioService.Facade.Core;
using Plugins.AudioService.Properties;
using Plugins.AudioService.Services.ID;
using Plugins.AudioService.Services.ID.Core;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using AudioSettings = Plugins.AudioService.Data.AudioSettings;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Plugins.AudioService
{
    public class AudioService : IAudioService
    {
        private readonly Preferences _preferences;
        private readonly IIDService _idService;
        private readonly Transform _root;

        public AudioService(Preferences preferences)
        {
            _preferences = preferences;
            _idService = new IDService();

            GameObject gameObject = new GameObject("AudioService");
            Object.DontDestroyOnLoad(gameObject);
            _root = gameObject.transform;

            AudioConfiguration audioConfiguration = UnityEngine.AudioSettings.GetConfiguration();

            if (_preferences.MaxSize > audioConfiguration.numVirtualVoices)
            {
                Debug.LogWarning("MaxSize is greater than the number of virtual voices. MaxSize will be set to the number of virtual voices.");
                _preferences.MaxSize = audioConfiguration.numVirtualVoices;
            }

            _preferences.InitialSize = Mathf.Min(_preferences.InitialSize, _preferences.MaxSize);

            Properties = new PropertiesBridge(_activePool);

            Initialize();
        }

        private readonly HashSet<PooledObject> _totalPool = new HashSet<PooledObject>();
        private readonly Dictionary<int, PooledObject> _activePool = new Dictionary<int, PooledObject>();
        private readonly HashSet<PooledObject> _inactivePool = new HashSet<PooledObject>();

        public PropertiesBridge Properties { get; }

        public int Play(AudioClip clip, Vector3 position, Quaternion rotation, AudioSettings settings)
        {
            PooledObject pooledObject = GetFree();
            IAudio audio = pooledObject.Audio;
            audio.Clip = clip;
            audio.Position = position;
            audio.Rotation = rotation;
            audio.ApplySettings(settings);
            audio.Play();
            return pooledObject.ID;
        }

        public int Play(AudioClip clip, Vector3 position, AudioSettings settings) => Play(clip, position, Quaternion.identity, settings);

        public int Play(AudioClip clip, AudioSettings settings) => Play(clip, Vector3.zero, Quaternion.identity, settings);

        public int Play(AudioClip clip, Vector3 position) => Play(clip, position, Quaternion.identity, _preferences.DefaultSettings);

        public int Play(SoundConfig sound, Vector3 position, Quaternion rotation)
        {
            int id = Play(sound.RandomClip, position, rotation, sound.Settings);

            if (sound.PitchShift != 0)
                Properties.Pitch.SetValue(id, sound.Settings.Pitch + Random.Range(-sound.PitchShift, sound.PitchShift));

            return id;
        }

        public int Play(SoundConfig sound, Vector3 position) => Play(sound, position, Quaternion.identity);

        public int Play(SoundConfig sound) => Play(sound, Vector3.zero, Quaternion.identity);

        public void Pause(int id)
        {
            if (_activePool.TryGetValue(id, out PooledObject pooledObject))
                pooledObject.Audio.Pause();
        }

        public void PauseAll()
        {
            foreach (PooledObject pooledObject in _activePool.Values)
            {
                pooledObject.Audio.Pause();
            }
        }

        public void PauseAll(Func<IReadonlyAudio, bool> predicate)
        {
            foreach (PooledObject pooledObject in _activePool.Values)
            {
                if (predicate(pooledObject.Audio))
                    pooledObject.Audio.Pause();
            }
        }

        public void Resume(int id)
        {
            if (_activePool.TryGetValue(id, out PooledObject pooledObject))
                pooledObject.Audio.Resume();
        }

        public void ResumeAll()
        {
            foreach (PooledObject pooledObject in _activePool.Values)
            {
                pooledObject.Audio.Resume();
            }
        }

        public void ResumeAll(Func<IReadonlyAudio, bool> predicate)
        {
            foreach (PooledObject pooledObject in _activePool.Values)
            {
                if (predicate(pooledObject.Audio))
                    pooledObject.Audio.Resume();
            }
        }

        public void Stop(int id)
        {
            if (_activePool.TryGetValue(id, out PooledObject pooledObject))
                pooledObject.Audio.Stop();
        }

        public void StopAll()
        {
            foreach (PooledObject pooledObject in _activePool.Values.ToList())
            {
                pooledObject.Audio.Stop();
            }
        }

        public void StopAll(Func<IReadonlyAudio, bool> predicate)
        {
            foreach (PooledObject pooledObject in _activePool.Values.ToList())
            {
                if (predicate(pooledObject.Audio))
                    pooledObject.Audio.Stop();
            }
        }

        public bool IsActive(int id) => _activePool.ContainsKey(id);

        public int ActiveAudiosCount() => _activePool.Count;

        public int ActiveAudiosCount(Func<IReadonlyAudio, bool> predicate)
        {
            int count = 0;

            foreach (PooledObject pooledObject in _activePool.Values)
            {
                if (predicate(pooledObject.Audio))
                    count++;
            }

            return count;
        }

        public void ApplySettings(int id, AudioSettings settings)
        {
            if (_activePool.TryGetValue(id, out PooledObject pooledObject))
                pooledObject.Audio.ApplySettings(settings);
        }

        private void Initialize() => Expand(_preferences.InitialSize);

        private void Expand()
        {
            GameObject gameObject = new GameObject("Audio");
            gameObject.transform.SetParent(_root);
            gameObject.SetActive(false);

            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            IAudio audio = new Audio(audioSource);

            PooledObject pooledObject = new PooledObject
            {
                Audio = audio,
                GameObject = gameObject
            };

            _totalPool.Add(pooledObject);
            _inactivePool.Add(pooledObject);

            StartObserving(pooledObject);
        }

        private void Expand(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                Expand();
            }
        }

        private void StartObserving(PooledObject pooledObject)
        {
            pooledObject.Subscriptions.Add(pooledObject.GameObject.OnEnableAsObservable().Subscribe(_ => OnEnabled(pooledObject)));
            pooledObject.Subscriptions.Add(pooledObject.GameObject.OnDisableAsObservable().Subscribe(_ => OnDisabled(pooledObject)));
            pooledObject.Subscriptions.Add(pooledObject.GameObject.OnDestroyAsObservable().Subscribe(_ => OnDestroyed(pooledObject)));
        }

        private void StopObserving(PooledObject pooledObject) => pooledObject.Subscriptions.Dispose();

        private void OnEnabled(PooledObject pooledObject)
        {
            pooledObject.ID = _idService.Get();
            _activePool.Add(pooledObject.ID, pooledObject);
            _inactivePool.Remove(pooledObject);
        }

        private void OnDisabled(PooledObject pooledObject)
        {
            _activePool.Remove(pooledObject.ID);
            _inactivePool.Add(pooledObject);
        }

        private void OnDestroyed(PooledObject pooledObject)
        {
            StopObserving(pooledObject);
            _activePool.Remove(pooledObject.ID);
            _inactivePool.Remove(pooledObject);
            _totalPool.Remove(pooledObject);
            pooledObject.Audio.Stop();
        }

        private PooledObject GetFree()
        {
            if (_inactivePool.Count > 0)
                return MakeActive(_inactivePool.First());

            if (_totalPool.Count < _preferences.MaxSize)
            {
                Expand();
                return GetFree();
            }

            return MakeActive(GetLessImportant());
        }

        private PooledObject MakeActive(PooledObject pooledObject)
        {
            pooledObject.GameObject.SetActive(true);
            return pooledObject;
        }

        private PooledObject GetLessImportant()
        {
            int minPriority = int.MinValue;
            float maxPlayTime = float.MinValue;
            bool allPrioritiesAreEqual = true;

            PooledObject foundPooledObject = null;
            PooledObject lessImportantPooledObject = null;
            PooledObject longestPlayedPooledObject = null;

            ICollection<PooledObject> activePool = _activePool.Values;
            minPriority = activePool.First().Audio.Priority;

            foreach (PooledObject pooledObject in activePool)
            {
                if (pooledObject.Audio.Priority > minPriority)
                {
                    minPriority = pooledObject.Audio.Priority;
                    lessImportantPooledObject = pooledObject;
                    allPrioritiesAreEqual = false;
                }

                if (pooledObject.Audio.Timer.Time.TotalSeconds.Value > maxPlayTime)
                {
                    maxPlayTime = (float)pooledObject.Audio.Timer.Time.TotalSeconds.Value;
                    longestPlayedPooledObject = pooledObject;
                }
            }

            foundPooledObject = allPrioritiesAreEqual ? longestPlayedPooledObject : lessImportantPooledObject;

            foundPooledObject?.Audio.Stop();
            return foundPooledObject;
        }

        [Serializable]
        public class Preferences
        {
            public int InitialSize = 10;
            public int MaxSize = 50;
            public AudioSettings DefaultSettings;
        }

        [Serializable]
        public class PooledObject
        {
            public IAudio Audio;
            public GameObject GameObject;
            public int ID;
            public CompositeDisposable Subscriptions = new CompositeDisposable();
        }
    }
}