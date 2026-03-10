using System;
using Plugins.Banks.Core;
using Plugins.Banks.Float;
using Plugins.Health.Core;
using UniRx;
using UnityEngine;

namespace Plugins.Health
{
    public class Health : IHealth
    {
        private readonly IClampedBank<float> _health;
        private readonly Subject<float> _onDamaged;
        private readonly Subject<float> _onHealed;

        public Health() : this(0) { }

        public Health(float maxHealth) : this(maxHealth, maxHealth) { }

        public Health(float health, float maxHealth)
        {
            _health = new ClampedFloatBank(health, maxHealth);
            _onDamaged = new Subject<float>();
            _onHealed = new Subject<float>();
        }

        public IReadOnlyReactiveProperty<float> Value => _health.Amount;

        public IReadOnlyReactiveProperty<float> MaxValue => _health.MaxAmount;

        public IReadOnlyReactiveProperty<float> FillAmount => _health.FillAmount;

        public IReadOnlyReactiveProperty<bool> IsFull => _health.IsFull;

        public IReadOnlyReactiveProperty<bool> IsDeath => _health.IsEmpty;

        public IObservable<float> OnDamaged => _onDamaged;

        public IObservable<float> OnHealed => _onHealed;

        public void SetValue(float health) => _health.SetValue(health);

        public void SetMaxValue(float maxValue) => _health.SetMaxValue(maxValue);

        public void Heal(float amount)
        {
            amount = Mathf.Clamp(amount, 0, _health.MaxAmount.Value - _health.Amount.Value);

            _health.Add(amount);

            if (amount == 0)
                return;

            _onHealed.OnNext(amount);
        }

        public void TakeDamage(float damage)
        {
            damage = Mathf.Clamp(damage, 0, _health.Amount.Value);

            _health.Spend(damage);

            if (damage == 0)
                return;

            _onDamaged.OnNext(damage);
        }

        public void Kill() => _health.Clear();

        public void Restore() => _health.Fill();
    }
}