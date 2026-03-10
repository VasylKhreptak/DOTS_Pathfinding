using Cysharp.Threading.Tasks;

namespace Infrastructure.Services.AsyncJson.Core
{
    public interface IAsyncJsonService
    {
        public UniTask<string> SerializeAsync<T>(T obj);

        public UniTask<T> DeserializeAsync<T>(string json);
    }
}