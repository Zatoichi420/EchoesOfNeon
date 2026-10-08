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
        [SerializeField] private Toggle monoAudioToggle;

        [Header("Accessibility settings")]
        [SerializeField] private Slider colorblindModeSlider;
        [SerializeField] private Toggle highContrastOutlinesToggle;
        [SerializeField] private Toggle cameraBobToggle;
        [SerializeField] private Slider screenShakeSlider;
        [SerializeField] private Toggle reduceMotionToggle;
        [SerializeField] private Toggle audioDescriptionToggle;
        [SerializeField] private Toggle subtitlesToggle;
        [SerializeField] private Toggle soundCaptionsToggle;
        [SerializeField] private Slider uiScaleSlider;

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

            HideUnimplementedSettings();

            ShowOnly(new[] { mainMenuPanel, optionsPanel }, mainMenuPanel);
            Focus(startButton);
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

            optionsButton.onClick.AddListener(() =>
            {
                ShowOnly(new[] { mainMenuPanel, optionsPanel }, optionsPanel);
                ShowTab(controllerPanel, controllerTabButton, announceTitle: false);
                _optionsAccessible.AnnounceTitle();
                Focus(controllerTabButton);
            });

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
            _tabs = new[] { controllerPanel, keybindPanel, audioPanel, accessibilityPanel };
            _tabButtons = new[] { controllerTabButton, keybindTabButton, audioTabButton, accessibilityTabButton };
            var tabTitles = new[] { "Controller settings", "Key bindings", "Audio settings", "Accessibility settings" };

            // Titled containers + role/selected state, so the screen reader
            // knows these are tabs and which one is open.
            for (int i = 0; i < _tabs.Length; i++)
            {
                var panel = _tabs[i].AddComponent<AccessiblePanel>();
                panel.Title = tabTitles[i];
                panel.OnDismiss = GoBackFromOptions;
                _tabPanels[i] = panel;
                _tabButtons[i].GetComponent<AccessibleButton>()?.SetRole(UnityEngine.Accessibility.AccessibilityRole.TabButton);
            }
            _optionsAccessible = optionsPanel.AddComponent<AccessiblePanel>();
            _optionsAccessible.Title = "Options";
            _optionsAccessible.OnDismiss = GoBackFromOptions;

            // Escape / gamepad B anywhere inside Options goes back.
            optionsPanel.AddComponent<CancelRelay>().OnCancelRequested = GoBackFromOptions;

            for (int i = 0; i < _tabs.Length; i++)
            {
                var panelObj = _tabs[i];
                var button = _tabButtons[i];
                button.onClick.AddListener(() => ShowTab(panelObj, button, announceTitle: true));
            }
            optionsBackButton.onClick.AddListener(GoBackFromOptions);

            // Default to the Controller tab the first time Options opens.
            ShowTab(controllerPanel, controllerTabButton, announceTitle: false);
        }

        private GameObject[] _tabs;
        private Button[] _tabButtons;
        private readonly AccessiblePanel[] _tabPanels = new AccessiblePanel[4];
        private AccessiblePanel _optionsAccessible;

        private void ShowTab(GameObject panel, Button tabButton, bool announceTitle)
        {
            ShowOnly(_tabs, panel);
            for (int i = 0; i < _tabButtons.Length; i++)
                _tabButtons[i].GetComponent<AccessibleButton>()?.SetSelected(_tabButtons[i] == tabButton);
            if (announceTitle)
                for (int i = 0; i < _tabs.Length; i++)
                    if (_tabs[i] == panel) { _tabPanels[i].AnnounceTitle(); break; }
        }

        private void GoBackFromOptions()
        {
            if (!optionsPanel.activeInHierarchy) return;
            ShowOnly(new[] { mainMenuPanel, optionsPanel }, mainMenuPanel);
            AccessibilityManager.Instance?.SetFocusPrefix("Main menu.");
            Focus(optionsButton);
        }

        /// <summary>Moves keyboard/gamepad focus to a control after a panel
        /// change. Without this the EventSystem keeps pointing at a button
        /// that was just hidden and arrow keys do nothing.</summary>
        private static void Focus(Selectable target)
        {
            if (target == null || UnityEngine.EventSystems.EventSystem.current == null) return;
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(target.gameObject);
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
            monoAudioToggle.isOn = d.monoAudio;
            _suppressCallbacks = false;

            masterVolumeSlider.onValueChanged.AddListener(v => ApplyAudio(x => x.masterVolume = v));
            musicVolumeSlider.onValueChanged.AddListener(v => ApplyAudio(x => x.musicVolume = v));
            sfxVolumeSlider.onValueChanged.AddListener(v => ApplyAudio(x => x.sfxVolume = v));
            dialogueVolumeSlider.onValueChanged.AddListener(v => ApplyAudio(x => x.dialogueVolume = v));
            monoAudioToggle.onValueChanged.AddListener(v => ApplyAudio(x => x.monoAudio = v));
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
            soundCaptionsToggle.isOn = s.soundCaptionsEnabled;
            uiScaleSlider.value = UiScaleStepFor(s.uiScale);
            _suppressCallbacks = false;

            colorblindModeSlider.onValueChanged.AddListener(v => ApplyAccessibility(x => x.colorblindMode = (ColorblindMode)Mathf.RoundToInt(v)));
            highContrastOutlinesToggle.onValueChanged.AddListener(v => ApplyAccessibility(x => x.highContrastOutlines = v));
            cameraBobToggle.onValueChanged.AddListener(v => ApplyAccessibility(x => x.cameraBobEnabled = v));
            screenShakeSlider.onValueChanged.AddListener(v => ApplyAccessibility(x => x.screenShakeScale = v));
            reduceMotionToggle.onValueChanged.AddListener(v => ApplyAccessibility(x => x.reduceMotion = v));
            audioDescriptionToggle.onValueChanged.AddListener(v => ApplyAccessibility(x => x.audioDescriptionEnabled = v));
            subtitlesToggle.onValueChanged.AddListener(v => ApplyAccessibility(x => x.subtitlesEnabled = v));
            soundCaptionsToggle.onValueChanged.AddListener(v => ApplyAccessibility(x => x.soundCaptionsEnabled = v));
            uiScaleSlider.onValueChanged.AddListener(v => ApplyAccessibility(x => x.uiScale = UiScaleSteps[Mathf.Clamp(Mathf.RoundToInt(v), 0, UiScaleSteps.Length - 1)]));
        }

        // Slider step -> scale: 100 / 125 / 150 percent.
        public static readonly float[] UiScaleSteps = { 1f, 1.25f, 1.5f };

        private static int UiScaleStepFor(float scale)
        {
            int best = 0;
            for (int i = 1; i < UiScaleSteps.Length; i++)
                if (Mathf.Abs(UiScaleSteps[i] - scale) < Mathf.Abs(UiScaleSteps[best] - scale)) best = i;
            return best;
        }

        // --- Hide controls whose setting nothing reads yet ---

        private void HideUnimplementedSettings()
        {
            HideIfUnimplemented(AccessibilityManager.UnimplementedSettings, nameof(AccessibilitySettings.aimAssistFriction), aimAssistFrictionSlider);
            HideIfUnimplemented(AccessibilityManager.UnimplementedSettings, nameof(AccessibilitySettings.aimAssistMagnetism), aimAssistMagnetismSlider);
            HideIfUnimplemented(AccessibilityManager.UnimplementedSettings, nameof(AccessibilitySettings.screenShakeScale), screenShakeSlider);
            HideIfUnimplemented(AccessibilityManager.UnimplementedSettings, nameof(AccessibilitySettings.audioDescriptionEnabled), audioDescriptionToggle);
            HideIfUnimplemented(AudioSettingsManager.UnimplementedSettings, nameof(AudioSettingsData.sfxVolume), sfxVolumeSlider);
            HideIfUnimplemented(AudioSettingsManager.UnimplementedSettings, nameof(AudioSettingsData.dialogueVolume), dialogueVolumeSlider);
            CompactRows(controllerPanel.transform);
            CompactRows(audioPanel.transform);
            CompactRows(accessibilityPanel.transform);
        }

        /// <summary>Closes the gaps left by hidden rows: active children keep
        /// their order but are re-spaced at the panel's original row pitch.</summary>
        public static void CompactRows(Transform panel)
        {
            var ys = new System.Collections.Generic.SortedSet<float>(System.Collections.Generic.Comparer<float>.Create((a, b) => b.CompareTo(a)));
            foreach (Transform child in panel)
            {
                var rt = child as RectTransform;
                if (rt != null && child.gameObject.activeSelf) ys.Add(Mathf.Round(rt.anchoredPosition.y));
            }
            if (ys.Count < 2) return;

            var list = new System.Collections.Generic.List<float>(ys);
            float pitch = float.MaxValue;
            for (int i = 1; i < list.Count; i++) pitch = Mathf.Min(pitch, list[i - 1] - list[i]);
            if (pitch < 1f) return;

            foreach (Transform child in panel)
            {
                var rt = child as RectTransform;
                if (rt == null || !child.gameObject.activeSelf) continue;
                int row = list.IndexOf(Mathf.Round(rt.anchoredPosition.y));
                if (row < 0) continue;
                var p = rt.anchoredPosition;
                p.y = list[0] - row * pitch;
                rt.anchoredPosition = p;
            }
        }

        private static void HideIfUnimplemented(string[] unimplemented, string field, Selectable control)
        {
            if (control == null || System.Array.IndexOf(unimplemented, field) < 0) return;

            // Each row is a Text label plus the control, placed at the same
            // y. Disable both so neither sighted focus nor the screen reader
            // ever lands on a setting that does nothing. (AccessibleX nodes
            // remove themselves in OnDisable.)
            var controlRect = (RectTransform)control.transform;
            foreach (Transform sibling in control.transform.parent)
            {
                if (sibling == control.transform) continue;
                var rt = sibling as RectTransform;
                if (rt != null && sibling.GetComponent<Text>() != null
                    && Mathf.Approximately(rt.anchoredPosition.y, controlRect.anchoredPosition.y))
                    sibling.gameObject.SetActive(false);
            }
            control.gameObject.SetActive(false);
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
