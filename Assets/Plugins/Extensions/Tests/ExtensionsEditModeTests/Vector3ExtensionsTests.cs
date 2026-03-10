using NUnit.Framework;
using UnityEngine;

namespace Plugins.Extensions.Tests.ExtensionsEditModeTests
{
    public class Vector3ExtensionsTests
    {
        [Test]
        public void WithXTest()
        {
            Vector3 vector = new Vector3(1, 2, 3);
            Vector3 newVector = vector.WithX(4);
            Assert.AreEqual(new Vector3(4, 2, 3), newVector);
        }

        [Test]
        public void WithYTest()
        {
            Vector3 vector = new Vector3(1, 2, 3);
            Vector3 newVector = vector.WithY(4);
            Assert.AreEqual(new Vector3(1, 4, 3), newVector);
        }

        [Test]
        public void WithZTest()
        {
            Vector3 vector = new Vector3(1, 2, 3);
            Vector3 newVector = vector.WithZ(4);
            Assert.AreEqual(new Vector3(1, 2, 4), newVector);
        }

        [Test]
        public void WithVector2Test()
        {
            Vector3 vector = new Vector3(1, 2, 3);
            Vector2 other = new Vector2(4, 5);
            Vector3 newVector = vector.WithVector2(other);
            Assert.AreEqual(new Vector3(4, 5, 3), newVector);
        }

        [Test]
        public void IsCloseToTest()
        {
            Vector3 vector = new Vector3(1, 2, 3);
            Vector3 other = new Vector3(1.01f, 2.01f, 3.01f);
            Assert.IsTrue(vector.IsCloseTo(other, 0.1f));
        }

        [Test]
        public void IsNotCloseToTest()
        {
            Vector3 vector = new Vector3(1, 2, 3);
            Vector3 other = new Vector3(1.01f, 2.01f, 3.01f);
            Assert.IsFalse(vector.IsCloseTo(other, 0.001f));
        }
    }
}