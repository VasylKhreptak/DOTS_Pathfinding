using System;

namespace Plugins.Timer.Reactive
{
    public interface IReactiveTimeSpan : IReadOnlyReactiveTimeSpan
    {
        public void Add(TimeSpan timeSpan);
        public void Subtract(TimeSpan timeSpan);
        public void Multiply(double factor);
        public void Divide(double divisor);

        public void Negate();
    }
}