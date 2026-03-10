using Gameplay.Components.Audio;
using Plugins.AudioService.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using VContainer;

namespace Gameplay.Systems.Audio
{
    [DisableAutoCreation]
    public partial class AudioSourceSystem : SystemBase
    {
        private IAudioService _audioService;

        [Inject]
        public void Construct(IAudioService audioService)
        {
            _audioService = audioService;
        }

        protected override void OnCreate()
        {
            RequireForUpdate<AudioSourceData>();
        }

        protected override void OnUpdate()
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            CreateCleanupData(ecb);
            HandleApplyDataToSource();
            HandlePlay();
            HandlePause();
            HandleResume();
            HandleStop();
            HandlePositionFollow();
            CleanupAudioSources(ecb);

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        protected override void OnDestroy()
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach ((RefRO<AudioSourceCleanupData> audioSourceCleanupData, Entity entity) in
                     SystemAPI.Query<RefRO<AudioSourceCleanupData>>().WithEntityAccess())
            {
                _audioService.Stop(audioSourceCleanupData.ValueRO.ID);
                ecb.RemoveComponent<AudioSourceCleanupData>(entity);
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        private void CreateCleanupData(EntityCommandBuffer ecb)
        {
            foreach ((RefRO<AudioSourceData> _, Entity entity) in
                     SystemAPI.Query<RefRO<AudioSourceData>>().WithNone<AudioSourceCleanupData>().WithEntityAccess())
            {
                AudioSourceCleanupData cleanupData = new AudioSourceCleanupData
                {
                    ID = -1
                };

                ecb.AddComponent(entity, cleanupData);
            }
        }

        private void HandleApplyDataToSource()
        {
            foreach ((RefRO<AudioSourceData> audioSourceData, RefRW<AudioSourceCleanupData> audioSourceCleanupData, Entity entity) in
                     SystemAPI.Query<RefRO<AudioSourceData>, RefRW<AudioSourceCleanupData>>().WithAll<ApplyDataToSourceFlag>().WithEntityAccess())
            {
                if (!_audioService.IsActive(audioSourceCleanupData.ValueRO.ID))
                    continue;

                _audioService.ApplySettings(audioSourceCleanupData.ValueRO.ID, audioSourceData.ValueRO.Settings);

                EntityManager.SetComponentEnabled<ApplyDataToSourceFlag>(entity, false);
            }
        }

        private void HandlePlay()
        {
            foreach ((RefRW<AudioSourceData> audioSourceData, RefRW<AudioSourceCleanupData> audioSourceCleanupData, RefRO<LocalToWorld> localToWorld, Entity entity) in
                     SystemAPI.Query<RefRW<AudioSourceData>, RefRW<AudioSourceCleanupData>, RefRO<LocalToWorld>>().WithAll<PlayFlag>().WithEntityAccess())
            {
                _audioService.Stop(audioSourceCleanupData.ValueRO.ID);

                if (audioSourceData.ValueRO.AudioClip == null)
                    continue;

                int audioId = _audioService.Play(audioSourceData.ValueRO.AudioClip, localToWorld.ValueRO.Position, audioSourceData.ValueRO.Settings);

                audioSourceCleanupData.ValueRW.ID = audioId;

                EntityManager.SetComponentEnabled<PlayFlag>(entity, false);
            }
        }

        private void HandlePause()
        {
            foreach ((RefRO<AudioSourceCleanupData> audioSourceCleanupData, Entity entity) in
                     SystemAPI.Query<RefRO<AudioSourceCleanupData>>().WithAll<AudioSourceData, PauseFlag>().WithEntityAccess())
            {
                if (!_audioService.IsActive(audioSourceCleanupData.ValueRO.ID))
                    continue;

                _audioService.Pause(audioSourceCleanupData.ValueRO.ID);

                EntityManager.SetComponentEnabled<PauseFlag>(entity, false);
            }
        }

        private void HandleResume()
        {
            foreach ((RefRO<AudioSourceCleanupData> audioSourceCleanupData, Entity entity) in
                     SystemAPI.Query<RefRO<AudioSourceCleanupData>>().WithAll<AudioSourceData, ResumeFlag>().WithEntityAccess())
            {
                if (!_audioService.IsActive(audioSourceCleanupData.ValueRO.ID))
                    continue;

                _audioService.Resume(audioSourceCleanupData.ValueRO.ID);

                EntityManager.SetComponentEnabled<ResumeFlag>(entity, false);
            }
        }

        private void HandlePositionFollow()
        {
            foreach ((RefRO<AudioSourceCleanupData> audioSourceCleanupData, RefRO<LocalToWorld> localToWorld) in
                     SystemAPI.Query<RefRO<AudioSourceCleanupData>, RefRO<LocalToWorld>>().WithAll<AudioSourceData, FollowPositionFlag>())
            {
                if (!_audioService.IsActive(audioSourceCleanupData.ValueRO.ID))
                    continue;

                _audioService.Properties.Position.SetValue(audioSourceCleanupData.ValueRO.ID, localToWorld.ValueRO.Position);
            }
        }

        private void HandleStop()
        {
            foreach ((RefRO<AudioSourceCleanupData> cleanup, Entity entity) in
                     SystemAPI.Query<RefRO<AudioSourceCleanupData>>().WithAll<AudioSourceData, StopFlag>().WithEntityAccess())
            {
                _audioService.Stop(cleanup.ValueRO.ID);

                EntityManager.SetComponentEnabled<StopFlag>(entity, false);
            }
        }

        private void CleanupAudioSources(EntityCommandBuffer ecb)
        {
            foreach ((RefRO<AudioSourceCleanupData> cleanupData, Entity entity) in
                     SystemAPI.Query<RefRO<AudioSourceCleanupData>>().WithNone<LocalToWorld>().WithEntityAccess())
            {
                _audioService.Stop(cleanupData.ValueRO.ID);

                ecb.RemoveComponent<AudioSourceCleanupData>(entity);
            }
        }
    }
}