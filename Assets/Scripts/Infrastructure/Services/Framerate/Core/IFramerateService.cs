using UniRx;

namespace Infrastructure.Services.Framerate.Core
{
    public interface IFramerateService
    {
        public IReadOnlyReactiveProperty<float> AverageFramerate { get; }

        public void SetTargetFramerate(int framerate);
    }
}