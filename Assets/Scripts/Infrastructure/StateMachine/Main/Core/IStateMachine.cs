using System;
using Infrastructure.StateMachine.Main.States.Core;

namespace Infrastructure.StateMachine.Main.Core
{
    public interface IStateMachine<in TBaseState> : IExitable
    {
        public Type ActiveStateType { get; }

        public TState Enter<TState>() where TState : class, TBaseState, IState;

        public TState Enter<TState, TPayload>(TPayload payload) where TState : class, TBaseState, IPayloadedState<TPayload>;

        public bool Back();
    }
}