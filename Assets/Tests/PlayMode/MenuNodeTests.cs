using System.Collections;
using EchoesOfNeon.Accessibility;
using EchoesOfNeon.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace EchoesOfNeon.Tests
{
    public class MenuNodeTests
    {
        private GameObject _managerGO;
        private GameObject _canvasGO;

        [SetUp]
        public void SetUp()
        {
            _managerGO = new GameObject("AccessibilityManager", typeof(AccessibilityManager));
            _canvasGO = new GameObject("Canvas", typeof(Canvas));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_canvasGO);
            Object.DestroyImmediate(_managerGO);
        }

        private GameObject NewButton(string label)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_canvasGO.transform, false);
            var textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(go.transform, false);
            textGO.GetComponent<Text>().text = label;
            go.AddComponent<AccessibleButton>();
            return go;
        }

        [UnityTest]
        public IEnumerator Button_RegistersNodeWhenEnabled_AndRemovesItWhenDisabled()
        {
            yield return null;
            var go = NewButton("Start Game");
            var accessible = go.GetComponent<AccessibleButton>();
            Assert.IsNotNull(accessible.Node, "enabled button has no screen-reader node");
            Assert.AreEqual("Start Game", accessible.Node.label);
            Assert.AreEqual(UnityEngine.Accessibility.AccessibilityRole.Button, accessible.Node.role);

            go.SetActive(false);
            Assert.IsNull(accessible.Node, "node should be removed when the button is disabled");
            go.SetActive(true);
            Assert.IsNotNull(accessible.Node, "node should come back when re-enabled");
        }

        [UnityTest]
        public IEnumerator Panel_DisablingParent_RemovesEveryChildNode()
        {
            yield return null;
            var panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(_canvasGO.transform, false);
            var a = NewButton("A"); a.transform.SetParent(panel.transform, false);
            var b = NewButton("B"); b.transform.SetParent(panel.transform, false);
            Assert.IsNotNull(a.GetComponent<AccessibleButton>().Node);
            Assert.IsNotNull(b.GetComponent<AccessibleButton>().Node);

            panel.SetActive(false);
            Assert.IsNull(a.GetComponent<AccessibleButton>().Node);
            Assert.IsNull(b.GetComponent<AccessibleButton>().Node);
        }
    }
}
