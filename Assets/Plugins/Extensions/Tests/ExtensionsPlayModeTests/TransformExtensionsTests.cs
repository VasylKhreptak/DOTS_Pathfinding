using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Plugins.Extensions.Tests.ExtensionsPlayModeTests
{
    public class TransformExtensionsTests
    {
        [UnityTest]
        public IEnumerator GetChildrenTest()
        {
            yield return null;

            GameObject parent = new GameObject("Parent");
            GameObject child1 = new GameObject("Child1");
            GameObject child2 = new GameObject("Child2");
            GameObject child3 = new GameObject("Child3");

            child1.transform.SetParent(parent.transform);
            child2.transform.SetParent(parent.transform);
            child3.transform.SetParent(parent.transform);

            Transform[] children = parent.transform.GetChildren();

            Assert.AreEqual(3, children.Length);
            Assert.AreEqual(child1.transform, children[0]);
            Assert.AreEqual(child2.transform, children[1]);
            Assert.AreEqual(child3.transform, children[2]);
        }

        [UnityTest]
        public IEnumerator CreateParentTest()
        {
            yield return null;

            GameObject target = new GameObject("Target");
            target.transform.position = Vector3.one * Random.value;
            target.transform.rotation = Random.rotation;
            target.transform.localScale = Vector3.one * (Random.value + 0.5f);

            Vector3 targetPosition = target.transform.position;
            Quaternion targetRotation = target.transform.rotation;
            Vector3 targetScale = target.transform.localScale;

            Transform parent = target.transform.CreateParent("Parent");

            Assert.AreEqual("Parent", parent.name);

            Assert.AreEqual(targetPosition, target.transform.position);
            Assert.AreEqual(targetRotation, target.transform.rotation);
            Assert.AreEqual(targetScale.x, target.transform.localScale.x, 0.001f);
            Assert.AreEqual(targetScale.y, target.transform.localScale.y, 0.001f);
            Assert.AreEqual(targetScale.z, target.transform.localScale.z, 0.001f);

            Assert.AreEqual(Vector3.zero, parent.localPosition);
            Assert.AreEqual(Quaternion.identity, parent.localRotation);
            Assert.AreEqual(Vector3.one, parent.localScale);

            Transform newParent = target.transform.CreateParent("New Parent");

            Assert.AreEqual(Vector3.zero, newParent.localPosition);
            Assert.AreEqual(Quaternion.identity, newParent.localRotation);
            Assert.AreEqual(Vector3.one, newParent.localScale);

            Assert.AreEqual(targetPosition, target.transform.position);
            Assert.AreEqual(targetRotation, target.transform.rotation);
            Assert.AreEqual(targetScale.x, target.transform.localScale.x, 0.001f);
            Assert.AreEqual(targetScale.y, target.transform.localScale.y, 0.001f);
            Assert.AreEqual(targetScale.z, target.transform.localScale.z, 0.001f);
        }
    }
}