using NUnit.Framework;
using UnityEngine;

namespace Plugins.Extensions.Tests.ExtensionsEditModeTests
{
    public class AnimationCurveExtensionsTests
    {
        [Test]
        public void EvaluateTest()
        {
            AnimationCurve linearCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

            Assert.AreEqual(0f, linearCurve.Evaluate(0f, 1f, 0f));
            Assert.AreEqual(0.5f, linearCurve.Evaluate(0f, 1f, 0.5f));
            Assert.AreEqual(1f, linearCurve.Evaluate(0f, 1f, 1f));
            Assert.AreEqual(2f, linearCurve.Evaluate(1f, 2f, 1f));
            Assert.AreEqual(1f, linearCurve.Evaluate(0f, 2f, 0.5f));
        }

        [Test]
        public void EvaluateWithRangeTest()
        {
            AnimationCurve linearCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

            Assert.AreEqual(0.5f, linearCurve.Evaluate(0.5f, 0f, 1f, 0f, 1f));
            Assert.AreEqual(1f, linearCurve.Evaluate(0.5f, 0f, 1f, 0f, 2f));
            Assert.AreEqual(3f, linearCurve.Evaluate(0.5f, 0f, 1f, 2f, 4f));
            Assert.AreEqual(-0.5f, linearCurve.Evaluate(0.5f, 0f, 1f, -1f, 0f));
        }
    }
}