using System;
using VContainer.Unity;

namespace Infrastructure.StateMachine.Main.States.Info.Core
{
    public interface IStateInfo : IFixedTickable, ITickable, ILateTickable
    {
        public Type StateType { get; }

        public void Enter();

        public void Exit();
    }
}