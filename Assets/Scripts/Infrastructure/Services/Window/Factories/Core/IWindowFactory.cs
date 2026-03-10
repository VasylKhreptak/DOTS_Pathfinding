using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.Services.Window.Core;

namespace Infrastructure.Services.Window.Factories.Core
{
    public interface IWindowFactory
    {
        public UniTask<IWindow> CreateWindow(WindowID windowID, CancellationToken token = default);
    }
}