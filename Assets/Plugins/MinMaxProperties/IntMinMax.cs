using System;
using Plugins.MinMaxProperties.Core;

namespace Plugins.MinMaxProperties
{
    [Serializable]
    public class IntMinMax : MinMaxProperty<int>
    {
        public override int Random() => UnityEngine.Random.Range(Min, Max);
    }
}