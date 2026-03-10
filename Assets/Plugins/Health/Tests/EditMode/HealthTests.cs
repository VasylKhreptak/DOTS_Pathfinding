using System;
using NUnit.Framework;
using Plugins.Health.Core;
using UniRx;

namespace Plugins.Health.Tests.EditMode
{
    public class HealthTests
    {
        [Test]
        public void InitialValues()
        {
            IHealth health = new Health();

            Assert.AreEqual(0, health.Value.Value);
            Assert.AreEqual(0, health.MaxValue.Value);
            Assert.AreEqual(0, health.FillAmount.Value);
            Assert.IsTrue(health.IsFull.Value);
            Assert.IsTrue(health.IsDeath.Value);
        }

        [Test]
        public void InitialNegativeValues()
        {
            IHealth health = new Health(-50, -100);

            Assert.AreEqual(0, health.Value.Value);
            Assert.AreEqual(0, health.MaxValue.Value);
            Assert.AreEqual(0, health.FillAmount.Value);
            Assert.IsTrue(health.IsFull.Value);
            Assert.IsTrue(health.IsDeath.Value);
        }

        [Test]
        public void InitialPositiveValues()
        {
            IHealth health = new Health(50, 100);

            Assert.AreEqual(50, health.Value.Value);
            Assert.AreEqual(100, health.MaxValue.Value);
            Assert.AreEqual(0.5f, health.FillAmount.Value);
            Assert.IsFalse(health.IsFull.Value);
            Assert.IsFalse(health.IsDeath.Value);
        }

        [Test]
        public void SetValue()
        {
            IHealth health = new Health(50, 100);

            health.SetValue(25);

            Assert.AreEqual(25, health.Value.Value);
            Assert.AreEqual(100, health.MaxValue.Value);
            Assert.AreEqual(0.25f, health.FillAmount.Value);
            Assert.IsFalse(health.IsFull.Value);
            Assert.IsFalse(health.IsDeath.Value);
        }

        [Test]
        public void SetValueMaxValue()
        {
            IHealth health = new Health(50, 100);

            health.SetValue(100);

            Assert.AreEqual(100, health.Value.Value);
            Assert.AreEqual(100, health.MaxValue.Value);
            Assert.AreEqual(1, health.FillAmount.Value);
            Assert.IsTrue(health.IsFull.Value);
            Assert.IsFalse(health.IsDeath.Value);
        }

        [Test]
        public void SetValueMinValue()
        {
            IHealth health = new Health(50, 100);

            health.SetValue(0);

            Assert.AreEqual(0, health.Value.Value);
            Assert.AreEqual(100, health.MaxValue.Value);
            Assert.AreEqual(0, health.FillAmount.Value);
            Assert.IsFalse(health.IsFull.Value);
            Assert.IsTrue(health.IsDeath.Value);
        }

        [Test]
        public void SetNegativeValue()
        {
            IHealth health = new Health(50, 100);

            health.SetValue(-25);

            Assert.AreEqual(0, health.Value.Value);
            Assert.AreEqual(100, health.MaxValue.Value);
            Assert.AreEqual(0, health.FillAmount.Value);
            Assert.IsFalse(health.IsFull.Value);
            Assert.IsTrue(health.IsDeath.Value);
        }

        [Test]
        public void SetValueMoreThanMaxValue()
        {
            IHealth health = new Health(50, 100);

            health.SetValue(150);

            Assert.AreEqual(100, health.Value.Value);
            Assert.AreEqual(100, health.MaxValue.Value);
            Assert.AreEqual(1, health.FillAmount.Value);
            Assert.IsTrue(health.IsFull.Value);
            Assert.IsFalse(health.IsDeath.Value);
        }

        [Test]
        public void SetMaxValue()
        {
            IHealth health = new Health(50, 100);

            health.SetMaxValue(200);

            Assert.AreEqual(50, health.Value.Value);
            Assert.AreEqual(200, health.MaxValue.Value);
            Assert.AreEqual(0.25f, health.FillAmount.Value);
            Assert.IsFalse(health.IsFull.Value);
            Assert.IsFalse(health.IsDeath.Value);
        }

        [Test]
        public void SetMaxValueLessThanValue()
        {
            IHealth health = new Health(50, 100);

            health.SetMaxValue(25);

            Assert.AreEqual(25, health.Value.Value);
            Assert.AreEqual(25, health.MaxValue.Value);
            Assert.AreEqual(1, health.FillAmount.Value);
            Assert.IsTrue(health.IsFull.Value);
            Assert.IsFalse(health.IsDeath.Value);
        }

        [Test]
        public void SetMaxValueNegative()
        {
            IHealth health = new Health(50, 100);

            health.SetMaxValue(-25);

            Assert.AreEqual(0, health.Value.Value);
            Assert.AreEqual(0, health.MaxValue.Value);
            Assert.AreEqual(0, health.FillAmount.Value);
            Assert.IsTrue(health.IsFull.Value);
            Assert.IsTrue(health.IsDeath.Value);
        }

        [Test]
        public void Heal()
        {
            IHealth health = new Health(50, 100);

            health.Heal(25);

            Assert.AreEqual(75, health.Value.Value);
            Assert.AreEqual(100, health.MaxValue.Value);
            Assert.AreEqual(0.75f, health.FillAmount.Value);
            Assert.IsFalse(health.IsFull.Value);
            Assert.IsFalse(health.IsDeath.Value);
        }

        [Test]
        public void HealNegative()
        {
            IHealth health = new Health(50, 100);

            health.Heal(-25);

            Assert.AreEqual(50, health.Value.Value);
            Assert.AreEqual(100, health.MaxValue.Value);
            Assert.AreEqual(0.5f, health.FillAmount.Value);
            Assert.IsFalse(health.IsFull.Value);
            Assert.IsFalse(health.IsDeath.Value);
        }

        [Test]
        public void HealMoreThanMaxValue()
        {
            IHealth health = new Health(50, 100);

            health.Heal(150);

            Assert.AreEqual(100, health.Value.Value);
            Assert.AreEqual(100, health.MaxValue.Value);
            Assert.AreEqual(1, health.FillAmount.Value);
            Assert.IsTrue(health.IsFull.Value);
            Assert.IsFalse(health.IsDeath.Value);
        }

        [Test]
        public void TakeDamage()
        {
            IHealth health = new Health(50, 100);

            health.TakeDamage(25);

            Assert.AreEqual(25, health.Value.Value);
            Assert.AreEqual(100, health.MaxValue.Value);
            Assert.AreEqual(0.25f, health.FillAmount.Value);
            Assert.IsFalse(health.IsFull.Value);
            Assert.IsFalse(health.IsDeath.Value);
        }

        [Test]
        public void TakeDamageNegative()
        {
            IHealth health = new Health(50, 100);

            health.TakeDamage(-25);

            Assert.AreEqual(50, health.Value.Value);
            Assert.AreEqual(100, health.MaxValue.Value);
            Assert.AreEqual(0.5f, health.FillAmount.Value);
            Assert.IsFalse(health.IsFull.Value);
            Assert.IsFalse(health.IsDeath.Value);
        }

        [Test]
        public void TakeDamageMoreThanValue()
        {
            IHealth health = new Health(50, 100);

            health.TakeDamage(75);

            Assert.AreEqual(0, health.Value.Value);
            Assert.AreEqual(100, health.MaxValue.Value);
            Assert.AreEqual(0, health.FillAmount.Value);
            Assert.IsFalse(health.IsFull.Value);
            Assert.IsTrue(health.IsDeath.Value);
        }

        [Test]
        public void TakeDamageMoreThanMaxValue()
        {
            IHealth health = new Health(50, 100);

            health.TakeDamage(150);

            Assert.AreEqual(0, health.Value.Value);
            Assert.AreEqual(100, health.MaxValue.Value);
            Assert.AreEqual(0, health.FillAmount.Value);
            Assert.IsFalse(health.IsFull.Value);
            Assert.IsTrue(health.IsDeath.Value);
        }

        [Test]
        public void Kill()
        {
            IHealth health = new Health(50, 100);

            health.Kill();

            Assert.AreEqual(0, health.Value.Value);
            Assert.AreEqual(100, health.MaxValue.Value);
            Assert.AreEqual(0, health.FillAmount.Value);
            Assert.IsFalse(health.IsFull.Value);
            Assert.IsTrue(health.IsDeath.Value);
        }

        [Test]
        public void Restore()
        {
            IHealth health = new Health(50, 100);

            health.Restore();

            Assert.AreEqual(100, health.Value.Value);
            Assert.AreEqual(100, health.MaxValue.Value);
            Assert.AreEqual(1, health.FillAmount.Value);
            Assert.IsTrue(health.IsFull.Value);
            Assert.IsFalse(health.IsDeath.Value);
        }

        [Test]
        public void DamageEvent()
        {
            float damageReceived = 0;
            float damageSent = 0;
            int eventFiredCount = 0;

            IHealth health = new Health(50, 100);

            IDisposable damageSubscription = health.OnDamaged.Subscribe(damage =>
            {
                damageReceived += damage;
                eventFiredCount++;
            });

            health.TakeDamage(25);
            damageSent += 25;
            health.TakeDamage(25);
            damageSent += 25;
            health.TakeDamage(25);
            health.TakeDamage(25);

            damageSubscription.Dispose();

            Assert.AreEqual(2, eventFiredCount);
            Assert.AreEqual(damageSent, damageReceived);
            Assert.AreEqual(0, health.Value.Value);
            Assert.AreEqual(100, health.MaxValue.Value);
            Assert.AreEqual(0, health.FillAmount.Value);
            Assert.IsFalse(health.IsFull.Value);
            Assert.IsTrue(health.IsDeath.Value);
        }

        [Test]
        public void HealEvent()
        {
            float healReceived = 0;
            float healSent = 0;
            int eventFiredCount = 0;

            IHealth health = new Health(50, 100);

            IDisposable healSubscription = health.OnHealed.Subscribe(heal =>
            {
                healReceived += heal;
                eventFiredCount++;
            });

            health.Heal(25);
            healSent += 25;
            health.Heal(25);
            healSent += 25;
            health.Heal(25);
            health.Heal(25);

            healSubscription.Dispose();

            Assert.AreEqual(2, eventFiredCount);
            Assert.AreEqual(healSent, healReceived);
            Assert.AreEqual(100, health.Value.Value);
            Assert.AreEqual(100, health.MaxValue.Value);
            Assert.AreEqual(1, health.FillAmount.Value);
            Assert.IsTrue(health.IsFull.Value);
            Assert.IsFalse(health.IsDeath.Value);
        }
    }
}