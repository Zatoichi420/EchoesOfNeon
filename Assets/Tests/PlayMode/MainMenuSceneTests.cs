#if UNITY_EDITOR
using System.Collections;
using EchoesOfNeon.Accessibility;
using EchoesOfNeon.UI;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace EchoesOfNeon.Tests
{
    /// <summary>Drives the real MainMenu scene end to end (no screen reader needed).</summary>
    public class MainMenuSceneTests
    {
        private static Button FindButton(string name)
        {
            foreach (var b in Resources.FindObjectsOfTypeAll<Button>())
                if (b.name == name && b.gameObject.scene.IsValid()) return b;
            return null;
        }

        [UnityTest]
        public IEnumerator Menu_FocusStartsOnStart_OptionsAnnouncesAndBackReturns()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/MainMenu.unity", new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null;

            var a11y = AccessibilityManager.Instance;
            Assert.IsNotNull(a11y);
            Assert.IsNotNull(EventSystem.current.currentSelectedGameObject, "no control has focus when the menu opens");
            StringAssert.Contains("button", a11y.LastAnnouncement ?? "");

            var options = GameObject.Find("OptionsButton") ?? FindButton("Options")?.gameObject;
            Assert.IsNotNull(options, "could not find the Options button");
            options.GetComponent<Button>().onClick.Invoke();
            yield return null; yield return null;

            StringAssert.StartsWith("Options.", a11y.LastAnnouncement);
            StringAssert.Contains("tab", a11y.LastAnnouncement);
            StringAssert.Contains("selected", a11y.LastAnnouncement);

            var relay = Object.FindFirstObjectByType<CancelRelay>();
            Assert.IsNotNull(relay);
            relay.OnCancelRequested();
            yield return null; yield return null;
            StringAssert.StartsWith("Main menu.", a11y.LastAnnouncement);
            Assert.IsNotNull(EventSystem.current.currentSelectedGameObject);
        }

        [UnityTest]
        public IEnumerator Menu_HidesRowsForUnimplementedSettings()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/MainMenu.unity", new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null;
            foreach (var s in Resources.FindObjectsOfTypeAll<Slider>())
            {
                if (!s.gameObject.scene.IsValid()) continue;
                if (s.name == "ScreenShake" || s.name == "AimAssistFriction" || s.name == "AimAssistMagnetism")
                    Assert.IsFalse(s.gameObject.activeSelf, s.name + " should be hidden");
            }
        }

        [UnityTest]
        public IEnumerator Menu_HasNewAudioAndCaptionControls_AndNoGapsLeftByHiddenRows()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/MainMenu.unity", new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null;

            bool mono = false, caps = false, scale = false;
            foreach (var t in Resources.FindObjectsOfTypeAll<Toggle>())
            {
                if (!t.gameObject.scene.IsValid()) continue;
                if (t.name == "MonoAudio") mono = true;
                if (t.name == "SoundCaptions") caps = true;
            }
            foreach (var sl in Resources.FindObjectsOfTypeAll<Slider>())
                if (sl.gameObject.scene.IsValid() && sl.name == "UiScale") scale = true;
            Assert.IsTrue(mono && caps && scale, $"mono={mono} captions={caps} uiScale={scale}");

            // Accessibility tab rows should be evenly spaced after compaction.
            var panel = GameObject.Find("MenuCanvas").transform.Find("OptionsPanel");
            Assert.IsNotNull(panel, "OptionsPanel not found");
            foreach (Transform tab in panel)
            {
                if (!tab.name.ToLower().Contains("accessibility")) continue;
                var ys = new System.Collections.Generic.SortedSet<float>();
                foreach (Transform c in tab)
                    if (c.gameObject.activeSelf && c is RectTransform rt && c.GetComponent<Text>() != null) ys.Add(Mathf.Round(rt.anchoredPosition.y));
                float prev = float.NaN, pitch = float.NaN;
                foreach (var y in ys)
                {
                    if (!float.IsNaN(prev))
                    {
                        float gap = y - prev;
                        if (float.IsNaN(pitch)) pitch = gap;
                        Assert.AreEqual(pitch, gap, 1f, "uneven row spacing - a hidden row left a gap");
                    }
                    prev = y;
                }
            }
        }
    }
}
#endif
