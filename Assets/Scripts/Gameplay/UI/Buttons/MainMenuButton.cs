using Cysharp.Threading.Tasks;
using Gameplay.StateMachine.States;
using Gameplay.StateMachine.States.Core;
using Infrastructure.Services.Window.Core;
using Infrastructure.StateMachine.Main.Core;
using Infrastructure.UI.Popups.Core;
using UI.Common;
using VContainer;

namespace Gameplay.UI.Buttons
{
    public class MainMenuButton : BaseButton
    {
        private IStateMachine<IGameplayState> _gameplayStateMachine;
        private IWindowService _windowService;

        [Inject]
        public void Construct(IStateMachine<IGameplayState> gameplayStateMachine, IWindowService windowService)
        {
            _gameplayStateMachine = gameplayStateMachine;
            _windowService = windowService;
        }

        protected override void OnClick()
        {
            _windowService
                .CreateWindow(WindowID.ConfirmationPopup)
                .ContinueWith(popup =>
                {
                    IConfirmationPopup confirmationPopup = (IConfirmationPopup)popup;

                    confirmationPopup.SetHeader("Exit to Main Menu");
                    confirmationPopup.SetMainText("Are you sure you want to exit to the Main Menu?");
                    confirmationPopup.SetConfirmButtonText("Yes");
                    confirmationPopup.SetDeclineButtonText("No");

                    confirmationPopup.Show().ContinueWith(() => confirmationPopup.SelectDeclineButton()).Forget();

                    confirmationPopup.ResultTask
                        .ContinueWith(result =>
                        {
                            if (result == ConfirmationPopupResult.Yes)
                                _gameplayStateMachine.Enter<LoadMenuState>();
                            else
                                confirmationPopup.Hide().Forget();
                        })
                        .Forget();
                })
                .Forget();
        }
    }
}