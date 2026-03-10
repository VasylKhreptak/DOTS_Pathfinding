using NUnit.Framework;
using UnityEngine;

namespace Plugins.Extensions.Tests.ExtensionsEditModeTests
{
    public class LayerMaskExtensionsTests
    {
        [Test]
        public void ContainsLayerTest()
        {
            LayerMask layerMask = 1 << 0;
            bool result = layerMask.ContainsLayer(0);
            Assert.IsTrue(result);
        }

        [Test]
        public void NotContainsLayerTest()
        {
            LayerMask layerMask = 1 << 0;
            bool result = layerMask.ContainsLayer(1);
            Assert.IsFalse(result);
        }
    }
}