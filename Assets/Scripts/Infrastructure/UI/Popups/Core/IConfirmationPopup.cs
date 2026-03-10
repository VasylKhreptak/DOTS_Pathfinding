using Cysharp.Threading.Tasks;

namespace Infrastructure.UI.Popups.Core
{
    public interface IConfirmationPopup : IPopup
    {
        public UniTask<ConfirmationPopupResult> ResultTask { get; }

        public void SetHeader(string text);

        public void SetMainText(string text);

        public void SetConfirmButtonText(string text);

        public void SetDeclineButtonText(string text);

        public void SelectConfirmButton();

        public void SelectDeclineButton();
    }
}