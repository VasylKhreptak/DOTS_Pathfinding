using UnityEngine;

namespace Plugins.Extensions
{
    public static class AudioExtensions
    {
        public static float To01(float db)
        {
            db = Mathf.Clamp(db, -80f, 20);
            return Mathf.Pow(10f, db / 20f);
        }

        public static float ToDB(float value)
        {
            value = Mathf.Clamp(value, 0.0001f, 1f);
            return Mathf.Log10(value) * 20f;
        }
    }
}