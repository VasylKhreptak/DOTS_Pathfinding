using System;
using Cysharp.Threading.Tasks;
using Infrastructure.Services.AsyncJson.Core;
using Infrastructure.Services.AsyncSaveLoad.Core;
using UnityEngine;

namespace Infrastructure.Services.AsyncSaveLoad
{
    public class PrefsAsyncSaveLoadService : IAsyncSaveLoadService
    {
        private readonly IAsyncJsonService _jsonService;

        public PrefsAsyncSaveLoadService(IAsyncJsonService jsonService)
        {
            _jsonService = jsonService;
        }

        public async UniTask SaveAsync<T>(string key, T data)
        {
            string jsonData = await _jsonService.SerializeAsync(data);

            PlayerPrefs.SetString(key, jsonData);
        }

        public async UniTask<T> LoadAsync<T>(string key, T defaultValue = default)
        {
            if (HasKey(key))
            {
                string jsonData = PlayerPrefs.GetString(key);

                try
                {
                    T instance = await _jsonService.DeserializeAsync<T>(jsonData);

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

        public UniTask DeleteAsync(string key)
        {
            PlayerPrefs.DeleteKey(key);
            return UniTask.CompletedTask;
        }
    }
}