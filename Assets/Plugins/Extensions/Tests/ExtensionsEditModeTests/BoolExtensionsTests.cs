using NUnit.Framework;

namespace Plugins.Extensions.Tests.ExtensionsEditModeTests
{
    public class BoolExtensionsTests
    {
        [Test]
        public void ToggleTest()
        {
            bool value = true;

            value = value.Toggle();

            Assert.IsFalse(value);

            value = value.Toggle();

            Assert.IsTrue(value);
        }

        [Test]
        public void SignTest()
        {
            bool value = true;

            Assert.AreEqual(1, value.ToSign());

            value = false;

            Assert.AreEqual(-1, value.ToSign());
        }

        [Test]
        public void IntTest()
        {
            bool value = true;

            Assert.AreEqual(1, value.ToInt());

            value = false;

            Assert.AreEqual(0, value.ToInt());
        }
    }
}