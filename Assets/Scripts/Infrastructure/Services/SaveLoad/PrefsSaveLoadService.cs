using System;
using Infrastructure.Services.Json.Core;
using Infrastructure.Services.SaveLoad.Core;
using UnityEngine;

namespace Infrastructure.Services.SaveLoad
{
    public class PrefsSaveLoadService : ISaveLoadService
    {
        private readonly IJsonService _jsonService;

        public PrefsSaveLoadService(IJsonService jsonService)
        {
            _jsonService = jsonService;
        }

        public void Save<T>(string key, T data)
        {
            string jsonData = _jsonService.Serialize(data);

            PlayerPrefs.SetString(key, jsonData);
            PlayerPrefs.Save();
        }

        public T Load<T>(string key, T defaultValue = default)
        {
            if (HasKey(key))
            {
                string jsonData = PlayerPrefs.GetString(key);

                try
                {
                    T instance = _jsonService.Deserialize<T>(jsonData);

                    if (instance == null)
                        return defaultValue;

                    return instance;
                }
                catch (Exception)
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }

        public bool HasKey(string key) => PlayerPrefs.HasKey(key);

        public void Delete(string key) => PlayerPrefs.DeleteKey(key);
    }
}