using System.Collections;
using EchoesOfNeon.Accessibility;
using EchoesOfNeon.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace EchoesOfNeon.Tests
{
    public class FocusSpeechTests
    {
        private GameObject _manager, _canvas, _events;

        [SetUp]
        public void SetUp()
        {
            _manager = new GameObject("AccessibilityManager", typeof(AccessibilityManager));
            _canvas = new GameObject("Canvas", typeof(Canvas));
            _events = new GameObject("EventSystem", typeof(EventSystem));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_events);
            Object.DestroyImmediate(_canvas);
            Object.DestroyImmediate(_manager);
        }

        private GameObject Button(string label)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_canvas.transform, false);
            var t = new GameObject("Text", typeof(RectTransform), typeof(Text));
            t.transform.SetParent(go.transform, false);
            t.GetComponent<Text>().text = label;
            go.AddComponent<AccessibleButton>();
            return go;
        }

        [UnityTest]
        public IEnumerator Selecting_AControl_AnnouncesItsDescription()
        {
            yield return null;
            var b = Button("Start Game");
            EventSystem.current.SetSelectedGameObject(b);
            yield return null;
            Assert.AreEqual("Start Game, button", AccessibilityManager.Instance.LastAnnouncement);
        }

        [UnityTest]
        public IEnumerator FocusPrefix_IsSpokenOnce_ThenCleared()
        {
            yield return null;
            var a = Button("Resume"); var b = Button("Quit");
            AccessibilityManager.Instance.SetFocusPrefix("Paused.");
            EventSystem.current.SetSelectedGameObject(a);
            yield return null;
            Assert.AreEqual("Paused. Resume, button", AccessibilityManager.Instance.LastAnnouncement);
            EventSystem.current.SetSelectedGameObject(b);
            yield return null;
            Assert.AreEqual("Quit, button", AccessibilityManager.Instance.LastAnnouncement);
        }

        [UnityTest]
        public IEnumerator TabButton_SaysSelected_AndPanelDismissRunsBack()
        {
            yield return null;
            var tab = Button("Audio");
            var acc = tab.GetComponent<AccessibleButton>();
            acc.SetRole(UnityEngine.Accessibility.AccessibilityRole.TabButton);
            acc.SetSelected(true);
            Assert.AreEqual("Audio, tab, selected", AccessibleDescription.Describe(tab));
            acc.SetSelected(false);
            Assert.AreEqual("Audio, tab", AccessibleDescription.Describe(tab));

            bool dismissed = false;
            var panelGO = new GameObject("Options", typeof(RectTransform));
            panelGO.transform.SetParent(_canvas.transform, false);
            var panel = panelGO.AddComponent<AccessiblePanel>();
            panel.Title = "Options";
            panel.OnDismiss = () => dismissed = true;
            Assert.IsNotNull(panel.Node);
            Assert.IsTrue(panel.Dismiss());
            Assert.IsTrue(dismissed);
        }

        [UnityTest]
        public IEnumerator Label_PrefersTextMeshPro_AndRepeatReplaysLast()
        {
            yield return null;
            var go = new GameObject("fallbackName", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_canvas.transform, false);
            var tmpGO = new GameObject("TMP", typeof(RectTransform));
            tmpGO.transform.SetParent(go.transform, false);
            var tmp = tmpGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "Quit Game";
            Assert.AreEqual("Quit Game", AccessibleLabel.Resolve(go.transform));

            AccessibilityManager.Instance.Announce("Objective updated.");
            AccessibilityManager.Instance.RepeatLastAnnouncement();
            Assert.AreEqual("Objective updated.", AccessibilityManager.Instance.LastAnnouncement);
        }
    }
}
