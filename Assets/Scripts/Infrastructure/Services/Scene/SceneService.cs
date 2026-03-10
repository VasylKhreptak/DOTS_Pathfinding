using Infrastructure.Services.Scene.Core;
using UnityEngine.SceneManagement;

namespace Infrastructure.Services.Scene
{
    public class SceneService : ISceneService
    {
        public void Load(string name) => SceneManager.LoadScene(name);

        public void LoadCurrent() => Load(SceneManager.GetActiveScene().name);
    }
}