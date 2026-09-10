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
    }

    /// <summary>
    /// Phase 2: the accessibility settings hub, plus two responsibilities
    /// that only belong here:
    /// 1. Toggle-vs-hold translation for Aim/Sprint/Crouch, layered on top of
    ///    InputManager's raw press/release events (InputManager deliberately
    ///    stays unaware of this preference - see its own header comment).
    /// 2. The native screen-reader hookup (UnityEngine.Accessibility,
    ///    UI-Automation-based on Windows - the reason this project targets
    ///    Unity 6.3+ instead of the 6.0 LTS line). This wires the one-off
    ///    "announce this" path now (Announce(string)); building out a full
    ///    AccessibilityNode tree for real menu buttons is Phase 5/6 work,
    ///    once actual menu UI exists to attach nodes to - an empty
    ///    AccessibilityHierarchy is registered now so that plumbing is ready.
    /// </summary>
    public class AccessibilityManager : MonoBehaviour
    {
        public static AccessibilityManager Instance { get; private set; }

        public AccessibilitySettings Settings { get; private set; } = new AccessibilitySettings();
        public event Action<AccessibilitySettings> OnSettingsChanged;

        public bool IsAiming { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool IsCrouching { get; private set; }

        public event Action<bool> OnAimStateChanged;
        public event Action<bool> OnSprintStateChanged;
        public event Action<bool> OnCrouchStateChanged;

        private const string SettingsPrefsKey = "EchoesOfNeon.AccessibilitySettings";

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
        }

        // --- Toggle-vs-hold translation ---

        private void HandleAimPressed() => SetAiming(Settings.toggleAim ? !IsAiming : true);
        private void HandleAimReleased() { if (!Settings.toggleAim) SetAiming(false); }
        private void SetAiming(bool value)
        {
            if (IsAiming == value) return;
            IsAiming = value;
            OnAimStateChanged?.Invoke(IsAiming);
        }

        private void HandleSprintPressed() => SetSprinting(Settings.toggleSprint ? !IsSprinting : true);
        private void HandleSprintReleased() { if (!Settings.toggleSprint) SetSprinting(false); }
        private void SetSprinting(bool value)
        {
            if (IsSprinting == value) return;
            IsSprinting = value;
            OnSprintStateChanged?.Invoke(IsSprinting);
        }

        private void HandleCrouchPressed() => SetCrouching(Settings.toggleCrouch ? !IsCrouching : true);
        private void HandleCrouchReleased() { if (!Settings.toggleCrouch) SetCrouching(false); }
        private void SetCrouching(bool value)
        {
            if (IsCrouching == value) return;
            IsCrouching = value;
            OnCrouchStateChanged?.Invoke(IsCrouching);
        }

        // --- Settings persistence ---

        public void LoadSettings()
        {
            if (!PlayerPrefs.HasKey(SettingsPrefsKey)) return;
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(SettingsPrefsKey), Settings);
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
            if (string.IsNullOrEmpty(text) || !AssistiveSupport.isScreenReaderEnabled) return;
            AssistiveSupport.notificationDispatcher.SendAnnouncement(text);
        }
    }
}
