using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Infrastructure.Services.AsyncJson.Core;
using Infrastructure.Services.AsyncSaveLoad.Core;
using UnityEngine;

namespace Infrastructure.Services.AsyncSaveLoad
{
    public class FileAsyncSaveLoadService : IAsyncSaveLoadService
    {
        private readonly IAsyncJsonService _jsonService;

        public FileAsyncSaveLoadService(IAsyncJsonService jsonService)
        {
            _jsonService = jsonService;
        }

        public async UniTask SaveAsync<T>(string key, T data)
        {
            string jsonData = await _jsonService.SerializeAsync(data);

            string path = GetPath(key);

            await File.WriteAllTextAsync(path, jsonData);
        }

        public async UniTask<T> LoadAsync<T>(string key, T defaultValue = default)
        {
            string path = GetPath(key);

            if (File.Exists(path))
            {
                string jsonData = await File.ReadAllTextAsync(path);

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

        public bool HasKey(string key)
        {
            string path = GetPath(key);

            return File.Exists(path);
        }

        public async UniTask DeleteAsync(string key)
        {
            string path = GetPath(key);

            if (File.Exists(path))
                await UniTask.RunOnThreadPool(() => File.Delete(path));
        }

        private string GetPath(string key) => Path.Combine(Application.persistentDataPath, key);
    }
}