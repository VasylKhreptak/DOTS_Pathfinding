using Infrastructure.StateMachine.Main;
using Menu.StateMachine.States.Core;

namespace Menu.StateMachine
{
    public class MenuStateMachine : StateMachine<IMenuState>
    {
        public MenuStateMachine(MenuStateFactory stateFactory) : base(stateFactory) { }
    }
}