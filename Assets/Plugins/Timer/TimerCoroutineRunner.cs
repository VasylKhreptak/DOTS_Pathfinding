using System.Collections;
using UnityEngine;

namespace Plugins.Timer
{
    public static class TimerCoroutineRunner
    {
        private static CoroutineRunnerBehaviour _behaviour;

        private static bool _initialized;

        private static void Initialize()
        {
            if (_initialized)
                return;

            GameObject gameObject = new GameObject("Timer Coroutine Runner");
            Object.DontDestroyOnLoad(gameObject);
            _behaviour = gameObject.AddComponent<CoroutineRunnerBehaviour>();

            _initialized = true;
        }

        public static Coroutine Run(IEnumerator routine)
        {
            Initialize();

            return _behaviour.StartCoroutine(routine);
        }

        public static void Stop(Coroutine routine)
        {
            Initialize();

            _behaviour.StopCoroutine(routine);
        }

        public class CoroutineRunnerBehaviour : MonoBehaviour { }
    }
}