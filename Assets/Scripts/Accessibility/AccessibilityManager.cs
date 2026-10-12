using System;
using EchoesOfNeon.Core;
using UnityEngine;
using UnityEngine.Accessibility;

namespace EchoesOfNeon.Accessibility
{
    public enum ColorblindMode { None, Protanopia, Deuteranopia, Tritanopia, HighContrastMonochrome }

    /// <summary>Plain data - JSON-serializable settings blob, persisted via
    /// PlayerPrefs. Holding a value here does not, by itself, make anything
    /// happen: colorblind palettes and outline shaders (Phase 6) and the
    /// Neural Dilate timescale (Phase 4, Oculus Sensory Suite) read these
    /// values and apply them - this class is the single source of truth for
    /// the settings, not the renderer/timescale code itself.</summary>
    [Serializable]
    public class AccessibilitySettings
    {
        public ColorblindMode colorblindMode = ColorblindMode.None;
        public bool highContrastOutlines = false;

        public float aimAssistFriction = 0.3f;   // 0 = off, 1 = max slowdown crossing a target
        public float aimAssistMagnetism = 0.3f;  // 0 = off, 1 = max soft snap-to-target

        public bool singleStickMode = false;
        public bool toggleAim = false;
        public bool toggleSprint = false;
        public bool toggleCrouch = false;
        public float lookSensitivity = 1f;

        public float neuralDilateTimescale = 0.3f; // Time.timeScale while Neural Dilate is held

        public bool cameraBobEnabled = true;
        public float screenShakeScale = 1f;
        public bool reduceMotion = false;

        // Stubs added 2026-09-28 alongside the menu's Accessibility screen,
        // ahead of the systems that would actually use them (cutscene audio
        // description track, subtitle rendering - see
        // echoes-of-neon-audio-and-accessibility-implementation memory /
        // this session's MEMORY.md entry). Wired to real toggles now so the
        // menu doesn't need revisiting once those systems land; both no-op
        // until then.
        public bool audioDescriptionEnabled = false;
        public bool subtitlesEnabled = true;      // on-screen text for spoken lines (enemy barks)
        public bool soundCaptionsEnabled = false; // on-screen text for non-speech sounds: "[Gunfire, to your left]"
        public float uiScale = 1f;

        public bool toggleDilate = false;         // Neural Dilate: press to start, press again to stop
        public int difficulty = 2;                // 0 Story, 1 Easy, 2 Normal, 3 Hard (DifficultyRules)
        public int timingAssist = 0;              // 0 Standard, 1 Relaxed, 2 Generous (DifficultyRules)                // 1 - 1.5: size of captions and the pause menu
    }

    /// <summary>
    /// Phase 2: the accessibility settings hub, plus two responsibilities
    /// that only belong here:
    /// 1. Toggle-vs-hold translation for Aim/Sprint/Crouch, layered on top of
    ///    InputManager's raw press/release events (InputManager deliberately
    ///    stays unaware of this preference - see its own header comment).
    /// 2. The native screen-reader hookup (UnityEngine.Accessibility,
    ///    UI-Automation-based on Windows - the reason this project targets
    ///    Unity 6.3+ instead of the 6.0 LTS line). Announce(string) covers
    ///    one-off toasts. CreateMenuNode/RemoveMenuNode/NotifyLayoutChanged
    ///    (below) are the Phase 5/6 groundwork for real menu buttons -
    ///    Assets/Scripts/UI/AccessibleButton.cs, AccessibleToggle.cs, and
    ///    AccessibleSlider.cs are the components that actually call them,
    ///    each pairing with a standard UGUI Button/Toggle/Slider. No real
    ///    menu Canvas exists in the project yet to drop these onto - this is
    ///    the wiring, not a built menu. Unverified in the Editor as of
    ///    2026-09-28 (written outside Unity); compile and test against
    ///    VoiceOver before relying on it.
    /// </summary>
    public class AccessibilityManager : MonoBehaviour
    {
        public static AccessibilityManager Instance { get; private set; }

        /// <summary>Raised once the manager exists and its screen-reader
        /// hierarchy is ready. Menu controls that enabled earlier (scene
        /// object order is not guaranteed) register their nodes when this
        /// fires instead of silently never registering.</summary>
        public static event Action Ready;

        public AccessibilitySettings Settings { get; private set; } = new AccessibilitySettings();
        public event Action<AccessibilitySettings> OnSettingsChanged;

        public bool IsAiming { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool IsCrouching { get; private set; }

        public event Action<bool> OnAimStateChanged;
        public event Action<bool> OnSprintStateChanged;
        public event Action<bool> OnCrouchStateChanged;

        private const string SettingsPrefsKey = "EchoesOfNeon.AccessibilitySettings";

        /// <summary>Fields of AccessibilitySettings that no gameplay system
        /// reads yet. MenuController hides the matching controls so players
        /// are never offered a switch that does nothing. When you implement
        /// one, delete its name here - the EditMode test
        /// SettingsAuditTests fails if a field is neither consumed in code
        /// nor listed here, and also if a listed field turns out to be
        /// consumed.</summary>
        public static readonly string[] UnimplementedSettings =
        {
            nameof(AccessibilitySettings.screenShakeScale),      // no screen shake exists yet
            nameof(AccessibilitySettings.aimAssistFriction),     // no aim-assist system yet
            nameof(AccessibilitySettings.aimAssistMagnetism),
            nameof(AccessibilitySettings.audioDescriptionEnabled), // needs cutscenes to exist first
        };

        private AccessibilityHierarchy _hierarchy;
        private InputManager _input;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
            SetupScreenReaderHierarchy();
            if (GetComponent<EchoesOfNeon.UI.MenuFocusAnnouncer>() == null)
                gameObject.AddComponent<EchoesOfNeon.UI.MenuFocusAnnouncer>();
            Ready?.Invoke();
        }

        private void Start()
        {
            // Start(), not Awake()/OnEnable() - guarantees InputManager has
            // already run its own Awake() and set Instance, regardless of
            // GameObject ordering in the scene.
            _input = InputManager.Instance;
            if (_input == null)
            {
                Debug.LogWarning("[AccessibilityManager] No InputManager found in scene - toggle/hold translation will not function.");
                return;
            }
            _input.OnAimPressed += HandleAimPressed;
            _input.OnAimReleased += HandleAimReleased;
            _input.OnSprintPressed += HandleSprintPressed;
            _input.OnSprintReleased += HandleSprintReleased;
            _input.OnCrouchPressed += HandleCrouchPressed;
            _input.OnCrouchReleased += HandleCrouchReleased;
            _input.OnStatusPressed += HandleStatusPressed;
            _input.OnRepeatPressed += RepeatLastAnnouncement;
            _input.OnHintPressed += HandleHintPressed;
        }

        private void HandleHintPressed()
        {
            var device = _input.CurrentDevice;
            Announce(ControlsGuide.Compose(action => _input.GetBindingDisplayString(action, device)));
        }

        // Tactical scenes register providers with StatusRegistry; the
        // side-scroller answers the Status key itself and registers none.
        private void HandleStatusPressed()
        {
            if (StatusRegistry.Count == 0) return;
            Announce(StatusRegistry.Compose());
        }

        private void OnDestroy()
        {
            if (_input == null) return;
            _input.OnAimPressed -= HandleAimPressed;
            _input.OnAimReleased -= HandleAimReleased;
            _input.OnSprintPressed -= HandleSprintPressed;
            _input.OnSprintReleased -= HandleSprintReleased;
            _input.OnCrouchPressed -= HandleCrouchPressed;
            _input.OnCrouchReleased -= HandleCrouchReleased;
            _input.OnStatusPressed -= HandleStatusPressed;
            _input.OnRepeatPressed -= RepeatLastAnnouncement;
            _input.OnHintPressed -= HandleHintPressed;
        }

        // --- Toggle-vs-hold translation ---

        private void HandleAimPressed() => SetAiming(Settings.toggleAim ? !IsAiming : true);
        private void HandleAimReleased() { if (!Settings.toggleAim) SetAiming(false); }
        private void SetAiming(bool value)
        {
            if (IsAiming == value) return;
            IsAiming = value;
            if (Settings.toggleAim) Announce("Aim " + (value ? "on" : "off")); // hold-to-toggle state is otherwise invisible
            OnAimStateChanged?.Invoke(IsAiming);
        }

        private void HandleSprintPressed() => SetSprinting(Settings.toggleSprint ? !IsSprinting : true);
        private void HandleSprintReleased() { if (!Settings.toggleSprint) SetSprinting(false); }
        private void SetSprinting(bool value)
        {
            if (IsSprinting == value) return;
            IsSprinting = value;
            if (Settings.toggleSprint) Announce("Sprint " + (value ? "on" : "off")); // hold-to-toggle state is otherwise invisible
            OnSprintStateChanged?.Invoke(IsSprinting);
        }

        private void HandleCrouchPressed() => SetCrouching(Settings.toggleCrouch ? !IsCrouching : true);
        private void HandleCrouchReleased() { if (!Settings.toggleCrouch) SetCrouching(false); }
        private void SetCrouching(bool value)
        {
            if (IsCrouching == value) return;
            IsCrouching = value;
            if (Settings.toggleCrouch) Announce("Crouch " + (value ? "on" : "off")); // hold-to-toggle state is otherwise invisible
            OnCrouchStateChanged?.Invoke(IsCrouching);
        }

        // --- Settings persistence ---

        public void LoadSettings()
        {
            if (!PlayerPrefs.HasKey(SettingsPrefsKey)) return;
            try
            {
                JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(SettingsPrefsKey), Settings);
            }
            catch (System.ArgumentException e)
            {
                // Corrupt saved settings used to throw in Awake and leave the manager half-built.
                Debug.LogWarning($"[AccessibilityManager] Saved settings unreadable, using defaults. {e.Message}");
                Settings = new AccessibilitySettings();
                Announce("Saved settings could not be read. Defaults restored.");
            }
        }

        public void ApplySettings(AccessibilitySettings newSettings)
        {
            Settings = newSettings ?? new AccessibilitySettings();
            PlayerPrefs.SetString(SettingsPrefsKey, JsonUtility.ToJson(Settings));
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke(Settings);
        }

        // --- Native screen-reader hookup ---

        private void SetupScreenReaderHierarchy()
        {
            _hierarchy = new AccessibilityHierarchy();
            AssistiveSupport.activeHierarchy = _hierarchy;
        }

        public bool IsScreenReaderActive => AssistiveSupport.isScreenReaderEnabled;

        /// <summary>One-off spoken announcement (e.g. "Game paused",
        /// "Objective updated") - not tied to a specific focused UI element.
        /// No-ops quietly if no screen reader is active, so callers never
        /// need to check IsScreenReaderActive themselves first.</summary>
        public void Announce(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            LastAnnouncement = text;
            Speak(text);
        }

        /// <summary>The most recent thing Announce() was asked to say, so the
        /// Repeat key can say it again if the player missed it.</summary>
        public string LastAnnouncement { get; private set; }

        public void RepeatLastAnnouncement()
        {
            Speak(string.IsNullOrEmpty(LastAnnouncement) ? "Nothing to repeat." : LastAnnouncement);
        }

        // Screen reader first; otherwise the OS voice (SpeechFallback) so a
        // player running without VoiceOver still hears the game.
        private static void Speak(string text)
        {
            if (AssistiveSupport.isScreenReaderEnabled)
                AssistiveSupport.notificationDispatcher.SendAnnouncement(text);
            else
                SpeechFallback.Speak(text);
        }

        private string _focusPrefix;

        /// <summary>Text to speak before the next focus announcement (e.g. a
        /// panel title: "Options. Controller, tab, selected"), so a panel
        /// change and its first control are one sentence instead of one
        /// interrupting the other.</summary>
        public void SetFocusPrefix(string prefix) => _focusPrefix = prefix;

        public string ConsumeFocusPrefix()
        {
            var p = _focusPrefix;
            _focusPrefix = null;
            return p;
        }

        /// <summary>Registers one focusable menu element (button, toggle,
        /// slider, ...) in the active AccessibilityHierarchy and points its
        /// on-screen frame at the given RectTransform, so the OS screen
        /// reader can hit-test and focus it. Prefer going through
        /// AccessibleButton/AccessibleToggle/AccessibleSlider rather than
        /// calling this directly - they also handle state/value updates and
        /// cleanup on disable. Returns null if called before a screen
        /// reader hierarchy exists (e.g. from Awake() on the very first
        /// frame) - callers should call this from OnEnable(), by which
        /// point AccessibilityManager.Awake() has already run.</summary>
        public AccessibilityNode CreateMenuNode(string label, AccessibilityRole role, RectTransform rect, AccessibilityNode parent = null)
        {
            if (_hierarchy == null) return null;

            var node = _hierarchy.AddNode(label: label, parent: parent);
            node.role = role;
            node.isActive = true;
            if (rect != null)
            {
                // Unity does not read Canvas/RectTransform layout on its
                // own - frameGetter is the only way the screen reader learns
                // where this element actually is on screen, and it must
                // stay a live delegate (not a one-time Rect) so it tracks
                // the element if the canvas resizes or the layout reflows.
                node.frameGetter = () => RectTransformToScreenRect(rect);
            }
            return node;
        }

        /// <summary>Removes a node created by CreateMenuNode - call this
        /// on OnDisable() for any menu panel/element that can close, so the
        /// hierarchy never points the screen reader at a destroyed or
        /// hidden element. Unity does not do this automatically; per the
        /// "lost focus after actions" failure mode
        /// (~/ACCESSIBILITY_WORKFLOW.md), a stale node left in place after
        /// a panel closes is exactly the kind of silent gap that rule
        /// exists to catch.</summary>
        public void RemoveMenuNode(AccessibilityNode node)
        {
            if (node == null || _hierarchy == null) return;
            _hierarchy.RemoveNode(node);
        }

        /// <summary>Tells the OS something in the hierarchy changed shape
        /// (a panel opened/closed, focus should move) - Unity does not
        /// infer this from AddNode/RemoveNode calls on its own. Call after
        /// opening a menu panel, closing one, or any action whose result
        /// should be what the screen reader announces next; pass the node
        /// that should receive focus when known (e.g. the first control in
        /// a newly-opened panel), or omit it to just signal a layout
        /// change without forcing focus anywhere specific.</summary>
        public void NotifyLayoutChanged(AccessibilityNode focusTarget = null)
        {
            if (!AssistiveSupport.isScreenReaderEnabled) return;
            AssistiveSupport.notificationDispatcher.SendLayoutChanged(focusTarget);
        }

        private static Rect RectTransformToScreenRect(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);

            var canvas = rect.GetComponentInParent<Canvas>();
            var cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;

            var min = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
            var max = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);

            // Screen-space Y grows upward from WorldToScreenPoint but the
            // accessibility frame (like most screen/OS coordinate spaces)
            // expects Y growing downward from the top - flip here so the
            // reported frame actually lines up with where VoiceOver/Narrator
            // expect to hit-test on screen.
            return new Rect(min.x, Screen.height - max.y, max.x - min.x, max.y - min.y);
        }
    }
}
