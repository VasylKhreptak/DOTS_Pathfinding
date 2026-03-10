using Infrastructure.StateMachine.Main.States.Core;

namespace Infrastructure.StateMachine.Main.States.Factory.Core
{
    public interface IStateFactory
    {
        public T GetState<T>() where T : class, IBaseState;
    }
}