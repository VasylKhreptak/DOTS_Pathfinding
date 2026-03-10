using System;
using Cysharp.Threading.Tasks;
using Infrastructure.Services.AsyncScene.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Infrastructure.Services.AsyncScene
{
    public class AsyncSceneService : IAsyncSceneService
    {
        public async UniTask Load(string name, IProgress<float> progress = null)
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(name);

            if (asyncOperation == null)
                throw new Exception($"Scene '{name}' could not be loaded.");

            while (!asyncOperation.isDone)
            {
                progress?.Report(asyncOperation.progress);

                if (asyncOperation.progress >= 1f)
                    break;

                await UniTask.Yield();
            }

            progress?.Report(1f);
        }

        public UniTask LoadCurrent(IProgress<float> progress = null) => Load(SceneManager.GetActiveScene().name);
    }
}