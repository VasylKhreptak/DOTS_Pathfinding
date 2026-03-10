using NUnit.Framework;
using UnityEngine;

namespace Plugins.Extensions.Tests.ExtensionsEditModeTests
{
    public class AssetDatabaseExtensionsTests
    {
        [Test]
        public void BaseTest()
        {
            Texture2D texture = AssetDatabaseExtensions.FindFirstOrDefault<Texture2D>();

            Assert.IsNotNull(texture);
        }
    }
}