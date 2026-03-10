using System;
using Cysharp.Threading.Tasks;

namespace Infrastructure.Services.AsyncScene.Core
{
    public interface IAsyncSceneService
    {
        public UniTask Load(string name, IProgress<float> progress = null);

        public UniTask LoadCurrent(IProgress<float> progress = null);
    }
}