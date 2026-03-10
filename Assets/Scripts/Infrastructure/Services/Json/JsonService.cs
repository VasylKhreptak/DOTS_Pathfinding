using Infrastructure.Services.Json.Core;
using Newtonsoft.Json;

namespace Infrastructure.Services.Json
{
    public class JsonService : IJsonService
    {
        public string Serialize<T>(T obj) => JsonConvert.SerializeObject(obj);

        public T Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json);
    }
}