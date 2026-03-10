using NUnit.Framework;
using UnityEngine;

namespace Plugins.Extensions.Tests.ExtensionsEditModeTests
{
    public class ColorExtensionsTests
    {
        [Test]
        public void WithAlphaTest()
        {
            Color color = Color.red;

            color = color.WithAlpha(0.5f);

            Assert.AreEqual(0.5f, color.a);
        }
    }
}