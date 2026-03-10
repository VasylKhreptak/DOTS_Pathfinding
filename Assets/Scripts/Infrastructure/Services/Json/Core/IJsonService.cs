namespace Infrastructure.Services.Json.Core
{
    public interface IJsonService
    {
        public string Serialize<T>(T obj);

        public T Deserialize<T>(string json);
    }
}