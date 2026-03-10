using NUnit.Framework;

namespace Plugins.Extensions.Tests.ExtensionsEditModeTests
{
    public class MathfExtensionsTests
    {
        [Test]
        public void ZeroProbabilityTest() => Assert.IsFalse(MathfExtensions.Probability(0f));

        [Test]
        public void OneProbabilityTest() => Assert.IsTrue(MathfExtensions.Probability(1f));
    }
}