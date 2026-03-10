using System;
using UniRx;

namespace Plugins.Timer.Reactive
{
    public interface IReadOnlyReactiveTimeSpan
    {
        public IReadOnlyReactiveProperty<int> Days { get; }
        public IReadOnlyReactiveProperty<int> Hours { get; }
        public IReadOnlyReactiveProperty<int> Minutes { get; }
        public IReadOnlyReactiveProperty<int> Seconds { get; }
        public IReadOnlyReactiveProperty<int> Milliseconds { get; }
        public IReadOnlyReactiveProperty<long> Ticks { get; }

        public IReadOnlyReactiveProperty<double> TotalDays { get; }
        public IReadOnlyReactiveProperty<double> TotalHours { get; }
        public IReadOnlyReactiveProperty<double> TotalMinutes { get; }
        public IReadOnlyReactiveProperty<double> TotalSeconds { get; }
        public IReadOnlyReactiveProperty<double> TotalMilliseconds { get; }

        public IObservable<Unit> OnAnyUpdate { get; }

        public TimeSpan TimeSpan { get; }
    }
}