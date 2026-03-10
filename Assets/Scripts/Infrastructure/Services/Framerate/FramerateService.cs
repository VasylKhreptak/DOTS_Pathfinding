using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Services.Framerate.Core;
using UniRx;
using UnityEngine;
using VContainer.Unity;

namespace Infrastructure.Services.Framerate
{
    public class FramerateService : IInitializable, IDisposable, IFramerateService
    {
        private const float UpdateInterval = 0.5f;
        private const float FrameCaptureInterval = 0.1f;
        private const int FrameBuffer = 10;

        private readonly FloatReactiveProperty _averageFramerate = new FloatReactiveProperty(0);

        private readonly Queue<float> _capturedFrames = new Queue<float>(FrameBuffer);

        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        public IReadOnlyReactiveProperty<float> AverageFramerate => _averageFramerate;

        public void Initialize()
        {
            // DisableVsync();
            // SetTargetFramerate((int)Screen.currentResolution.refreshRateRatio.value);
            StartCapturingFrames();
            StartUpdatingAverageFramerate();
        }

        public void Dispose() => _subscriptions.Dispose();

        public void SetTargetFramerate(int framerate) => Application.targetFrameRate = framerate;

        private void DisableVsync() => QualitySettings.vSyncCount = 0;

        private void StartCapturingFrames()
        {
            Observable
                .Interval(TimeSpan.FromSeconds(FrameCaptureInterval))
                .DoOnSubscribe(CaptureFrame)
                .Subscribe(_ => CaptureFrame())
                .AddTo(_subscriptions);
        }

        private void StartUpdatingAverageFramerate()
        {
            Observable
                .Interval(TimeSpan.FromSeconds(UpdateInterval))
                .DoOnSubscribe(UpdateAverageFramerate)
                .Subscribe(_ => UpdateAverageFramerate())
                .AddTo(_subscriptions);
        }

        private void UpdateAverageFramerate() => _averageFramerate.Value = _capturedFrames.Average();

        private void CaptureFrame()
        {
            if (_capturedFrames.Count == FrameBuffer)
                _capturedFrames.Dequeue();

            _capturedFrames.Enqueue(1f / Time.unscaledDeltaTime);
        }
    }
}