using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Gameplay.DebuggerOptions;
using Gameplay.ObjectPoolSystem;
using Gameplay.Services.SystemsState;
using Gameplay.StateMachine;
using Gameplay.StateMachine.States;
using Gameplay.StateMachine.States.Core;
using Infrastructure.Services.Window.Core;
using Infrastructure.StateMachine.Main.Core;
using Plugins.ObjectPoolSystem;
using Unity.Scenes;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Gameplay
{
    public class GameplayScope : LifetimeScope, IInitializable
    {
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private ObjectPoolPreference<GameplayPoolType>[] _objectPoolPreferences;
        [SerializeField] private List<SubScene> _subScenes;
        [SerializeField] private LoadMenuState.Preferences _loadMenuStatePreferences;

        #region MonoBehaviour

        private void OnValidate()
        {
            if (_subScenes == null || _subScenes.Count == 0)
                _subScenes = FindObjectsByType<SubScene>(FindObjectsSortMode.None).ToList();
        }

        #endregion

        protected override void Configure(IContainerBuilder builder)
        {
            RegisterInstances(builder);
            RegisterObjectPools(builder);
            RegisterServices(builder);
            RegisterStateMachine(builder);
            RegisterDebuggerOptions(builder);
            MakeInitializable(builder);
        }

        public void Initialize() => Container.Resolve<IStateMachine<IGameplayState>>().Enter<BootstrapState>();

        private void RegisterInstances(IContainerBuilder builder)
        {
            builder.RegisterInstance(_mainCamera);
        }

        private void RegisterObjectPools(IContainerBuilder builder)
        {
            foreach (ObjectPoolPreference<GameplayPoolType> objectPoolPreference in _objectPoolPreferences)
            {
                objectPoolPreference.CreateFunc = _ => UniTask.FromResult(Instantiate(objectPoolPreference.Prefab));
            }

            builder.Register<ObjectPools<GameplayPoolType>>(Lifetime.Singleton).WithParameter(_objectPoolPreferences).AsImplementedInterfaces();
        }

        private void RegisterServices(IContainerBuilder builder)
        {
            builder.Register<SystemsStateService>(Lifetime.Singleton).AsImplementedInterfaces();
        }

        private void RegisterStateMachine(IContainerBuilder builder)
        {
            RegisterStates(builder);
            builder.Register<GameplayStateFactory>(Lifetime.Singleton);
            builder.Register<GameplayStateMachine>(Lifetime.Singleton).AsImplementedInterfaces();
        }

        private void RegisterStates(IContainerBuilder builder)
        {
            IWindowService parentWindowService = Parent.Container.Resolve<IWindowService>();

            //chained
            builder.Register<BootstrapState>(Lifetime.Singleton);
            builder.Register<AwaitSubScenesLoadingState>(Lifetime.Singleton).WithParameter((IReadOnlyList<SubScene>)_subScenes);
            builder.Register<InitializeObjectPoolsState>(Lifetime.Singleton);
            builder.Register<CreateSystemsState>(Lifetime.Singleton);
            builder.Register<SetupLevelState>(Lifetime.Singleton);
            builder.Register<SetupUIState>(Lifetime.Singleton);
            builder.Register<FinalizeLoadingState>(Lifetime.Singleton).WithParameter(parentWindowService);
            builder.Register<LoopState>(Lifetime.Singleton);

            //other
            builder.Register<SaveDataState>(Lifetime.Singleton);
            builder.Register<LoadMenuState>(Lifetime.Singleton).WithParameter(parentWindowService).WithParameter(_loadMenuStatePreferences);
            builder.Register<DestroySystemsState>(Lifetime.Singleton);
            builder.Register<RestartState>(Lifetime.Singleton);
        }

        private void RegisterDebuggerOptions(IContainerBuilder builder)
        {
            builder.Register<GameplayOptions>(Lifetime.Singleton).AsImplementedInterfaces();
        }

        private void MakeInitializable(IContainerBuilder builder)
        {
            builder.Register<IInitializable>(c => this, Lifetime.Singleton).As<IInitializable>();
        }
    }
}