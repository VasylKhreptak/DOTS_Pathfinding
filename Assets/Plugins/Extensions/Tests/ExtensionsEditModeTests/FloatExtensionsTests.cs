using NUnit.Framework;

namespace Plugins.Extensions.Tests.ExtensionsEditModeTests
{
    public class FloatExtensionsTests
    {
        [Test]
        public void IsBetweenTest()
        {
            float value = 5f;

            Assert.IsTrue(value.IsBetween(0f, 10f));
            Assert.IsFalse(value.IsBetween(10f, 20f));
        }

        [Test]
        public void RemapTest()
        {
            Assert.AreEqual(0.5f, 5f.Remap(0f, 10f, 0f, 1f));
            Assert.AreEqual(0.5f, 5f.Remap(10f, 0f, 1f, 0f));
        }
    }
}