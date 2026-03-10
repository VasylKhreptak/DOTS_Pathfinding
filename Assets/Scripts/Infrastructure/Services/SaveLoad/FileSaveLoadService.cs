using System;
using System.IO;
using Infrastructure.Services.Json.Core;
using Infrastructure.Services.SaveLoad.Core;
using UnityEngine;

namespace Infrastructure.Services.SaveLoad
{
    public class FileSaveLoadService : ISaveLoadService
    {
        private readonly IJsonService _jsonService;

        public FileSaveLoadService(IJsonService jsonService)
        {
            _jsonService = jsonService;
        }

        public void Save<T>(string key, T data)
        {
            string jsonData = _jsonService.Serialize(data);

            string path = GetPath(key);

            File.WriteAllText(path, jsonData);
        }

        public T Load<T>(string key, T defaultValue = default)
        {
            string path = GetPath(key);

            if (File.Exists(path))
            {
                string jsonData = File.ReadAllText(path);

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

        public bool HasKey(string key)
        {
            string path = GetPath(key);

            return File.Exists(path);
        }

        public void Delete(string key)
        {
            string path = GetPath(key);

            if (File.Exists(path))
                File.Delete(path);
        }

        private string GetPath(string key) => Path.Combine(Application.persistentDataPath, key);
    }
}