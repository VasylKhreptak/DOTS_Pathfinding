using UnityEditor;
using UnityEngine;

namespace Infrastructure.Editor
{
    public static class ToggleGpuInstancing
    {
        [MenuItem("Game/Tools/GPU instancing/Enable")]
        public static void EnableGpuInstancing() => SetGpuInstancing(true);

        [MenuItem("Game/Tools/GPU instancing/Disable")]
        public static void DisableGpuInstancing() => SetGpuInstancing(false);

        private static void SetGpuInstancing(bool enabled)
        {
            string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Art" });

            foreach (string guid in materialGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

                if (material != null && material.shader != null)
                {
                    if (material.shader.isSupported && material.HasProperty("_EnableInstancing"))
                    {
                        material.enableInstancing = enabled;
                        EditorUtility.SetDirty(material);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"GPU instancing has been {(enabled ? "enabled" : "disabled")} for {materialGuids.Length} materials.");
        }
    }
}