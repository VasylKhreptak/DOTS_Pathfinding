using Newtonsoft.Json;
using NUnit.Framework;
using Plugins.Health.Core;

namespace Plugins.Health.Tests.EditMode
{
    public class SerializationTests
    {
        [Test]
        public void Test()
        {
            IHealth health = new Health(50, 100);

            string jsonString = JsonConvert.SerializeObject(health);

            IHealth deserializedHealth = JsonConvert.DeserializeObject<Health>(jsonString);

            Assert.AreEqual(health.Value.Value, deserializedHealth.Value.Value);
            Assert.AreEqual(health.MaxValue.Value, deserializedHealth.MaxValue.Value);
            Assert.AreEqual(health.FillAmount.Value, deserializedHealth.FillAmount.Value);
            Assert.AreEqual(health.IsDeath.Value, deserializedHealth.IsDeath.Value);
            Assert.AreEqual(health.IsFull.Value, deserializedHealth.IsFull.Value);
        }
    }
}