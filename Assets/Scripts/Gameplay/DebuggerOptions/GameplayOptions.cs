using System.ComponentModel;
using DebuggerOptions.Core;
using Gameplay.StateMachine.States;
using Gameplay.StateMachine.States.Core;
using Infrastructure.StateMachine.Main.Core;

namespace Gameplay.DebuggerOptions
{
    public class GameplayOptions : BaseOptions
    {
        private const string Category = "Gameplay";

        private readonly IStateMachine<IGameplayState> _stateMachine;

        public GameplayOptions(IStateMachine<IGameplayState> stateMachine)
        {
            _stateMachine = stateMachine;
        }

        [Category(Category)]
        public void LoadMainMenu() => _stateMachine.Enter<LoadMenuState>();
    }
}