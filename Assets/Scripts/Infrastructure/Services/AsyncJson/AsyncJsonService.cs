using Cysharp.Threading.Tasks;
using Infrastructure.Services.AsyncJson.Core;
using Infrastructure.Services.Json.Core;

namespace Infrastructure.Services.AsyncJson
{
    public class AsyncJsonService : IAsyncJsonService
    {
        private readonly IJsonService _jsonService;

        public AsyncJsonService(IJsonService jsonService)
        {
            _jsonService = jsonService;
        }

        public UniTask<string> SerializeAsync<T>(T obj) => UniTask.RunOnThreadPool(() => _jsonService.Serialize(obj));

        public UniTask<T> DeserializeAsync<T>(string json) => UniTask.RunOnThreadPool(() => _jsonService.Deserialize<T>(json));
    }
}