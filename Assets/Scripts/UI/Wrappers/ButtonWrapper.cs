using System;
using UI.Wrappers.Core;
using UniRx;
using Button = UnityEngine.UI.Button;

namespace UI.Wrappers
{
    public class ButtonWrapper : Button, ISelectableWrapper
    {
        private readonly ReactiveProperty<SelectableStateInfo> _currentStateInfo = new ReactiveProperty<SelectableStateInfo>();

        public IReadOnlyReactiveProperty<SelectableStateInfo> CurrentStateInfo => _currentStateInfo;

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            SelectableState selectableState = state switch
            {
                SelectionState.Normal => SelectableState.Normal,
                SelectionState.Highlighted => SelectableState.Highlighted,
                SelectionState.Pressed => SelectableState.Pressed,
                SelectionState.Selected => SelectableState.Selected,
                SelectionState.Disabled => SelectableState.Disabled,
                var _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };

            _currentStateInfo.Value = new SelectableStateInfo(selectableState, instant);
        }
    }
}