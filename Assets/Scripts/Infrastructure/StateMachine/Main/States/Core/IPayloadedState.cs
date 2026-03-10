namespace Infrastructure.StateMachine.Main.States.Core
{
    public interface IPayloadedState<in TPayload> : IBaseState
    {
        public void Enter(TPayload payload);
    }
}