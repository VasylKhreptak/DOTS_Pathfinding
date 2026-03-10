namespace Infrastructure.StateMachine.Main.States.Core
{
    public interface IState : IBaseState
    {
        public void Enter();
    }
}