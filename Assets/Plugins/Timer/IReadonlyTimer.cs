using System;
using Plugins.Timer.Reactive;
using UniRx;

namespace Plugins.Timer
{
    public interface IReadonlyTimer
    {
        public IReadOnlyReactiveProperty<float> Progress { get; }
        public IReadOnlyReactiveProperty<float> RemainingProgress { get; }
        public IReadOnlyReactiveTimeSpan Time { get; }
        public IReadOnlyReactiveTimeSpan RemainingTime { get; }
        public IReadOnlyReactiveTimeSpan TargetTime { get; }
        public IReadOnlyReactiveProperty<float> TimeScale { get; }
        public IReadOnlyReactiveProperty<bool> IsPaused { get; }

        public UpdateMethod UpdateMethod { get; }

        public IObservable<Unit> OnStarted { get; }
        public IObservable<Unit> OnCompleted { get; }
        public IObservable<Unit> OnStopped { get; }
    }
}