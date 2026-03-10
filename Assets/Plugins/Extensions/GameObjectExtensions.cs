using UnityEngine;

namespace Plugins.Extensions
{
    public static class GameObjectExtensions
    {
        public static T TryAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject.TryGetComponent(out T existingComponent))
                return existingComponent;

            return gameObject.AddComponent<T>();
        }

        public static GameObject[] GetChildren(this GameObject gameObject)
        {
            GameObject[] children = new GameObject[gameObject.transform.childCount];

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                children[i] = gameObject.transform.GetChild(i).gameObject;
            }

            return children;
        }
    }
}