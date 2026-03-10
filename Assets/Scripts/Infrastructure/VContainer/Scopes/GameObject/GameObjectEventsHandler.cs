using System;
using Infrastructure.Services.FixedTickable.Core;
using Infrastructure.Services.LateTickable.Core;
using Infrastructure.Services.Tickable.Core;
using Infrastructure.VContainer.Scopes.GameObject.EventHandlers;
using UniRx;
using UniRx.Triggers;
using VContainer.Unity;
using IFixedTickable = Infrastructure.Services.FixedTickable.Core.IFixedTickable;
using ILateTickable = Infrastructure.Services.LateTickable.Core.ILateTickable;
using ITickable = Infrastructure.Services.Tickable.Core.ITickable;

namespace Infrastructure.VContainer.Scopes.GameObject
{
    public class GameObjectEventsHandler : IInitializable, IStartable, IDisposable
    {
        private readonly UnityEngine.GameObject _gameObject;
        private readonly ITickableService _tickableService;
        private readonly IFixedTickableService _fixedTickableService;
        private readonly ILateTickableService _lateTickableService;

        public GameObjectEventsHandler(UnityEngine.GameObject gameObject, ITickableService tickableService, IFixedTickableService fixedTickableService,
            ILateTickableService lateTickableService)
        {
            _gameObject = gameObject;
            _tickableService = tickableService;
            _fixedTickableService = fixedTickableService;
            _lateTickableService = lateTickableService;
        }

        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        private bool _enabled;

        public virtual void Initialize()
        {
            _gameObject.OnEnableAsObservable().Subscribe(_ => OnEnable()).AddTo(_subscriptions);
            _gameObject.OnDisableAsObservable().Subscribe(_ => OnDisable()).AddTo(_subscriptions);
        }

        public virtual void Start()
        {
            if (_gameObject.activeInHierarchy)
                OnEnable();
        }

        public virtual void Dispose() => _subscriptions?.Dispose();

        private void OnEnable()
        {
            if (this is IEnableable enableable)
                enableable.Enable();

            if (this is ITickable tickable)
                _tickableService.Add(tickable);

            if (this is IFixedTickable fixedTickable)
                _fixedTickableService.Add(fixedTickable);

            if (this is ILateTickable lateTickable)
                _lateTickableService.Add(lateTickable);

            _enabled = true;
        }

        private void OnDisable()
        {
            if (!_enabled)
                return;

            if (this is IDisableable disableable)
                disableable.Disable();

            if (this is ITickable tickable)
                _tickableService.Remove(tickable);

            if (this is IFixedTickable fixedTickable)
                _fixedTickableService.Remove(fixedTickable);

            if (this is ILateTickable lateTickable)
                _lateTickableService.Remove(lateTickable);
        }
    }
}