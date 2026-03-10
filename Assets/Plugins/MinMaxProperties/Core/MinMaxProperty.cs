using System;

namespace Plugins.MinMaxProperties.Core
{
    public abstract class MinMaxProperty<T>
    {
        public T Min;
        public T Max;

        public MinMaxProperty() { }

        public MinMaxProperty(T min, T max)
        {
            Min = min;
            Max = max;
        }

        public abstract T Random();
    }
}