namespace Infrastructure.Services.Scene.Core
{
    public interface ISceneService
    {
        public void Load(string name);

        public void LoadCurrent();
    }
}