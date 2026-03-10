using System;
using Sirenix.OdinInspector;
using UI.Wrappers.Core;
using UniRx;
using UnityEngine;

namespace UI.Animations.Selectables.Core
{
    public abstract class BaseAnimation : SerializedMonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ISelectableWrapper _selectableWrapper;

        private IDisposable _subscription;

        #region MonoBehaviour

        protected virtual void OnValidate() => _selectableWrapper ??= GetComponentInParent<ISelectableWrapper>();

        protected virtual void Awake() => _subscription = _selectableWrapper.CurrentStateInfo.Subscribe(OnStateInfoChanged);

        protected virtual void OnDestroy() => _subscription?.Dispose();

        #endregion

        protected abstract void OnStateInfoChanged(SelectableStateInfo stateInfo);
    }
}