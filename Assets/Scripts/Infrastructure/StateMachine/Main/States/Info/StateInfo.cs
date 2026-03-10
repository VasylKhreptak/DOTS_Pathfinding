using System;
using Infrastructure.StateMachine.Main.States.Core;
using Infrastructure.StateMachine.Main.States.Info.Core;
using VContainer.Unity;

namespace Infrastructure.StateMachine.Main.States.Info
{
    public class StateInfo<TState, TBaseState> : IStateInfo where TState : class, IState, TBaseState
    {
        private readonly StateMachine<TBaseState> _stateMachine;
        private readonly IFixedTickable _fixedTickable;
        private readonly ITickable _tickable;
        private readonly ILateTickable _lateTickable;
        private readonly IExitable _exitable;

        public StateInfo(StateMachine<TBaseState> stateMachine, TState state)
        {
            _stateMachine = stateMachine;
            _fixedTickable = state as IFixedTickable;
            _tickable = state as ITickable;
            _lateTickable = state as ILateTickable;
            _exitable = state as IExitable;
            StateType = typeof(TState);
        }

        public Type StateType { get; }

        public virtual void Enter() => _stateMachine.Enter<TState>();

        public void FixedTick() => _fixedTickable?.FixedTick();

        public void Tick() => _tickable?.Tick();

        public void LateTick() => _lateTickable?.LateTick();

        public void Exit() => _exitable?.Exit();
    }
}