using System.IO;
using UnityEditor;
using UnityEngine;

namespace Infrastructure.Editor
{
    public static class ClearDataMenu
    {
        [MenuItem("Game/🧹 Clear Data %F12")]
        public static void ClearData()
        {
            string path = Path.Combine(Application.persistentDataPath, "Data");

            if (File.Exists(path))
                File.Delete(path);

            PlayerPrefs.DeleteAll();

            Debug.Log("Deleted all data");
        }
    }
}