using UniRx;

namespace UI.Wrappers.Core
{
    public interface ISelectableWrapper
    {
        public IReadOnlyReactiveProperty<SelectableStateInfo> CurrentStateInfo { get; }
    }
}