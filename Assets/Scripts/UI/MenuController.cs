using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EchoesOfNeon.UI
{
    /// <summary>
    /// Wires every button/toggle/slider built by MainMenuUISetup.cs to the
    /// actual settings/save systems. All listeners are added at runtime
    /// (Start(), non-persistent AddListener) rather than as persistent
    /// UnityEvent calls, so the whole menu can be built headlessly by an
    /// Editor script without needing to script persistent-listener
    /// serialization - one central, explicit, readable place to see every
    /// control's behaviour, matching this project's existing style
    /// (explicit code over reflection/generic binding magic).
    ///
    /// Panel visibility is plain SetActive() toggling. That's deliberate,
    /// not just simplicity: AccessibleButton/AccessibleToggle/AccessibleSlider
    /// each register their AccessibilityNode in OnEnable and remove it in
    /// OnDisable, so SetActive(false) on a panel already does the "keep the
    /// hierarchy in sync as panels open/close" work the accessibility
    /// research called out - no extra bookkeeping needed here.
    /// </summary>
    public class MenuController : MonoBehaviour
    {
        [Header("Top-level panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject optionsPanel;

        [Header("Main menu buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private AccessibleButton continueAccessibleButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button quitButton;

        [Header("Options tabs")]
        [SerializeField] private GameObject controllerPanel;
        [SerializeField] private GameObject keybindPanel;
        [SerializeField] private GameObject audioPanel;
        [SerializeField] private GameObject accessibilityPanel;
        [SerializeField] private Button controllerTabButton;
        [SerializeField] private Button keybindTabButton;
        [SerializeField] private Button audioTabButton;
        [SerializeField] private Button accessibilityTabButton;
        [SerializeField] private Button optionsBackButton;

        [Header("Controller settings")]
        [SerializeField] private Slider aimAssistFrictionSlider;
        [SerializeField] private Slider aimAssistMagnetismSlider;
        [SerializeField] private Toggle singleStickToggle;
        [SerializeField] private Toggle toggleAimToggle;
        [SerializeField] private Toggle toggleSprintToggle;
        [SerializeField] private Toggle toggleCrouchToggle;
        [SerializeField] private Slider lookSensitivitySlider;

        [Header("Audio settings")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider dialogueVolumeSlider;

        [Header("Accessibility settings")]
        [SerializeField] private Slider colorblindModeSlider;
        [SerializeField] private Toggle highContrastOutlinesToggle;
        [SerializeField] private Toggle cameraBobToggle;
        [SerializeField] private Slider screenShakeSlider;
        [SerializeField] private Toggle reduceMotionToggle;
        [SerializeField] private Toggle audioDescriptionToggle;
        [SerializeField] private Toggle subtitlesToggle;

        [Header("Other")]
        [SerializeField] private MenuMusicPlayer musicPlayer;
        [SerializeField] private string firstGameplaySceneName = "TestRange_Proto";

        private bool _suppressCallbacks; // true while we're pushing loaded settings INTO the controls, so that doesn't re-trigger a save

        private void Start()
        {
            WireMainMenu();
            WireOptionsTabs();
            WireControllerSettings();
            WireAudioSettings();
            WireAccessibilitySettings();

            ShowOnly(new[] { mainMenuPanel, optionsPanel }, mainMenuPanel);
            RefreshContinueButton();
            if (GameSaveManager.Instance != null)
                GameSaveManager.Instance.OnSaveDataChanged += RefreshContinueButton;
        }

        private void OnDestroy()
        {
            if (GameSaveManager.Instance != null)
                GameSaveManager.Instance.OnSaveDataChanged -= RefreshContinueButton;
        }

        // --- Main menu ---

        private void WireMainMenu()
        {
            startButton.onClick.AddListener(() =>
            {
                GameSaveManager.Instance?.ClearSave();
                SceneManager.LoadScene(firstGameplaySceneName);
            });

            continueButton.onClick.AddListener(() =>
            {
                if (GameSaveManager.Instance == null || !GameSaveManager.Instance.HasSaveData) return;
                SceneManager.LoadScene(GameSaveManager.Instance.Data.sceneName);
            });

            optionsButton.onClick.AddListener(() => ShowOnly(new[] { mainMenuPanel, optionsPanel }, optionsPanel));

            quitButton.onClick.AddListener(() =>
            {
                AccessibilityManager.Instance?.Announce("Quitting game.");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
        }

        private void RefreshContinueButton()
        {
            var hasSave = GameSaveManager.Instance != null && GameSaveManager.Instance.HasSaveData;
            if (continueAccessibleButton != null) continueAccessibleButton.SetInteractable(hasSave);
            else if (continueButton != null) continueButton.interactable = hasSave;
        }

        // --- Options tab switching ---

        private void WireOptionsTabs()
        {
            var tabs = new[] { controllerPanel, keybindPanel, audioPanel, accessibilityPanel };
            controllerTabButton.onClick.AddListener(() => ShowOnly(tabs, controllerPanel));
            keybindTabButton.onClick.AddListener(() => ShowOnly(tabs, keybindPanel));
            audioTabButton.onClick.AddListener(() => ShowOnly(tabs, audioPanel));
            accessibilityTabButton.onClick.AddListener(() => ShowOnly(tabs, accessibilityPanel));
            optionsBackButton.onClick.AddListener(() => ShowOnly(new[] { mainMenuPanel, optionsPanel }, mainMenuPanel));

            // Default to the Controller tab the first time Options opens.
            ShowOnly(tabs, controllerPanel);
        }

        private static void ShowOnly(GameObject[] group, GameObject show)
        {
            foreach (var g in group)
                if (g != null) g.SetActive(g == show);
        }

        // --- Controller settings ---

        private void WireControllerSettings()
        {
            var s = AccessibilityManager.Instance?.Settings;
            if (s == null) return;

            _suppressCallbacks = true;
            aimAssistFrictionSlider.value = s.aimAssistFriction;
            aimAssistMagnetismSlider.value = s.aimAssistMagnetism;
            singleStickToggle.isOn = s.singleStickMode;
            toggleAimToggle.isOn = s.toggleAim;
            toggleSprintToggle.isOn = s.toggleSprint;
            toggleCrouchToggle.isOn = s.toggleCrouch;
            lookSensitivitySlider.value = s.lookSensitivity;
            _suppressCallbacks = false;

            aimAssistFrictionSlider.onValueChanged.AddListener(v => ApplyAccessibility(x => x.aimAssistFriction = v));
            aimAssistMagnetismSlider.onValueChanged.AddListener(v => ApplyAccessibility(x => x.aimAssistMagnetism = v));
            singleStickToggle.onValueChanged.AddListener(v => ApplyAccessibility(x => x.singleStickMode = v));
            toggleAimToggle.onValueChanged.AddListener(v => ApplyAccessibility(x => x.toggleAim = v));
            toggleSprintToggle.onValueChanged.AddListener(v => ApplyAccessibility(x => x.toggleSprint = v));
            toggleCrouchToggle.onValueChanged.AddListener(v => ApplyAccessibility(x => x.toggleCrouch = v));
            lookSensitivitySlider.onValueChanged.AddListener(v => ApplyAccessibility(x => x.lookSensitivity = v));
        }

        // --- Audio settings ---

        private void WireAudioSettings()
        {
            var d = AudioSettingsManager.Instance?.Data;
            if (d == null) return;

            _suppressCallbacks = true;
            masterVolumeSlider.value = d.masterVolume;
            musicVolumeSlider.value = d.musicVolume;
            sfxVolumeSlider.value = d.sfxVolume;
            dialogueVolumeSlider.value = d.dialogueVolume;
            _suppressCallbacks = false;

            masterVolumeSlider.onValueChanged.AddListener(v => ApplyAudio(x => x.masterVolume = v));
            musicVolumeSlider.onValueChanged.AddListener(v => ApplyAudio(x => x.musicVolume = v));
            sfxVolumeSlider.onValueChanged.AddListener(v => ApplyAudio(x => x.sfxVolume = v));
            dialogueVolumeSlider.onValueChanged.AddListener(v => ApplyAudio(x => x.dialogueVolume = v));
        }

        // --- Accessibility settings ---

        private void WireAccessibilitySettings()
        {
            var s = AccessibilityManager.Instance?.Settings;
            if (s == null) return;

            _suppressCallbacks = true;
            colorblindModeSlider.value = (int)s.colorblindMode;
            highContrastOutlinesToggle.isOn = s.highContrastOutlines;
            cameraBobToggle.isOn = s.cameraBobEnabled;
            screenShakeSlider.value = s.screenShakeScale;
            reduceMotionToggle.isOn = s.reduceMotion;
            audioDescriptionToggle.isOn = s.audioDescriptionEnabled;
            subtitlesToggle.isOn = s.subtitlesEnabled;
            _suppressCallbacks = false;

            colorblindModeSlider.onValueChanged.AddListener(v => ApplyAccessibility(x => x.colorblindMode = (ColorblindMode)Mathf.RoundToInt(v)));
            highContrastOutlinesToggle.onValueChanged.AddListener(v => ApplyAccessibility(x => x.highContrastOutlines = v));
            cameraBobToggle.onValueChanged.AddListener(v => ApplyAccessibility(x => x.cameraBobEnabled = v));
            screenShakeSlider.onValueChanged.AddListener(v => ApplyAccessibility(x => x.screenShakeScale = v));
            reduceMotionToggle.onValueChanged.AddListener(v => ApplyAccessibility(x => x.reduceMotion = v));
            audioDescriptionToggle.onValueChanged.AddListener(v => ApplyAccessibility(x => x.audioDescriptionEnabled = v));
            subtitlesToggle.onValueChanged.AddListener(v => ApplyAccessibility(x => x.subtitlesEnabled = v));
        }

        // --- Shared apply helpers ---

        private void ApplyAccessibility(System.Action<AccessibilitySettings> mutate)
        {
            if (_suppressCallbacks || AccessibilityManager.Instance == null) return;
            var s = AccessibilityManager.Instance.Settings;
            mutate(s);
            AccessibilityManager.Instance.ApplySettings(s);
        }

        private void ApplyAudio(System.Action<AudioSettingsData> mutate)
        {
            if (_suppressCallbacks || AudioSettingsManager.Instance == null) return;
            var d = AudioSettingsManager.Instance.Data;
            mutate(d);
            AudioSettingsManager.Instance.ApplySettings(d);
        }
    }
}
