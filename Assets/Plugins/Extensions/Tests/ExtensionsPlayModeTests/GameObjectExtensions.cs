using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Plugins.Extensions.Tests.ExtensionsPlayModeTests
{
    public class GameObjectExtensions
    {
        [UnityTest]
        public IEnumerator TryAddComponentTest()
        {
            yield return null;

            GameObject gameObject = new GameObject();

            Rigidbody component = gameObject.TryAddComponent<Rigidbody>();

            Assert.IsNotNull(component);
            Assert.AreEqual(component, gameObject.GetComponent<Rigidbody>());

            Rigidbody existingComponent = gameObject.TryAddComponent<Rigidbody>();

            Assert.AreEqual(component, existingComponent);
        }

        [UnityTest]
        public IEnumerator GetChildrenTest()
        {
            yield return null;

            GameObject gameObject = new GameObject();
            GameObject child1 = new GameObject();
            GameObject child2 = new GameObject();
            GameObject child3 = new GameObject();

            child1.transform.SetParent(gameObject.transform);
            child2.transform.SetParent(gameObject.transform);
            child3.transform.SetParent(gameObject.transform);

            GameObject[] children = gameObject.GetChildren();

            Assert.AreEqual(3, children.Length);
            Assert.Contains(child1, children);
            Assert.Contains(child2, children);
            Assert.Contains(child3, children);
        }
    }
}