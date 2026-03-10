using System;
using Plugins.MinMaxProperties.Core;

namespace Plugins.MinMaxProperties
{
    [Serializable]
    public class FloatMinMax : MinMaxProperty<float>
    {
        public override float Random() => UnityEngine.Random.Range(Min, Max);
    }
}