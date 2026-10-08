using System.Collections;
using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using EchoesOfNeon.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace EchoesOfNeon.Tests
{
    public class AudioAndCaptionTests
    {
        private GameObject _a11y, _audio, _scratch;

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteKey("EchoesOfNeon.AudioSettings");
            PlayerPrefs.DeleteKey("EchoesOfNeon.AccessibilitySettings");
            _a11y = new GameObject("A11y", typeof(AccessibilityManager));
            _audio = new GameObject("Audio", typeof(AudioSettingsManager));
            _scratch = new GameObject("Scratch");
            CaptionService.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_scratch);
            Object.DestroyImmediate(_audio);
            Object.DestroyImmediate(_a11y);
            PlayerPrefs.DeleteKey("EchoesOfNeon.AudioSettings");
            PlayerPrefs.DeleteKey("EchoesOfNeon.AccessibilitySettings");
        }

        [UnityTest]
        public IEnumerator SfxAndDialogueSources_FollowTheirOwnVolumeTimesMaster()
        {
            yield return null;
            var sfx = _scratch.AddComponent<AudioSource>();
            var dlg = new GameObject("d").AddComponent<AudioSource>();
            dlg.transform.SetParent(_scratch.transform);
            AudioRouting.Bind(sfx, AudioCategory.Sfx);
            AudioRouting.Bind(dlg, AudioCategory.Dialogue);

            var d = AudioSettingsManager.Instance.Data;
            d.masterVolume = 0.5f; d.sfxVolume = 0.4f; d.dialogueVolume = 1f;
            AudioSettingsManager.Instance.ApplySettings(d);
            Assert.AreEqual(0.2f, sfx.volume, 0.001f);
            Assert.AreEqual(0.5f, dlg.volume, 0.001f);

            // Code that sets its own base volume later is respected.
            sfx.volume = 0.5f;
            d.sfxVolume = 1f; d.masterVolume = 1f;
            AudioSettingsManager.Instance.ApplySettings(d);
            Assert.AreEqual(0.5f, sfx.volume, 0.001f);
        }

        [UnityTest]
        public IEnumerator MonoAudio_SwitchesTheSpeakerMode()
        {
            yield return null;
            var d = AudioSettingsManager.Instance.Data;
            d.monoAudio = true;
            AudioSettingsManager.Instance.ApplySettings(d);
            Assert.AreEqual(AudioSpeakerMode.Mono, AudioSettings.GetConfiguration().speakerMode);
            d.monoAudio = false;
            AudioSettingsManager.Instance.ApplySettings(d);
            Assert.AreEqual(AudioSpeakerMode.Stereo, AudioSettings.GetConfiguration().speakerMode);
        }

        private CaptionDisplay Display()
        {
            var existing = Object.FindFirstObjectByType<CaptionDisplay>();
            return existing != null ? existing : new GameObject("CaptionDisplay").AddComponent<CaptionDisplay>();
        }

        [UnityTest]
        public IEnumerator Subtitles_ShowSpokenBarks_OnlyWhenEnabled()
        {
            yield return null;
            var display = Display();
            yield return null;
            int before = display.CurrentLines().Count;

            AccessibilityManager.Instance.Settings.subtitlesEnabled = false;
            CaptionService.SpokenLine("Guard", "bark_alert_01", Vector3.forward * 5f);
            Assert.AreEqual(before, display.CurrentLines().Count, "subtitles off should show nothing");

            AccessibilityManager.Instance.Settings.subtitlesEnabled = true;
            CaptionService.SpokenLine("Guard", "bark_alert_01", Vector3.forward * 5f);
            var lines = display.CurrentLines();
            StringAssert.Contains("Contact! Get eyes on him!", lines[lines.Count - 1]);
        }

        [UnityTest]
        public IEnumerator SoundCaptions_AreOffByDefault_AndRateLimitedWhenOn()
        {
            yield return null;
            var display = Display();
            yield return null;
            int before = display.CurrentLines().Count;
            var evt = new AcousticEvent(Vector3.right * 4f, AcousticEventType.Gunfire, 20f, null);

            CaptionService.SoundEvent(evt);
            Assert.AreEqual(before, display.CurrentLines().Count);

            AccessibilityManager.Instance.Settings.soundCaptionsEnabled = true;
            CaptionService.SoundEvent(evt);
            CaptionService.SoundEvent(evt); // within 1.5 s: dropped
            Assert.AreEqual(before + 1, display.CurrentLines().Count);
        }

        [UnityTest]
        public IEnumerator UiScale_ShrinksTheReferenceResolution()
        {
            yield return null;
            var go = new GameObject("c", typeof(Canvas), typeof(CanvasScaler));
            go.transform.SetParent(_scratch.transform);
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            go.AddComponent<UiScaleApplier>();
            yield return null;
            Assert.AreEqual(1920f, scaler.referenceResolution.x, 0.01f);

            var s = AccessibilityManager.Instance.Settings;
            s.uiScale = 1.5f;
            AccessibilityManager.Instance.ApplySettings(s);
            Assert.AreEqual(1280f, scaler.referenceResolution.x, 0.01f);
        }
    }
}
