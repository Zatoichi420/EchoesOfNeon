using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EchoesOfNeon.UI
{
    /// <summary>
    /// Real pause: Escape / gamepad Start freezes the game (Time.timeScale 0,
    /// audio paused, gameplay input off), shows Resume / Main Menu / Quit,
    /// announces "Paused" and puts screen-reader focus on Resume. Pressing
    /// Pause again (or activating Resume) restores everything exactly as it
    /// was, including a Neural Dilate slow-motion timescale.
    ///
    /// Built entirely in code and spawned automatically in every scene except
    /// the main menu (see Bootstrap), so no scene needs editing. The three
    /// buttons are ordinary uGUI Buttons plus AccessibleButton, same as the
    /// main menu, so keyboard/gamepad navigation and VoiceOver work the same
    /// way. UNVERIFIED with real VoiceOver as of 2026-10-07.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        public const string MainMenuSceneName = "MainMenu";

        public bool IsPaused { get; private set; }

        private InputManager _input;
        private GameObject _panel;
        private AccessibleButton _resumeAccessible;
        private Button _resumeButton;
        private float _previousTimeScale = 1f;
        private float _previousFixedDeltaTime = 0.02f;
        private CursorLockMode _previousCursorLock;
        private bool _previousCursorVisible;

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
            if (scene.name == MainMenuSceneName) return;
            if (FindFirstObjectByType<PauseMenu>() != null) return;
            new GameObject("PauseMenu").AddComponent<PauseMenu>();
        }

        private void Update()
        {
            // InputManager is a DontDestroyOnLoad singleton that may spawn
            // after us; keep looking until it exists.
            if (_input == null && InputManager.Instance != null)
            {
                _input = InputManager.Instance;
                _input.OnPause += Toggle;
            }
        }

        private void OnDestroy()
        {
            if (_input != null) _input.OnPause -= Toggle;
            // Scene is going away mid-pause: never leave the game frozen.
            if (IsPaused) Resume(announce: false);
        }

        public void Toggle()
        {
            if (IsPaused) Resume(announce: true);
            else Pause();
        }

        public void Pause()
        {
            if (IsPaused) return;
            IsPaused = true;
            PauseState.IsPaused = true;

            _previousTimeScale = Time.timeScale;
            _previousFixedDeltaTime = Time.fixedDeltaTime;
            Time.timeScale = 0f;
            AudioListener.pause = true;
            _input?.SetGameplayInputEnabled(false);

            _previousCursorLock = Cursor.lockState;
            _previousCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            EnsureEventSystem();
            if (_panel == null) BuildPanel();
            _panel.SetActive(true); // buttons register their screen-reader nodes in OnEnable

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(_resumeButton.gameObject);

            var a11y = AccessibilityManager.Instance;
            if (a11y != null)
            {
                // Each button's OnEnable asked for focus; the last one wins,
                // so re-point focus at Resume explicitly. The focus
                // announcer then speaks "Paused. Resume, button." as one
                // sentence (prefix + the focused control).
                a11y.NotifyLayoutChanged(_resumeAccessible.Node);
                a11y.SetFocusPrefix("Paused.");
            }
        }

        public void Resume(bool announce)
        {
            if (!IsPaused) return;
            IsPaused = false;
            PauseState.IsPaused = false;

            if (_panel != null) _panel.SetActive(false);

            Time.timeScale = _previousTimeScale;
            Time.fixedDeltaTime = _previousFixedDeltaTime;
            AudioListener.pause = false;
            _input?.SetGameplayInputEnabled(true);

            Cursor.lockState = _previousCursorLock;
            Cursor.visible = _previousCursorVisible;

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);

            if (announce) AccessibilityManager.Instance?.Announce("Resumed.");
        }

        private void ReturnToMainMenu()
        {
            Resume(announce: false);
            Time.timeScale = 1f;
            SceneManager.LoadScene(MainMenuSceneName);
        }

        private void QuitGame()
        {
            AccessibilityManager.Instance?.Announce("Quitting game.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // --- UI construction ---

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            var module = go.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }

        private void BuildPanel()
        {
            var canvasGO = new GameObject("PauseCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(transform, false);
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            // Build inactive so the buttons' OnEnable (screen-reader nodes)
            // runs when Pause() activates the panel, not mid-construction.
            _panel = new GameObject("PausePanel", typeof(RectTransform), typeof(Image));
            _panel.SetActive(false);
            _panel.transform.SetParent(canvasGO.transform, false);
            var panelRect = (RectTransform)_panel.transform;
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = panelRect.offsetMax = Vector2.zero;
            _panel.GetComponent<Image>().color = new Color(0.03f, 0.035f, 0.045f, 0.92f);

            var accessiblePanel = _panel.AddComponent<AccessiblePanel>();
            accessiblePanel.Title = "Paused";
            accessiblePanel.OnDismiss = () => Resume(announce: true);
            AddTitle(_panel.transform, "Paused");
            var resume = AddButton("Resume", 60f, () => Resume(announce: true));
            AddButton("Main menu", -60f, ReturnToMainMenu);
            AddButton("Quit", -180f, QuitGame);

            _resumeButton = resume;
            _resumeAccessible = resume.GetComponent<AccessibleButton>();
        }

        private static Font BodyFont => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        private void AddTitle(Transform parent, string text)
        {
            var go = new GameObject("Title", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, 230f);
            rt.sizeDelta = new Vector2(800f, 100f);
            var t = go.GetComponent<Text>();
            t.font = BodyFont;
            t.fontSize = 64;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = new Color(0.15f, 0.85f, 0.95f);
            t.text = text;
        }

        private Button AddButton(string label, float y, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_panel.transform, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = new Vector2(520f, 90f);

            go.GetComponent<Image>().color = new Color(0.11f, 0.135f, 0.16f);
            var button = go.GetComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(0.7f, 0.95f, 1f);
            colors.selectedColor = new Color(0.4f, 0.9f, 1f); // visible keyboard/gamepad focus
            colors.pressedColor = new Color(0.15f, 0.85f, 0.95f);
            button.colors = colors;
            button.onClick.AddListener(onClick);

            var textGO = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGO.transform.SetParent(go.transform, false);
            var textRect = (RectTransform)textGO.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = textRect.offsetMax = Vector2.zero;
            var text = textGO.GetComponent<Text>();
            text.font = BodyFont;
            text.fontSize = 40;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.93f, 0.96f, 0.97f);
            text.text = label;

            // AccessibleButton reads its label from the child Text above.
            go.AddComponent<AccessibleButton>();
            return button;
        }
    }
}
