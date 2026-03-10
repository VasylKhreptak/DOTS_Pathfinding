using System;
using System.Collections.Generic;
using Infrastructure.StateMachine.Main.States.Core;
using Infrastructure.StateMachine.Main.States.Factory;
using VContainer;

namespace Gameplay.StateMachine.States
{
    public class GameplayStateFactory : StateFactory
    {
        public GameplayStateFactory(IObjectResolver resolver) : base(resolver) { }

        protected override Dictionary<Type, Func<IBaseState>> BuildStatesMap() =>
            new Dictionary<Type, Func<IBaseState>>
            {
                //chained
                [typeof(BootstrapState)] = Resolver.Resolve<BootstrapState>,
                [typeof(AwaitSubScenesLoadingState)] = Resolver.Resolve<AwaitSubScenesLoadingState>,
                [typeof(InitializeObjectPoolsState)] = Resolver.Resolve<InitializeObjectPoolsState>,
                [typeof(SetupLevelState)] = Resolver.Resolve<SetupLevelState>,
                [typeof(SetupUIState)] = Resolver.Resolve<SetupUIState>,
                [typeof(CreateSystemsState)] = Resolver.Resolve<CreateSystemsState>,
                [typeof(FinalizeLoadingState)] = Resolver.Resolve<FinalizeLoadingState>,
                [typeof(LoopState)] = Resolver.Resolve<LoopState>,

                //other
                [typeof(SaveDataState)] = Resolver.Resolve<SaveDataState>,
                [typeof(LoadMenuState)] = Resolver.Resolve<LoadMenuState>,
                [typeof(DestroySystemsState)] = Resolver.Resolve<DestroySystemsState>,
                [typeof(RestartState)] = Resolver.Resolve<RestartState>
            };
    }
}