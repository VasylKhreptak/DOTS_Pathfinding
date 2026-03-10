using System;
using System.Collections.Generic;
using Infrastructure.StateMachine.Main.States.Core;
using Infrastructure.StateMachine.Main.States.Factory;
using Menu.StateMachine.States;
using VContainer;

namespace Menu.StateMachine
{
    public class MenuStateFactory : StateFactory
    {
        public MenuStateFactory(IObjectResolver resolver) : base(resolver) { }

        protected override Dictionary<Type, Func<IBaseState>> BuildStatesMap() =>
            new Dictionary<Type, Func<IBaseState>>
            {
                //chained
                [typeof(BootstrapState)] = Resolver.Resolve<BootstrapState>,
                [typeof(SetupUIState)] = Resolver.Resolve<SetupUIState>,
                [typeof(FinalizeLoadingState)] = Resolver.Resolve<FinalizeLoadingState>,
                [typeof(LoopState)] = Resolver.Resolve<LoopState>,

                //other
                [typeof(LoadGameplayState)] = Resolver.Resolve<LoadGameplayState>
            };
    }
}