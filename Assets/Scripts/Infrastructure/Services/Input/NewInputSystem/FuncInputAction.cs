using System;
using Infrastructure.Services.Input.Core;

namespace Infrastructure.Services.Input.NewInputSystem
{
    public class FuncInputAction<T> : IInputAction<T>
    {
        private readonly Func<T> _func;

        public FuncInputAction(Func<T> func)
        {
            _func = func;
        }

        public bool Enabled { get; set; }

        public T Value => Enabled ? _func() : default;
    }
}