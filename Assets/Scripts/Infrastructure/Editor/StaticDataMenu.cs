using UnityEditor;
using UnityEngine;

namespace Infrastructure.Editor
{
    public static class StaticDataMenu
    {
        [MenuItem("Game/⚙ Config %F1")]
        public static void OpenGameConfig() => SelectObject("Assets/Resources/StaticData/GameConfig.asset");

        [MenuItem("Game/📈 Balance %F2")]
        public static void OpenGameBalance() => SelectObject("Assets/Resources/StaticData/GameBalance.asset");

        private static void SelectObject(string path)
        {
            Object targetAsset = AssetDatabase.LoadAssetAtPath<Object>(path);

            Selection.activeObject = targetAsset;
            EditorGUIUtility.PingObject(targetAsset);
        }
    }
}