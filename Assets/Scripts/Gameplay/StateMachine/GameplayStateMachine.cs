using Gameplay.StateMachine.States;
using Gameplay.StateMachine.States.Core;
using Infrastructure.StateMachine.Main;

namespace Gameplay.StateMachine
{
    public class GameplayStateMachine : StateMachine<IGameplayState>
    {
        public GameplayStateMachine(GameplayStateFactory stateFactory) : base(stateFactory) { }
    }
}