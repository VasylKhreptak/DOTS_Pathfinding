using Unity.Entities;
using UnityEngine;
using AudioSettings = Plugins.AudioService.Data.AudioSettings;

namespace Gameplay.Components.Audio
{
    public class AudioSourceAuthoring : MonoBehaviour
    {
        [SerializeField] private bool _playOnAwake;
        [SerializeField] private AudioClip _audioClip;
        [SerializeField] private bool _followPosition;
        [SerializeField] private AudioSettings _settings;

        private class AudioSourceBaker : Baker<AudioSourceAuthoring>
        {
            public override void Bake(AudioSourceAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AudioSourceData audioSourceData = new AudioSourceData
                {
                    AudioClip = authoring._audioClip,
                    Settings = authoring._settings
                };

                AddComponent(entity, audioSourceData);

                AddComponent<ApplyDataToSourceFlag>(entity);
                SetComponentEnabled<ApplyDataToSourceFlag>(entity, false);

                AddComponent<PlayFlag>(entity);
                SetComponentEnabled<PlayFlag>(entity, authoring._playOnAwake);

                AddComponent<StopFlag>(entity);
                SetComponentEnabled<StopFlag>(entity, false);
                AddComponent<PauseFlag>(entity);
                SetComponentEnabled<PauseFlag>(entity, false);
                AddComponent<ResumeFlag>(entity);
                SetComponentEnabled<ResumeFlag>(entity, false);

                AddComponent<FollowPositionFlag>(entity);
                SetComponentEnabled<FollowPositionFlag>(entity, authoring._followPosition);
            }
        }
    }

    public struct AudioSourceData : IComponentData
    {
        public UnityObjectRef<AudioClip> AudioClip;
        public AudioSettingsData Settings;
    }

    public struct AudioSourceCleanupData : ICleanupComponentData
    {
        public int ID;
    }

    public struct ApplyDataToSourceFlag : IComponentData, IEnableableComponent { }

    public struct PlayFlag : IComponentData, IEnableableComponent { }

    public struct PauseFlag : IComponentData, IEnableableComponent { }

    public struct ResumeFlag : IComponentData, IEnableableComponent { }

    public struct FollowPositionFlag : IComponentData, IEnableableComponent { }

    public struct StopFlag : IComponentData, IEnableableComponent { }
}