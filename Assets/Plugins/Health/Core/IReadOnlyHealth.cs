using System;
using UniRx;

namespace Plugins.Health.Core
{
    public interface IReadOnlyHealth
    {
        public IReadOnlyReactiveProperty<float> Value { get; }

        public IReadOnlyReactiveProperty<float> MaxValue { get; }

        public IReadOnlyReactiveProperty<float> FillAmount { get; }

        public IReadOnlyReactiveProperty<bool> IsFull { get; }

        public IReadOnlyReactiveProperty<bool> IsDeath { get; }

        public IObservable<float> OnDamaged { get; }

        public IObservable<float> OnHealed { get; }
    }
}