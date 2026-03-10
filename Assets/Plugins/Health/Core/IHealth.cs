namespace Plugins.Health.Core
{
    public interface IHealth : IReadOnlyHealth
    {
        public void SetValue(float health);

        public void SetMaxValue(float maxValue);

        public void Heal(float amount);

        public void TakeDamage(float damage);

        public void Kill();

        public void Restore();
    }
}