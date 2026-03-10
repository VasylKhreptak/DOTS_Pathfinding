using Cysharp.Threading.Tasks;

namespace Infrastructure.UI.Popups.Core
{
    public interface IContinuationPopup : IPopup
    {
        public UniTask ContinueTask { get; }
    }
}