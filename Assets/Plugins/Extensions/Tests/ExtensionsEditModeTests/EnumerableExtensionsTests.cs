using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Plugins.Extensions.Tests.ExtensionsEditModeTests
{
    public class EnumerableExtensionsTests
    {
        [Test]
        public void GetByWeightTest()
        {
            List<(int, float)> list = new List<(int, float)>
            {
                (1, 5f),
                (2, 3f),
                (3, 2f)
            };

            (int, float) result = list.GetByWeight(x => x.Item2);

            Assert.Contains(result, list);
        }

        [Test]
        public void RandomTest()
        {
            List<int> list = new List<int>
            {
                1,
                2,
                3,
                4,
                5
            };

            int result = list.Random();

            Assert.Contains(result, list);
        }

        [Test]
        public void ShuffleTest()
        {
            List<int> list = new List<int>
            {
                1,
                2,
                3,
                4,
                5
            };

            List<int> result = list.Shuffle().ToList();

            Assert.AreEqual(list.Count, result.Count);
            Assert.AreNotEqual(list, result);
            Assert.IsTrue(list.All(result.Contains));
        }
    }
}