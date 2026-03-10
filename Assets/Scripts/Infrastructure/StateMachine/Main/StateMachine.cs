using System;
using Infrastructure.StateMachine.Main.Core;
using Infrastructure.StateMachine.Main.States.Core;
using Infrastructure.StateMachine.Main.States.Factory.Core;
using Infrastructure.StateMachine.Main.States.Info;
using Infrastructure.StateMachine.Main.States.Info.Core;
using VContainer.Unity;

namespace Infrastructure.StateMachine.Main
{
    public class StateMachine<TBaseState> : IStateMachine<TBaseState>, IFixedTickable, ITickable, ILateTickable, IDisposable
    {
        private readonly IStateFactory _stateFactory;

        protected StateMachine(IStateFactory stateFactory)
        {
            _stateFactory = stateFactory;
        }

        private IStateInfo _currentStateInfo;
        private IStateInfo _lastStateInfo;

        public Type ActiveStateType => _currentStateInfo?.StateType;

        public TState Enter<TState>() where TState : class, TBaseState, IState
        {
            ChangeState(out TState state);

            _lastStateInfo = _currentStateInfo;
            _currentStateInfo = new StateInfo<TState, TBaseState>(this, state);

            state.Enter();
            return state;
        }

        public TState Enter<TState, TPayload>(TPayload payload) where TState : class, TBaseState, IPayloadedState<TPayload>
        {
            ChangeState(out TState state);

            _lastStateInfo = _currentStateInfo;
            _currentStateInfo = new PayloadedStateInfo<TState, TBaseState, TPayload>(this, state, payload);

            state.Enter(payload);
            return state;
        }

        public bool Back()
        {
            if (_lastStateInfo == null)
                return false;

            _lastStateInfo.Enter();
            return true;
        }

        public void FixedTick() => _currentStateInfo?.FixedTick();

        public void Tick() => _currentStateInfo?.Tick();

        public void LateTick() => _currentStateInfo?.LateTick();

        public void Dispose()
        {
            Exit();
            _lastStateInfo = null;
        }

        private void ChangeState<TState>(out TState state) where TState : class, IBaseState
        {
            _currentStateInfo?.Exit();

            state = _stateFactory.GetState<TState>();
        }

        public void Exit()
        {
            _currentStateInfo?.Exit();
            _currentStateInfo = null;
        }
    }
}