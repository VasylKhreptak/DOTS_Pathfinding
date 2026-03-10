using Gameplay.StateMachine.States;
using Gameplay.StateMachine.States.Core;
using Infrastructure.StateMachine.Main.Core;
using UI.Common;
using VContainer;

namespace Gameplay.UI.Buttons
{
    public class RestartGameButton : BaseButton
    {
        private IStateMachine<IGameplayState> _stateMachine;

        [Inject]
        public void Construct(IStateMachine<IGameplayState> stateMachine)
        {
            _stateMachine = stateMachine;
        }

        protected override void OnClick() => _stateMachine.Enter<RestartState>();
    }
}