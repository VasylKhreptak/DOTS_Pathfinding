using Infrastructure.StateMachine.Game.States.Core;
using Infrastructure.StateMachine.Main;

namespace Infrastructure.StateMachine.Game
{
    public class GameStateMachine : StateMachine<IGameState>
    {
        public GameStateMachine(GameStateFactory stateFactory) : base(stateFactory) { }
    }
}