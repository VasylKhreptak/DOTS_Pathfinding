using NUnit.Framework;
using UnityEngine;

namespace Plugins.Extensions.Tests.ExtensionsEditModeTests
{
    public class Vector2ExtensionsTests
    {
        [Test]
        public void WithXTest()
        {
            Vector2 vector = new Vector2(1, 2);
            Vector2 newVector = vector.WithX(4);
            Assert.AreEqual(new Vector2(4, 2), newVector);
        }

        [Test]
        public void WithYTest()
        {
            Vector2 vector = new Vector2(1, 2);
            Vector2 newVector = vector.WithY(4);
            Assert.AreEqual(new Vector2(1, 4), newVector);
        }

        [Test]
        public void IsCloseToTest()
        {
            Vector2 vector = new Vector2(1, 2);
            Vector2 other = new Vector2(1.01f, 2.01f);
            Assert.IsTrue(vector.IsCloseTo(other, 0.1f));
        }

        [Test]
        public void IsNotCloseToTest()
        {
            Vector2 vector = new Vector2(1, 2);
            Vector2 other = new Vector2(1.01f, 2.01f);
            Assert.IsFalse(vector.IsCloseTo(other, 0.001f));
        }
    }
}