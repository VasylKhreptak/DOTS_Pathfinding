using System.Collections.Generic;
using DebuggerOptions;
using Infrastructure.Configs;
using Infrastructure.Coroutines.Runner;
using Infrastructure.Data.Models.Persistent;
using Infrastructure.Data.Models.Static;
using Infrastructure.Services.Asset;
using Infrastructure.Services.AsyncJson;
using Infrastructure.Services.AsyncSaveLoad;
using Infrastructure.Services.AsyncScene;
using Infrastructure.Services.FixedTickable;
using Infrastructure.Services.Framerate;
using Infrastructure.Services.ID;
using Infrastructure.Services.Input.NewInputSystem;
using Infrastructure.Services.Instantiate;
using Infrastructure.Services.Json;
using Infrastructure.Services.LateTickable;
using Infrastructure.Services.Log;
using Infrastructure.Services.SaveLoad;
using Infrastructure.Services.Scene;
using Infrastructure.Services.Tickable;
using Infrastructure.Services.TimeScale;
using Infrastructure.Services.Vibration;
using Infrastructure.Services.Window;
using Infrastructure.Services.Window.Core;
using Infrastructure.Services.Window.Factories;
using Infrastructure.StateMachine.Game;
using Infrastructure.StateMachine.Game.States;
using Infrastructure.StateMachine.Game.States.Core;
using Infrastructure.StateMachine.Main.Core;
using Plugins.AudioService;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem.UI;
using VContainer;
using VContainer.Unity;
using SettingsService = Infrastructure.Services.Settings.SettingsService;

namespace Infrastructure.VContainer.Scopes
{
    public class ProjectScope : LifetimeScope, IInitializable
    {
        [Header("References")]
        [SerializeField] private CoroutineRunner _coroutineRunnerPrefab;
        [SerializeField] private UnityEngine.GameObject _eventSystemPrefab;
        [SerializeField] private AudioMixer _audioMixer;

        [Header("Configs")]
        [SerializeField] private AudioService.Preferences _audioServicePreferences;
        [SerializeField] private VibrationServiceConfig _vibrationServiceConfig;
        [SerializeField] private WindowFactoryConfig _windowFactoryConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            RegisterInstances(builder);
            RegisterConfigs(builder);
            RegisterDataModels(builder);
            RegisterMonoServices(builder);
            RegisterServices(builder);
            RegisterStateMachine(builder);
            InitializeDebugger(builder);
            MakeInitializable(builder);
        }

        public void Initialize() => Container.Resolve<IStateMachine<IGameState>>().Enter<BootstrapState>();

        private void RegisterInstances(IContainerBuilder builder)
        {
            builder.RegisterInstance(_audioMixer);
        }

        private void RegisterConfigs(IContainerBuilder builder)
        {
            builder.RegisterInstance(_windowFactoryConfig);
        }

        private void RegisterDataModels(IContainerBuilder builder)
        {
            builder.Register<StaticDataModel>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<PersistentDataModel>(Lifetime.Singleton).AsImplementedInterfaces();
        }

        private void RegisterMonoServices(IContainerBuilder builder)
        {
            builder.RegisterComponentInNewPrefab(_coroutineRunnerPrefab, Lifetime.Singleton).AsImplementedInterfaces();
        }

        private void RegisterServices(IContainerBuilder builder)
        {
            builder.Register<SceneService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<AsyncSceneService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<JsonService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<AsyncJsonService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<IDService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<LogService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<FramerateService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<PrefsSaveLoadService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<PrefsAsyncSaveLoadService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<InstantiateService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<AssetService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<TickableService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<FixedTickableService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<LateTickableService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<AudioService>(Lifetime.Singleton).AsImplementedInterfaces().WithParameter(_audioServicePreferences);
            builder.Register<NewInputVibrationService>(Lifetime.Singleton).AsImplementedInterfaces().WithParameter(_vibrationServiceConfig);
            builder.Register<SettingsService>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<TimeScaleService>(Lifetime.Singleton).AsImplementedInterfaces();
            RegisterInputService(builder);
            RegisterWindowService(builder);
        }

        private void RegisterInputService(IContainerBuilder builder)
        {
            UnityEngine.GameObject eventSystemInstance = Instantiate(_eventSystemPrefab);
            DontDestroyOnLoad(eventSystemInstance);
            InputSystemUIInputModule uiInputModule = eventSystemInstance.GetComponent<InputSystemUIInputModule>();
            builder.Register<NewInputService>(Lifetime.Singleton).AsImplementedInterfaces().WithParameter(uiInputModule);
        }

        private void RegisterWindowService(IContainerBuilder builder)
        {
            builder.Register<SemaphoreProvider>(Lifetime.Singleton);
            builder.Register<WindowFactory>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<WindowService>(Lifetime.Scoped).AsImplementedInterfaces();

            // RegisterWindowTester(builder);
        }

        private void RegisterWindowTester(IContainerBuilder builder)
        {
            IReadOnlyList<WindowID> possibleWindowIds = new List<WindowID>();

            builder.Register<WindowAutoTester>(Lifetime.Scoped).AsImplementedInterfaces().WithParameter(possibleWindowIds);
        }

        private void RegisterStateMachine(IContainerBuilder builder)
        {
            RegisterStates(builder);
            builder.Register<GameStateFactory>(Lifetime.Singleton);
            builder.Register<GameStateMachine>(Lifetime.Singleton).AsImplementedInterfaces();
        }

        private void RegisterStates(IContainerBuilder builder)
        {
            //chained
            builder.Register<BootstrapState>(Lifetime.Singleton);
            builder.Register<LoadDataState>(Lifetime.Singleton);
            builder.Register<SetupApplicationState>(Lifetime.Singleton);
            builder.Register<FinalizeLoadingState>(Lifetime.Singleton);
            builder.Register<LoopState>(Lifetime.Singleton);

            //other
            builder.Register<ReloadState>(Lifetime.Singleton);
            builder.Register<LoadSceneState>(Lifetime.Singleton);
            builder.Register<SaveDataState>(Lifetime.Singleton);
            builder.Register<LoadSceneWithLoadingScreenState>(Lifetime.Singleton);
        }

        private void InitializeDebugger(IContainerBuilder builder)
        {
            SRDebug.Init();
            builder.Register<ProjectOptions>(Lifetime.Singleton).AsImplementedInterfaces();
        }

        private void MakeInitializable(IContainerBuilder builder)
        {
            builder.Register<IInitializable>(c => this, Lifetime.Singleton).As<IInitializable>();
        }
    }
}