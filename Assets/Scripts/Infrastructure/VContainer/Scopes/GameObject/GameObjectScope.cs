using VContainer;
using VContainer.Unity;

namespace Infrastructure.VContainer.Scopes.GameObject
{
    public class GameObjectScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder) => builder.RegisterComponent(gameObject);
    }
}