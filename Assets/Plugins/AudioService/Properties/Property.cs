using System;
using Plugins.AudioService.Properties.Core;

namespace Plugins.AudioService.Properties
{
    public class Property<TValue> : ReadonlyProperty<TValue>, IProperty<TValue>
    {
        private readonly Func<int, bool> _canAccess;
        private readonly Action<int, TValue> _set;

        public Property(Func<int, bool> canAccess, Func<int, TValue> get, Action<int, TValue> set) : base(canAccess, get)
        {
            _set = set;
            _canAccess = canAccess;
        }

        public void SetValue(int input, TValue value)
        {
            if (_canAccess.Invoke(input))
                _set.Invoke(input, value);
        }
    }
}