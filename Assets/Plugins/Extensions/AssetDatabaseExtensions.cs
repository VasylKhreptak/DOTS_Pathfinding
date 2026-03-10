using UnityEditor;
using UnityEngine;

namespace Plugins.Extensions
{
    public static class AssetDatabaseExtensions
    {
        public static T FindFirstOrDefault<T>() where T : Object
        {
#if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");

            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return default;
#else
            Debug.LogError("AssetDatabaseExtensions.FindFirstOrDefault<T> is only available in the editor.");

            return default;
#endif
        }
    }
}