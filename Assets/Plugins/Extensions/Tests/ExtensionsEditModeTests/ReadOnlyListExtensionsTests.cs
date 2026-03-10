using System.Collections.Generic;
using NUnit.Framework;

namespace Plugins.Extensions.Tests.ExtensionsEditModeTests
{
    public class ReadOnlyListExtensionsTests
    {
        [Test]
        public void IndexOfTest()
        {
            IReadOnlyList<int> list = new[] { 1, 2, 3, 4, 5 };

            Assert.AreEqual(2, list.IndexOf(3));
            Assert.AreEqual(-1, list.IndexOf(6));
        }
    }
}