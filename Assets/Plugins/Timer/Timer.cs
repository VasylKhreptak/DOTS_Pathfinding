using System;
using System.Collections;
using Plugins.Timer.Reactive;
using UniRx;
using UnityEngine;

namespace Plugins.Timer
{
    public class Timer : ITimer
    {
        private readonly FloatReactiveProperty _progress = new FloatReactiveProperty(0f);
        private readonly FloatReactiveProperty _remainingProgress = new FloatReactiveProperty(0f);
        private readonly ReactiveTimeSpan _time = new ReactiveTimeSpan();
        private readonly ReactiveTimeSpan _remainingTime = new ReactiveTimeSpan();
        private readonly ReactiveTimeSpan _targetTime = new ReactiveTimeSpan();
        private readonly FloatReactiveProperty _timeScale = new FloatReactiveProperty(1f);
        private readonly BoolReactiveProperty _isPaused = new BoolReactiveProperty(false);

        private readonly Subject<Unit> _onStarted = new Subject<Unit>();
        private readonly Subject<Unit> _onCompleted = new Subject<Unit>();
        private readonly Subject<Unit> _onStopped = new Subject<Unit>();

        public UpdateMethod UpdateMethod { get; set; } = UpdateMethod.DeltaTime;

        public IReadOnlyReactiveProperty<float> Progress => _progress;
        public IReadOnlyReactiveProperty<float> RemainingProgress => _remainingProgress;
        public IReadOnlyReactiveTimeSpan Time => _time;
        public IReadOnlyReactiveTimeSpan RemainingTime => _remainingTime;
        public IReadOnlyReactiveTimeSpan TargetTime => _targetTime;
        public IReadOnlyReactiveProperty<float> TimeScale => _timeScale;
        public IReadOnlyReactiveProperty<bool> IsPaused => _isPaused;

        public IObservable<Unit> OnStarted => _onStarted;
        public IObservable<Unit> OnCompleted => _onCompleted;
        public IObservable<Unit> OnStopped => _onStopped;

        private Coroutine _coroutine;

        private TimeSpan _duration;

        public void Start(TimeSpan timeSpan)
        {
            Reset();

            _duration = Max(TimeSpan.Zero, timeSpan);

            _targetTime.Set(_duration);

            _onStarted.OnNext(Unit.Default);

            _coroutine = TimerCoroutineRunner.Run(TimerRoutine());
        }

        private IEnumerator TimerRoutine()
        {
            while (true)
            {
                if (_isPaused.Value)
                {
                    yield return null;
                    continue;
                }

                _time.Add(TimeSpan.FromSeconds(GetDeltaTime() * _timeScale.Value));
                _remainingTime.Set(_duration - _time.TimeSpan);
                _progress.Value = (float)(_time.TimeSpan / _duration);
                _remainingProgress.Value = 1f - _progress.Value;

                if ((Mathf.Sign(_timeScale.Value) >= 0 && _time.TimeSpan >= _targetTime) ||
                    (Mathf.Sign(_timeScale.Value) < 0 && _time.TimeSpan <= TimeSpan.Zero))
                {
                    Complete();
                    yield break;
                }

                yield return null;
            }
        }

        public void Complete()
        {
            if (_coroutine == null)
                return;

            Stop();
            _progress.Value = 1f;
            _remainingProgress.Value = 0f;
            _time.Set(_targetTime.TimeSpan);
            _remainingTime.Set(TimeSpan.Zero);

            _onCompleted.OnNext(Unit.Default);
        }

        public void Stop()
        {
            if (_coroutine == null)
                return;

            TimerCoroutineRunner.Stop(_coroutine);
            _coroutine = null;
            _onStopped.OnNext(Unit.Default);
        }

        public void Pause()
        {
            if (_coroutine == null)
                return;

            _isPaused.Value = true;
        }

        public void Resume()
        {
            if (_coroutine == null)
                return;

            _isPaused.Value = false;
        }

        public void TogglePause()
        {
            if (_isPaused.Value)
                Resume();
            else
                Pause();
        }

        public void Reset()
        {
            Stop();
            _progress.Value = 0f;
            _remainingProgress.Value = 0f;
            _time.Set(TimeSpan.Zero);
            _remainingTime.Set(TimeSpan.Zero);
            _targetTime.Set(TimeSpan.Zero);
            _isPaused.Value = false;
            _timeScale.Value = 1f;
        }

        public void SetTimeScale(float timescale)
        {
            if (_coroutine == null)
                return;

            _timeScale.Value = timescale;
        }

        public void SetTime(TimeSpan time)
        {
            if (_coroutine == null)
                return;

            _time.Set(Clamp(time, TimeSpan.Zero, _targetTime.TimeSpan));
        }

        public void SetTargetTime(TimeSpan targetTime)
        {
            if (_coroutine == null)
                return;

            _targetTime.Set(Max(TimeSpan.Zero, targetTime));
        }

        private TimeSpan Clamp(TimeSpan timeSpan, TimeSpan min, TimeSpan max)
        {
            if (timeSpan < min)
                return min;

            return timeSpan > max ? max : timeSpan;
        }

        private TimeSpan Max(TimeSpan t1, TimeSpan t2) => t1 > t2 ? t1 : t2;

        private float GetDeltaTime()
        {
            switch (UpdateMethod)
            {
                case UpdateMethod.DeltaTime:
                    return UnityEngine.Time.deltaTime;
                case UpdateMethod.UnscaledDeltaTime:
                    return UnityEngine.Time.unscaledDeltaTime;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}