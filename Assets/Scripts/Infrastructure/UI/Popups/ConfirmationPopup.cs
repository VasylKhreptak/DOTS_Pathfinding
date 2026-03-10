using Cysharp.Threading.Tasks;
using Infrastructure.Tools;
using Infrastructure.UI.Popups.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.UI.Popups
{
    public class ConfirmationPopup : FadePopup, IConfirmationPopup
    {
        [Header("References")]
        [SerializeField] private TMP_Text _headerText;
        [SerializeField] private TMP_Text _mainText;
        [SerializeField] private TMP_Text _confirmButtonText;
        [SerializeField] private TMP_Text _declineButtonText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _declineButton;

        private readonly AutoResetCancellationTokenSource _inputCts = new AutoResetCancellationTokenSource();
        private readonly UniTaskCompletionSource<ConfirmationPopupResult> _resultTaskSource = new UniTaskCompletionSource<ConfirmationPopupResult>();

        public UniTask<ConfirmationPopupResult> ResultTask => _resultTaskSource.Task;

        #region MonoBehaviour

        private void OnDisable()
        {
            _confirmButton.onClick.RemoveListener(OnConfirmClicked);
            _declineButton.onClick.RemoveListener(OnDeclineClicked);
        }

        #endregion

        public override async UniTask Show()
        {
            _inputCts.Cancel();

            await base.Show();

            _confirmButton.onClick.RemoveListener(OnConfirmClicked);
            _confirmButton.onClick.AddListener(OnConfirmClicked);

            _declineButton.onClick.RemoveListener(OnDeclineClicked);
            _declineButton.onClick.AddListener(OnDeclineClicked);
        }

        public override async UniTask Hide()
        {
            _inputCts.Cancel();

            await base.Hide();
        }

        public void SetHeader(string text) => _headerText.text = text;

        public void SetMainText(string text) => _mainText.text = text;

        public void SetConfirmButtonText(string text) => _confirmButtonText.text = text;
        public void SetDeclineButtonText(string text) => _declineButtonText.text = text;
        public void SelectConfirmButton() => SelectGameObjectIfActive(_confirmButton.gameObject);

        public void SelectDeclineButton() => SelectGameObjectIfActive(_declineButton.gameObject);

        private void OnConfirmClicked() => _resultTaskSource.TrySetResult(ConfirmationPopupResult.Yes);

        private void OnDeclineClicked() => _resultTaskSource.TrySetResult(ConfirmationPopupResult.No);
    }
}