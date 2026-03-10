using Cysharp.Threading.Tasks;

namespace Infrastructure.Services.AsyncSaveLoad.Core
{
    public interface IAsyncSaveLoadService
    {
        public UniTask SaveAsync<T>(string key, T data);

        public UniTask<T> LoadAsync<T>(string key, T defaultValue = default);

        public bool HasKey(string key);

        public UniTask DeleteAsync(string key);
    }
}