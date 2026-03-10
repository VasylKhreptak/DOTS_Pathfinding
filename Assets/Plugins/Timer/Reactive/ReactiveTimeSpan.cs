using System;
using UniRx;

namespace Plugins.Timer.Reactive
{
    public class ReactiveTimeSpan : IReactiveTimeSpan
    {
        private readonly IntReactiveProperty _days = new IntReactiveProperty(0);
        private readonly IntReactiveProperty _hours = new IntReactiveProperty(0);
        private readonly IntReactiveProperty _minutes = new IntReactiveProperty(0);
        private readonly IntReactiveProperty _seconds = new IntReactiveProperty(0);
        private readonly IntReactiveProperty _milliseconds = new IntReactiveProperty(0);
        private readonly LongReactiveProperty _ticks = new LongReactiveProperty(0);

        private readonly DoubleReactiveProperty _totalDays = new DoubleReactiveProperty(0);
        private readonly DoubleReactiveProperty _totalHours = new DoubleReactiveProperty(0);
        private readonly DoubleReactiveProperty _totalMinutes = new DoubleReactiveProperty(0);
        private readonly DoubleReactiveProperty _totalSeconds = new DoubleReactiveProperty(0);
        private readonly DoubleReactiveProperty _totalMilliseconds = new DoubleReactiveProperty(0);

        private readonly Subject<Unit> _onAnyUpdate = new Subject<Unit>();

        private TimeSpan _timeSpan;

        public IReadOnlyReactiveProperty<int> Days => _days;
        public IReadOnlyReactiveProperty<int> Hours => _hours;
        public IReadOnlyReactiveProperty<int> Minutes => _minutes;
        public IReadOnlyReactiveProperty<int> Seconds => _seconds;
        public IReadOnlyReactiveProperty<int> Milliseconds => _milliseconds;
        public IReadOnlyReactiveProperty<long> Ticks => _ticks;

        public IReadOnlyReactiveProperty<double> TotalDays => _totalDays;
        public IReadOnlyReactiveProperty<double> TotalHours => _totalHours;
        public IReadOnlyReactiveProperty<double> TotalMinutes => _totalMinutes;
        public IReadOnlyReactiveProperty<double> TotalSeconds => _totalSeconds;
        public IReadOnlyReactiveProperty<double> TotalMilliseconds => _totalMilliseconds;

        public IObservable<Unit> OnAnyUpdate => _onAnyUpdate;

        public TimeSpan TimeSpan => _timeSpan;

        public ReactiveTimeSpan()
        {
            _timeSpan = TimeSpan.Zero;
            UpdateProperties();
        }

        public ReactiveTimeSpan(TimeSpan timeSpan)
        {
            _timeSpan = timeSpan;
            UpdateProperties();
        }

        public void Set(TimeSpan timeSpan)
        {
            _timeSpan = timeSpan;
            UpdateProperties();
        }

        public void Add(TimeSpan timeSpan)
        {
            _timeSpan = _timeSpan.Add(timeSpan);
            UpdateProperties();
        }

        public void Subtract(TimeSpan timeSpan)
        {
            _timeSpan = _timeSpan.Subtract(timeSpan);
            UpdateProperties();
        }

        public void Multiply(double factor)
        {
            _timeSpan = _timeSpan.Multiply(factor);
            UpdateProperties();
        }

        public void Divide(double divisor)
        {
            _timeSpan = _timeSpan.Divide(divisor);
            UpdateProperties();
        }

        public void Negate()
        {
            _timeSpan = _timeSpan.Negate();
            UpdateProperties();
        }

        public static implicit operator TimeSpan(ReactiveTimeSpan reactiveTimeSpan) => reactiveTimeSpan._timeSpan;

        public static implicit operator ReactiveTimeSpan(TimeSpan timeSpan) => new ReactiveTimeSpan(timeSpan);

        private void UpdateProperties()
        {
            _days.Value = _timeSpan.Days;
            _hours.Value = _timeSpan.Hours;
            _minutes.Value = _timeSpan.Minutes;
            _seconds.Value = _timeSpan.Seconds;
            _milliseconds.Value = _timeSpan.Milliseconds;
            _ticks.Value = _timeSpan.Ticks;

            _totalDays.Value = _timeSpan.TotalDays;
            _totalHours.Value = _timeSpan.TotalHours;
            _totalMinutes.Value = _timeSpan.TotalMinutes;
            _totalSeconds.Value = _timeSpan.TotalSeconds;
            _totalMilliseconds.Value = _timeSpan.TotalMilliseconds;

            _onAnyUpdate.OnNext(Unit.Default);
        }
    }
}