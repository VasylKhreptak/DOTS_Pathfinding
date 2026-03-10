using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.Configs;
using Infrastructure.Services.Log.Core;
using Infrastructure.Services.Vibration.Core;
using Infrastructure.Tools;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace Infrastructure.Services.Vibration
{
    public class NewInputVibrationService : IVibrationService, IInitializable, IDisposable
    {
        private readonly VibrationServiceConfig _config;
        private readonly ILogService _logService;

        public NewInputVibrationService(VibrationServiceConfig config, ILogService logService)
        {
            _config = config;
            _logService = logService;
        }

        private readonly AutoResetCancellationTokenSource _cts = new AutoResetCancellationTokenSource();
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        private float _intensityMultiplier = 1f;

        public void Initialize()
        {
            Observable.EveryApplicationFocus().Subscribe(OnApplicationFocus).AddTo(_subscriptions);
            Observable.EveryApplicationPause().Subscribe(OnApplicationPause).AddTo(_subscriptions);
        }

        public void Dispose()
        {
            Stop();
            _subscriptions.Dispose();
        }

        public void Vibrate(VibrationPreset preset)
        {
            _logService.Log($"Vibration. Preset: {preset.ToString()}");

            VibrationServiceConfig.PresetConfig presetSettings = GetPresetSettings(preset);
            VibrateInternal(presetSettings.Intensity, presetSettings.Duration);
        }

        public void Vibrate(float intensity, float duration)
        {
            _logService.Log($"Vibration. Constant. Intensity: {intensity}. Duration: {duration}");
            VibrateInternal(intensity, duration);
        }

        public void Stop()
        {
            _cts.Cancel();
            TrySetMotorsSpeed(0f);
        }

        public void SetIntensity(float intensity) => _intensityMultiplier = Mathf.Clamp01(intensity);

        private void VibrateInternal(float intensity, float duration)
        {
            _cts.Cancel();
            VibrateTask(intensity, duration, _cts.Token).Forget();
        }

        private async UniTask VibrateTask(float intensity, float duration, CancellationToken token)
        {
            if (!TrySetMotorsSpeed(intensity * _intensityMultiplier))
                return;

            await UniTask.WaitForSeconds(duration, true, cancellationToken: token);

            TrySetMotorsSpeed(0f);
        }

        private bool TrySetMotorsSpeed(float intensity)
        {
            if (Gamepad.current == null)
                return false;

            Gamepad.current.SetMotorSpeeds(intensity, intensity);
            return true;
        }

        private VibrationServiceConfig.PresetConfig GetPresetSettings(VibrationPreset preset) => _config.Presets[preset];

        private void OnApplicationPause(bool pauseStatus)
        {
            if (Gamepad.current == null)
                return;

            if (pauseStatus)
                Gamepad.current.PauseHaptics();
            else
                Gamepad.current.ResumeHaptics();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (Gamepad.current == null)
                return;

            if (hasFocus)
                Gamepad.current.ResumeHaptics();
            else
                Gamepad.current.PauseHaptics();
        }
    }
}