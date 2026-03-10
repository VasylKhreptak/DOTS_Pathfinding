using System;
using Plugins.AudioService.Properties.Core;

namespace Plugins.AudioService.Properties
{
    public class ReadonlyProperty<TOut> : IReadonlyProperty<TOut>
    {
        private readonly Func<int, bool> _canAccess;
        private readonly Func<int, TOut> _get;

        public ReadonlyProperty(Func<int, bool> canAccess, Func<int, TOut> get)
        {
            _get = get;
            _canAccess = canAccess;
        }

        public bool IsAccessible(int id) => _canAccess.Invoke(id);

        public TOut GetValue(int id)
        {
            if (_canAccess.Invoke(id))
                return _get.Invoke(id);

            return default;
        }
    }
}