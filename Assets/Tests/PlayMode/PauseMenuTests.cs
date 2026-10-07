using System.Collections;
using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using EchoesOfNeon.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EchoesOfNeon.Tests
{
    public class PauseMenuTests
    {
        private GameObject _a11y, _input, _menu;

        [SetUp]
        public void SetUp()
        {
            _a11y = new GameObject("AccessibilityManager", typeof(AccessibilityManager));
            _input = new GameObject("InputManager", typeof(InputManager));
            Time.timeScale = 1f;
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            PauseState.IsPaused = false;
            if (_menu != null) Object.DestroyImmediate(_menu);
            Object.DestroyImmediate(_input);
            Object.DestroyImmediate(_a11y);
            foreach (var es in Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None))
                Object.DestroyImmediate(es.gameObject);
        }

        private PauseMenu NewMenu()
        {
            _menu = new GameObject("PauseMenu", typeof(PauseMenu));
            return _menu.GetComponent<PauseMenu>();
        }

        [UnityTest]
        public IEnumerator Pause_FreezesTime_AndResumeRestoresIt()
        {
            yield return null;
            var menu = NewMenu();
            menu.Pause();
            Assert.AreEqual(0f, Time.timeScale);
            Assert.IsTrue(PauseState.IsPaused);
            Assert.IsTrue(AudioListener.pause);

            menu.Resume(announce: false);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.IsFalse(PauseState.IsPaused);
            Assert.IsFalse(AudioListener.pause);
        }

        [UnityTest]
        public IEnumerator Resume_RestoresDilatedTimescale_NotJustOne()
        {
            yield return null;
            Time.timeScale = 0.3f; // as if Neural Dilate were held
            var menu = NewMenu();
            menu.Pause();
            menu.Resume(announce: false);
            Assert.AreEqual(0.3f, Time.timeScale, 0.0001f);
        }

        [UnityTest]
        public IEnumerator Pause_ShowsThreeButtons_WithScreenReaderNodes()
        {
            yield return null;
            var menu = NewMenu();
            menu.Pause();
            var buttons = menu.GetComponentsInChildren<AccessibleButton>();
            Assert.AreEqual(3, buttons.Length);
            foreach (var b in buttons) Assert.IsNotNull(b.Node);

            menu.Resume(announce: false);
            foreach (var b in buttons) Assert.IsNull(b.Node, "nodes must be removed when the pause menu closes");
        }

        [UnityTest]
        public IEnumerator DestroyingMenuWhilePaused_UnfreezesTheGame()
        {
            yield return null;
            var menu = NewMenu();
            menu.Pause();
            Object.Destroy(_menu);
            yield return null;
            Assert.AreEqual(1f, Time.timeScale);
            Assert.IsFalse(PauseState.IsPaused);
        }
    }
}
