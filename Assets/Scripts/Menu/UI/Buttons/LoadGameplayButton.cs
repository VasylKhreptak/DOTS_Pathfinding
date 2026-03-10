using Infrastructure.StateMachine.Main.Core;
using Menu.StateMachine.States;
using Menu.StateMachine.States.Core;
using UI.Common;
using VContainer;

namespace Menu.UI.Buttons
{
    public class LoadGameplayButton : BaseButton
    {
        private IStateMachine<IMenuState> _stateMachine;

        [Inject]
        public void Construct(IStateMachine<IMenuState> stateMachine)
        {
            _stateMachine = stateMachine;
        }

        protected override void OnClick() => _stateMachine.Enter<LoadGameplayState>();
    }
}