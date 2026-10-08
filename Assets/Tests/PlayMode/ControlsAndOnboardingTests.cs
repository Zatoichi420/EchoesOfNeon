using System.Collections;
using System.Reflection;
using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using EchoesOfNeon.Optics;
using EchoesOfNeon.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EchoesOfNeon.Tests
{
    public class ControlsAndOnboardingTests
    {
        private GameObject _a11y, _input, _extra;

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteKey("EchoesOfNeon.AccessibilitySettings");
            PlayerPrefs.DeleteKey("EchoesOfNeon.InputRebinds");
            _a11y = new GameObject("A11y", typeof(AccessibilityManager));
            _input = new GameObject("Input", typeof(InputManager));
            _extra = new GameObject("Extra");
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            Object.DestroyImmediate(_extra);
            Object.DestroyImmediate(_input);
            Object.DestroyImmediate(_a11y);
            PlayerPrefs.DeleteKey("EchoesOfNeon.AccessibilitySettings");
        }

        private static void Call(object target, string method) =>
            target.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance, null, System.Type.EmptyTypes, null).Invoke(target, null);

        [UnityTest]
        public IEnumerator MovementKeys_ShowTheirBindings_AndCanStartRebinding()
        {
            yield return null;
            var im = InputManager.Instance;
            var kb = InputManager.DeviceKind.KeyboardMouse;
            Assert.AreEqual("W", im.GetBindingDisplayString("Move:Up", kb));
            Assert.AreEqual("S", im.GetBindingDisplayString("Move:Down", kb));
            Assert.AreEqual("A", im.GetBindingDisplayString("Move:Left", kb));
            Assert.AreEqual("D", im.GetBindingDisplayString("Move:Right", kb));
            Assert.AreEqual("F1", im.GetBindingDisplayString("Hint", kb));

            var op = im.StartRebind("Move:Up", () => { });
            Assert.IsNotNull(op, "rebinding a movement key should start");
            op.Cancel(); op.Dispose();

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("No keyboard part"));
            Assert.IsNull(im.StartRebind("Move:Sideways", () => { }));
        }

        [UnityTest]
        public IEnumerator HintKey_SpeaksTheCurrentControls()
        {
            yield return null;
            Call(AccessibilityManager.Instance, "HandleHintPressed");
            var said = AccessibilityManager.Instance.LastAnnouncement;
            StringAssert.StartsWith("Controls.", said);
            StringAssert.Contains("Move forward: W.", said);
            StringAssert.Contains("Pause: Escape.", said);
        }

        [UnityTest]
        public IEnumerator ToggleDilate_PressTwiceToStop_AndKeyUpIsIgnored()
        {
            yield return null;
            var suite = _extra.AddComponent<OculusSensorySuite>();
            yield return null; // Start() runs
            AccessibilityManager.Instance.Settings.toggleDilate = true;

            Call(suite, "HandleDilatePressed");
            Assert.IsTrue(suite.IsDilating);
            Call(suite, "HandleDilateReleased");
            Assert.IsTrue(suite.IsDilating, "key-up must not end a toggled dilate");
            Call(suite, "HandleDilatePressed");
            Assert.IsFalse(suite.IsDilating, "second press should stop it");

            AccessibilityManager.Instance.Settings.toggleDilate = false;
            Call(suite, "HandleDilatePressed");
            Call(suite, "HandleDilateReleased");
            Assert.IsFalse(suite.IsDilating, "hold mode: release stops it");
        }

        [UnityTest]
        public IEnumerator SideScrollSprint_FollowsTheToggleSetting()
        {
            yield return null;
            var go = new GameObject("P", typeof(CharacterController));
            go.transform.SetParent(_extra.transform);
            var player = go.AddComponent<EchoesOfNeon.SideScroll.SideScrollPlayer>();
            yield return null;
            var sprint = typeof(EchoesOfNeon.SideScroll.SideScrollPlayer).GetField("_sprint", BindingFlags.NonPublic | BindingFlags.Instance);

            AccessibilityManager.Instance.Settings.toggleSprint = true;
            Call(AccessibilityManager.Instance, "HandleSprintPressed");
            Call(AccessibilityManager.Instance, "HandleSprintReleased");
            Assert.IsTrue((bool)sprint.GetValue(player), "toggle sprint should stay on after release");
            Call(AccessibilityManager.Instance, "HandleSprintPressed");
            Assert.IsFalse((bool)sprint.GetValue(player));
        }

        [UnityTest]
        public IEnumerator Difficulty_ScalesDamageTaken()
        {
            yield return null;
            var go = new GameObject("P", typeof(CharacterController));
            go.transform.SetParent(_extra.transform);
            var player = go.AddComponent<EchoesOfNeon.SideScroll.SideScrollPlayer>();
            yield return null;
            float start = player.Health;

            AccessibilityManager.Instance.Settings.difficulty = 0; // Story: 0.4x
            player.TakeHit(10f, Vector3.zero, true, null, null);
            Assert.AreEqual(4f, start - player.Health, 0.01f);
        }

        [Test]
        public void FirstRun_IsRememberedAcrossCalls()
        {
            const string key = "EchoesOfNeon.Test.FirstRun";
            PlayerPrefs.DeleteKey(key);
            Assert.IsFalse(FirstRunPrompt.Seen(key));
            FirstRunPrompt.MarkSeen(key);
            Assert.IsTrue(FirstRunPrompt.Seen(key));
            PlayerPrefs.DeleteKey(key);
            StringAssert.Contains("F1", FirstRunPrompt.GameplayMessage);
            StringAssert.Contains("Accessibility", FirstRunPrompt.MenuMessage);
        }
    }
}
