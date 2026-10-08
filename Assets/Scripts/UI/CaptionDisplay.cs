using System.Collections.Generic;
using EchoesOfNeon.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EchoesOfNeon.UI
{
    /// <summary>Draws captions at the bottom of the screen: up to three lines,
    /// each fading after a few seconds. White on a dark panel for contrast.
    /// Spawned automatically in every scene except the main menu.</summary>
    public class CaptionDisplay : MonoBehaviour
    {
        private const int MaxLines = 3;
        private const float LineSeconds = 4.5f;

        private struct Line { public string Text; public float ExpiresAt; }

        private readonly List<Line> _lines = new List<Line>();
        private Text _text;
        private GameObject _panel;
        private bool _subscribedAcoustic;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Spawn(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Spawn(scene);

        private static void Spawn(Scene scene)
        {
            if (scene.name == PauseMenu.MainMenuSceneName) return;
            if (FindFirstObjectByType<CaptionDisplay>() != null) return;
            new GameObject("CaptionDisplay").AddComponent<CaptionDisplay>();
        }

        private void Awake() => BuildUi();

        private void OnEnable() => CaptionService.OnCaption += Add;

        private void OnDisable()
        {
            CaptionService.OnCaption -= Add;
            if (_subscribedAcoustic && AcousticEventSystem.Instance != null)
                AcousticEventSystem.Instance.OnAcousticEvent -= CaptionService.SoundEvent;
            _subscribedAcoustic = false;
        }

        private void Update()
        {
            if (!_subscribedAcoustic && AcousticEventSystem.Instance != null)
            {
                AcousticEventSystem.Instance.OnAcousticEvent += CaptionService.SoundEvent;
                _subscribedAcoustic = true;
            }

            if (_lines.Count == 0) return;
            _lines.RemoveAll(l => Time.unscaledTime >= l.ExpiresAt);
            Refresh();
        }

        public void Add(string caption)
        {
            if (string.IsNullOrEmpty(caption)) return;
            _lines.Add(new Line { Text = caption, ExpiresAt = Time.unscaledTime + LineSeconds });
            while (_lines.Count > MaxLines) _lines.RemoveAt(0);
            Refresh();
        }

        public IReadOnlyList<string> CurrentLines()
        {
            var result = new List<string>();
            foreach (var l in _lines) result.Add(l.Text);
            return result;
        }

        private void Refresh()
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < _lines.Count; i++)
            {
                if (i > 0) sb.Append('\n');
                sb.Append(_lines[i].Text);
            }
            _text.text = sb.ToString();
            _panel.SetActive(_lines.Count > 0);
        }

        private void BuildUi()
        {
            var canvasGO = new GameObject("CaptionCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasGO.transform.SetParent(transform, false);
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 900; // under the pause menu (1000)
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGO.AddComponent<UiScaleApplier>();

            _panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            _panel.transform.SetParent(canvasGO.transform, false);
            var rt = (RectTransform)_panel.transform;
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 60f);
            rt.sizeDelta = new Vector2(1500f, 190f);
            _panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.8f);

            var textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(_panel.transform, false);
            var tr = (RectTransform)textGO.transform;
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(24f, 10f); tr.offsetMax = new Vector2(-24f, -10f);
            _text = textGO.GetComponent<Text>();
            _text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _text.fontSize = 44;
            _text.alignment = TextAnchor.LowerCenter;
            _text.color = Color.white;
            _panel.SetActive(false);
        }
    }
}
