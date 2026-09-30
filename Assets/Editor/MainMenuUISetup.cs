// One-time scene-authoring script for the main menu, built entirely in code
// - same batch-mode approach as TestSceneSetup.cs. Run via:
//   Unity.exe -batchmode -nographics -projectPath <path>
//     -executeMethod MainMenuUISetup.BuildMainMenuScene -quit -logFile <log>
using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using EchoesOfNeon.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class MainMenuUISetup
{
    // --- Palette: dark cyberpunk-noir, cyan for interactive/tech accents -
    // matches Docs/visual-standards.md's established cyan-for-augmented-tech
    // rule and the warm/cold contrast pattern, applied to UI chrome instead
    // of 3D lighting since this screen has none. ---
    private static readonly Color BgColor = new Color(0.03f, 0.035f, 0.045f, 1f);
    private static readonly Color PanelColor = new Color(0.07f, 0.085f, 0.10f, 0.96f);
    private static readonly Color AccentCyan = new Color(0.15f, 0.85f, 0.95f, 1f);
    private static readonly Color TextColor = new Color(0.93f, 0.96f, 0.97f, 1f); // near-white, not pure white - softer on a dark bg at high contrast
    private static readonly Color ButtonNormalColor = new Color(0.11f, 0.135f, 0.16f, 1f);
    private static readonly Color ButtonDisabledColor = new Color(0.11f, 0.11f, 0.11f, 0.5f);

    // --- Fonts: two-tier, per Orlando's explicit request (2026-09-28) -
    // Rajdhani (angular/cyberpunk) for titles/headers ONLY, Atkinson
    // Hyperlegible (Braille Institute, designed for low vision) for every
    // interactive element - buttons, labels, values - on every screen, so
    // the aesthetic and the legibility requirement don't fight each other.
    // Legacy UI Text, not TextMeshPro: TMP needs a font ATLAS asset, and
    // generating one headlessly (no Editor GUI in this environment) was
    // judged too high-risk/unverifiable for this pass - the TMP package is
    // still added to the project (Packages/manifest.json) for future use,
    // but this scene deliberately doesn't depend on it yet. See this
    // session's MEMORY.md entry for the full reasoning. ---
    private static Font _titleFont;
    private static Font _bodyFont;

    private static Font TitleFont => _titleFont != null ? _titleFont : BodyFont;
    private static Font BodyFont => _bodyFont != null ? _bodyFont : Resources.GetBuiltinResource<Font>("Arial.ttf");

    public static void BuildMainMenuScene()
    {
        LoadFonts();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateManagers();

        var cameraGO = new GameObject("Camera");
        var camera = cameraGO.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = BgColor;
        camera.orthographic = true;
        cameraGO.AddComponent<AudioListener>();
        cameraGO.tag = "MainCamera";

        var canvasGO = new GameObject("MenuCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        CreateEventSystem();

        var resources = UIResources();

        var mainMenuPanel = BuildMainMenuPanel(canvasGO.transform, resources,
            out var startBtn, out var continueBtn, out var continueAccessible, out var optionsBtn, out var quitBtn);

        var (optionsPanel, controllerPanel, keybindPanel, audioPanel, accessibilityPanel,
                controllerTab, keybindTab, audioTab, accessibilityTab, optionsBack,
                controllerRefs, audioRefs, accessibilityRefs) =
            BuildOptionsPanel(canvasGO.transform, resources);

        var musicGO = new GameObject("MenuMusicPlayer");
        musicGO.transform.SetParent(canvasGO.transform, false);
        var musicPlayer = musicGO.AddComponent<MenuMusicPlayer>();
        var musicSo = new SerializedObject(musicPlayer);
        var placeholderClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Ambient/new_carthage_night_rain.wav");
        if (placeholderClip == null)
            Debug.LogWarning("[MainMenuUISetup] Placeholder ambient clip not found - menu will be silent until Assets/Audio/Ambient/new_carthage_night_rain.wav exists or a clip is assigned.");
        musicSo.FindProperty("musicClip").objectReferenceValue = placeholderClip;
        musicSo.ApplyModifiedProperties();

        var controllerGO = new GameObject("MenuController");
        var controller = controllerGO.AddComponent<MenuController>();
        var so = new SerializedObject(controller);
        so.FindProperty("mainMenuPanel").objectReferenceValue = mainMenuPanel;
        so.FindProperty("optionsPanel").objectReferenceValue = optionsPanel;
        so.FindProperty("startButton").objectReferenceValue = startBtn;
        so.FindProperty("continueAccessibleButton").objectReferenceValue = continueAccessible;
        so.FindProperty("continueButton").objectReferenceValue = continueBtn;
        so.FindProperty("optionsButton").objectReferenceValue = optionsBtn;
        so.FindProperty("quitButton").objectReferenceValue = quitBtn;

        so.FindProperty("controllerPanel").objectReferenceValue = controllerPanel;
        so.FindProperty("keybindPanel").objectReferenceValue = keybindPanel;
        so.FindProperty("audioPanel").objectReferenceValue = audioPanel;
        so.FindProperty("accessibilityPanel").objectReferenceValue = accessibilityPanel;
        so.FindProperty("controllerTabButton").objectReferenceValue = controllerTab;
        so.FindProperty("keybindTabButton").objectReferenceValue = keybindTab;
        so.FindProperty("audioTabButton").objectReferenceValue = audioTab;
        so.FindProperty("accessibilityTabButton").objectReferenceValue = accessibilityTab;
        so.FindProperty("optionsBackButton").objectReferenceValue = optionsBack;

        so.FindProperty("aimAssistFrictionSlider").objectReferenceValue = controllerRefs.aimAssistFriction;
        so.FindProperty("aimAssistMagnetismSlider").objectReferenceValue = controllerRefs.aimAssistMagnetism;
        so.FindProperty("singleStickToggle").objectReferenceValue = controllerRefs.singleStick;
        so.FindProperty("toggleAimToggle").objectReferenceValue = controllerRefs.toggleAim;
        so.FindProperty("toggleSprintToggle").objectReferenceValue = controllerRefs.toggleSprint;
        so.FindProperty("toggleCrouchToggle").objectReferenceValue = controllerRefs.toggleCrouch;
        so.FindProperty("lookSensitivitySlider").objectReferenceValue = controllerRefs.lookSensitivity;

        so.FindProperty("masterVolumeSlider").objectReferenceValue = audioRefs.master;
        so.FindProperty("musicVolumeSlider").objectReferenceValue = audioRefs.music;
        so.FindProperty("sfxVolumeSlider").objectReferenceValue = audioRefs.sfx;
        so.FindProperty("dialogueVolumeSlider").objectReferenceValue = audioRefs.dialogue;

        so.FindProperty("colorblindModeSlider").objectReferenceValue = accessibilityRefs.colorblindMode;
        so.FindProperty("highContrastOutlinesToggle").objectReferenceValue = accessibilityRefs.highContrastOutlines;
        so.FindProperty("cameraBobToggle").objectReferenceValue = accessibilityRefs.cameraBob;
        so.FindProperty("screenShakeSlider").objectReferenceValue = accessibilityRefs.screenShake;
        so.FindProperty("reduceMotionToggle").objectReferenceValue = accessibilityRefs.reduceMotion;
        so.FindProperty("audioDescriptionToggle").objectReferenceValue = accessibilityRefs.audioDescription;
        so.FindProperty("subtitlesToggle").objectReferenceValue = accessibilityRefs.subtitles;

        so.FindProperty("musicPlayer").objectReferenceValue = musicPlayer;
        so.ApplyModifiedProperties();

        const string scenePath = "Assets/Scenes/MainMenu.unity";
        EditorSceneManager.SaveScene(scene, scenePath);

        var buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        buildScenes.RemoveAll(s => s.path == scenePath);
        buildScenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = buildScenes.ToArray();

        Debug.Log("[MainMenuUISetup] Main menu scene built and saved to " + scenePath);
    }

    private static void LoadFonts()
    {
        _titleFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Rajdhani-Bold.ttf");
        _bodyFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/AtkinsonHyperlegible-Regular.ttf");
        if (_titleFont == null || _bodyFont == null)
            Debug.LogWarning("[MainMenuUISetup] Custom fonts not found at Assets/Fonts/ - falling back to Unity's built-in font. Make sure Rajdhani-Bold.ttf and AtkinsonHyperlegible-Regular.ttf are imported before running this.");
    }

    private static void CreateManagers()
    {
        var inputGO = new GameObject("InputManager");
        inputGO.AddComponent<InputManager>();

        var accessibilityGO = new GameObject("AccessibilityManager");
        accessibilityGO.AddComponent<AccessibilityManager>();

        var saveGO = new GameObject("GameSaveManager");
        saveGO.AddComponent<GameSaveManager>();

        var audioSettingsGO = new GameObject("AudioSettingsManager");
        audioSettingsGO.AddComponent<AudioSettingsManager>();
    }

    private static void CreateEventSystem()
    {
        var go = new GameObject("EventSystem");
        go.AddComponent<UnityEngine.EventSystems.EventSystem>();
        var module = go.AddComponent<InputSystemUIInputModule>();

        // Project uses the new Input System exclusively (activeInputHandler
        // = 1) - build a small dedicated UI action map here rather than
        // relying on an auto-assigned default asset, since none exists in
        // this project yet and InputSystemUIInputModule doesn't fabricate
        // one on its own.
        // Must live inside a real InputActionAsset, not a loose
        // InputActionMap - InputActionReference.Create() (used below to
        // wire the module's point/click/move/submit/cancel properties)
        // throws otherwise.
        var uiAsset = ScriptableObject.CreateInstance<InputActionAsset>();
        var uiMap = uiAsset.AddActionMap("UI");
        // Saved as a real project asset (not left in-memory only) so the
        // InputActionReferences assigned below survive a scene save/reload
        // with a stable GUID, the same as any other serialized asset
        // reference.
        System.IO.Directory.CreateDirectory("Assets/Input");
        AssetDatabase.CreateAsset(uiAsset, "Assets/Input/MenuUIActions.inputactions.asset");
        var point = uiMap.AddAction("Point", InputActionType.PassThrough, expectedControlLayout: "Vector2");
        point.AddBinding("<Mouse>/position");
        var click = uiMap.AddAction("Click", InputActionType.PassThrough, expectedControlLayout: "Button");
        click.AddBinding("<Mouse>/leftButton");
        var navigate = uiMap.AddAction("Navigate", InputActionType.PassThrough, expectedControlLayout: "Vector2");
        navigate.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
        navigate.AddBinding("<Gamepad>/leftStick");
        navigate.AddBinding("<Gamepad>/dpad");
        var submit = uiMap.AddAction("Submit", InputActionType.Button);
        submit.AddBinding("<Keyboard>/enter");
        submit.AddBinding("<Gamepad>/buttonSouth");
        var cancel = uiMap.AddAction("Cancel", InputActionType.Button);
        cancel.AddBinding("<Keyboard>/escape");
        cancel.AddBinding("<Gamepad>/buttonEast");
        uiMap.Enable();
        AssetDatabase.SaveAssets();

        module.point = InputActionReference.Create(point);
        module.leftClick = InputActionReference.Create(click);
        module.move = InputActionReference.Create(navigate);
        module.submit = InputActionReference.Create(submit);
        module.cancel = InputActionReference.Create(cancel);
    }

    private static DefaultControls.Resources UIResources() => new DefaultControls.Resources
    {
        standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
        background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
        inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd"),
        knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
        checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd"),
        dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd"),
        mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd"),
    };

    // --- Generic building blocks ---

    private static RectTransform SetRect(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 size)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        return rt;
    }

    private static GameObject CreateFullRectPanel(string name, Transform parent, Color color, bool startActive)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        SetRect(go.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        var rt = go.GetComponent<RectTransform>();
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = color;
        go.SetActive(startActive);
        return go;
    }

    private static Text CreateTitle(string content, Transform parent, float y)
    {
        var go = new GameObject("Title", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        SetRect(go.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(1200f, 90f));
        var text = go.AddComponent<Text>();
        text.font = TitleFont;
        text.fontSize = 64;
        text.color = AccentCyan;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = content;
        return text;
    }

    private static GameObject CreateMenuButton(string name, string label, Transform parent, DefaultControls.Resources resources, Vector2 anchoredPos, Vector2 size)
    {
        var go = DefaultControls.CreateButton(resources);
        go.name = name;
        go.transform.SetParent(parent, false);
        SetRect(go.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), anchoredPos, size);

        var img = go.GetComponent<Image>();
        img.color = ButtonNormalColor;
        var button = go.GetComponent<Button>();
        var colors = button.colors;
        colors.normalColor = Color.white; // multiplies with Image.color above - keep white so ButtonNormalColor shows through unmodified
        colors.highlightedColor = new Color(0.7f, 0.95f, 1f, 1f);
        colors.pressedColor = AccentCyan;
        colors.selectedColor = new Color(0.6f, 0.9f, 1f, 1f);
        colors.disabledColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        button.colors = colors;

        var text = go.GetComponentInChildren<Text>();
        text.font = BodyFont;
        text.fontSize = 30;
        text.color = TextColor;
        text.text = label;
        text.alignment = TextAnchor.MiddleCenter;

        go.AddComponent<AccessibleButton>();
        return go;
    }

    private static (GameObject go, Toggle toggle) CreateSettingToggle(string name, string label, Transform parent, DefaultControls.Resources resources, float y)
    {
        var labelText = CreateRowLabel(label, parent, y);

        var go = DefaultControls.CreateToggle(resources);
        go.name = name;
        go.transform.SetParent(parent, false);
        SetRect(go.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, y), new Vector2(40f, 40f));

        var bg = go.transform.Find("Background");
        if (bg != null) bg.GetComponent<Image>().color = ButtonNormalColor;
        var checkmark = go.transform.Find("Background/Checkmark");
        if (checkmark != null) checkmark.GetComponent<Image>().color = AccentCyan;

        var toggle = go.GetComponent<Toggle>();
        go.AddComponent<AccessibleToggle>();
        return (go, toggle);
    }

    private static (GameObject go, Slider slider) CreateSettingSlider(string name, string label, Transform parent, DefaultControls.Resources resources, float y, float min, float max, bool wholeNumbers, string[] valueLabels = null)
    {
        CreateRowLabel(label, parent, y);

        var go = DefaultControls.CreateSlider(resources);
        go.name = name;
        go.transform.SetParent(parent, false);
        SetRect(go.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, y), new Vector2(360f, 24f));

        var slider = go.GetComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = wholeNumbers;

        var fill = go.transform.Find("Fill Area/Fill");
        if (fill != null) fill.GetComponent<Image>().color = AccentCyan;
        var handle = go.transform.Find("Handle Slide Area/Handle");
        if (handle != null) handle.GetComponent<Image>().color = TextColor;
        var bg = go.transform.Find("Background");
        if (bg != null) bg.GetComponent<Image>().color = ButtonNormalColor;

        var accessible = go.AddComponent<AccessibleSlider>();
        if (valueLabels != null)
        {
            var so = new SerializedObject(accessible);
            var prop = so.FindProperty("valueLabels");
            prop.arraySize = valueLabels.Length;
            for (int i = 0; i < valueLabels.Length; i++) prop.GetArrayElementAtIndex(i).stringValue = valueLabels[i];
            so.ApplyModifiedProperties();
        }

        return (go, slider);
    }

    private static Text CreateRowLabel(string label, Transform parent, float y)
    {
        var go = new GameObject("Label_" + label, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        SetRect(go.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(60f, y), new Vector2(-460f, 40f));
        var text = go.AddComponent<Text>();
        text.font = BodyFont;
        text.fontSize = 26;
        text.color = TextColor;
        text.alignment = TextAnchor.MiddleLeft;
        text.text = label;
        return text;
    }

    // --- Main menu panel ---

    private static GameObject BuildMainMenuPanel(Transform canvasTransform, DefaultControls.Resources resources,
        out Button startBtn, out Button continueBtn, out AccessibleButton continueAccessible, out Button optionsBtn, out Button quitBtn)
    {
        var panel = CreateFullRectPanel("MainMenuPanel", canvasTransform, BgColor, true);
        CreateTitle("ECHOES OF NEON", panel.transform, -80f);

        float y = -280f;
        const float spacing = 90f;
        var buttonSize = new Vector2(420f, 64f);

        var startGO = CreateMenuButton("StartButton", "Start New Game", panel.transform, resources, new Vector2(0f, y), buttonSize);
        startBtn = startGO.GetComponent<Button>();
        y -= spacing;

        var continueGO = CreateMenuButton("ContinueButton", "Continue", panel.transform, resources, new Vector2(0f, y), buttonSize);
        continueBtn = continueGO.GetComponent<Button>();
        continueAccessible = continueGO.GetComponent<AccessibleButton>();
        continueBtn.interactable = false; // MenuController.RefreshContinueButton() sets the real value from GameSaveManager at Start()
        y -= spacing;

        var optionsGO = CreateMenuButton("OptionsButton", "Options", panel.transform, resources, new Vector2(0f, y), buttonSize);
        optionsBtn = optionsGO.GetComponent<Button>();
        y -= spacing;

        var quitGO = CreateMenuButton("QuitButton", "Quit", panel.transform, resources, new Vector2(0f, y), buttonSize);
        quitBtn = quitGO.GetComponent<Button>();

        return panel;
    }

    // --- Options panel + 4 tabs ---

    private struct ControllerRefs
    {
        public Slider aimAssistFriction, aimAssistMagnetism, lookSensitivity;
        public Toggle singleStick, toggleAim, toggleSprint, toggleCrouch;
    }

    private struct AudioRefs
    {
        public Slider master, music, sfx, dialogue;
    }

    private struct AccessibilityRefs
    {
        public Slider colorblindMode, screenShake;
        public Toggle highContrastOutlines, cameraBob, reduceMotion, audioDescription, subtitles;
    }

    private static (GameObject optionsPanel, GameObject controllerPanel, GameObject keybindPanel, GameObject audioPanel, GameObject accessibilityPanel,
            Button controllerTab, Button keybindTab, Button audioTab, Button accessibilityTab, Button back,
            ControllerRefs controllerRefs, AudioRefs audioRefs, AccessibilityRefs accessibilityRefs)
        BuildOptionsPanel(Transform canvasTransform, DefaultControls.Resources resources)
    {
        var optionsPanel = CreateFullRectPanel("OptionsPanel", canvasTransform, BgColor, false);
        CreateTitle("OPTIONS", optionsPanel.transform, -60f);

        // Tab bar
        var tabNames = new[] { "Controller", "Keybindings", "Audio", "Accessibility" };
        var tabButtons = new Button[4];
        float tabX = -540f;
        for (int i = 0; i < tabNames.Length; i++)
        {
            var tabGO = CreateMenuButton("Tab_" + tabNames[i], tabNames[i], optionsPanel.transform, resources, new Vector2(tabX, -150f), new Vector2(260f, 50f));
            tabButtons[i] = tabGO.GetComponent<Button>();
            tabX += 280f;
        }

        var backGO = CreateMenuButton("BackButton", "Back", optionsPanel.transform, resources, new Vector2(0f, -960f), new Vector2(300f, 56f));

        var controllerPanel = CreateFullRectPanel("ControllerPanel", optionsPanel.transform, Color.clear, true);
        var keybindPanel = CreateFullRectPanel("KeybindPanel", optionsPanel.transform, Color.clear, false);
        var audioPanel = CreateFullRectPanel("AudioPanel", optionsPanel.transform, Color.clear, false);
        var accessibilityPanel = CreateFullRectPanel("AccessibilityPanel", optionsPanel.transform, Color.clear, false);

        var controllerRefs = BuildControllerTab(controllerPanel.transform, resources);
        BuildKeybindTab(keybindPanel.transform, resources);
        var audioRefs = BuildAudioTab(audioPanel.transform, resources);
        var accessibilityRefs = BuildAccessibilityTab(accessibilityPanel.transform, resources);

        return (optionsPanel, controllerPanel, keybindPanel, audioPanel, accessibilityPanel,
            tabButtons[0], tabButtons[1], tabButtons[2], tabButtons[3], backGO.GetComponent<Button>(),
            controllerRefs, audioRefs, accessibilityRefs);
    }

    private static ControllerRefs BuildControllerTab(Transform parent, DefaultControls.Resources resources)
    {
        float y = -230f;
        const float rowSpacing = 70f;
        var refs = new ControllerRefs();

        (_, refs.aimAssistFriction) = CreateSettingSlider("AimAssistFriction", "Aim Assist - Friction", parent, resources, y, 0f, 1f, false); y -= rowSpacing;
        (_, refs.aimAssistMagnetism) = CreateSettingSlider("AimAssistMagnetism", "Aim Assist - Magnetism", parent, resources, y, 0f, 1f, false); y -= rowSpacing;
        (_, refs.lookSensitivity) = CreateSettingSlider("LookSensitivity", "Look Sensitivity", parent, resources, y, 0.1f, 3f, false); y -= rowSpacing;
        (_, refs.singleStick) = CreateSettingToggle("SingleStickMode", "Single-Stick Mode", parent, resources, y); y -= rowSpacing;
        (_, refs.toggleAim) = CreateSettingToggle("ToggleAim", "Toggle Aim (instead of Hold)", parent, resources, y); y -= rowSpacing;
        (_, refs.toggleSprint) = CreateSettingToggle("ToggleSprint", "Toggle Sprint (instead of Hold)", parent, resources, y); y -= rowSpacing;
        (_, refs.toggleCrouch) = CreateSettingToggle("ToggleCrouch", "Toggle Crouch (instead of Hold)", parent, resources, y);

        return refs;
    }

    /// <summary>Only the single-binding button actions - Move/Look are a
    /// composite and a continuous Vector2 action respectively, and
    /// interactively rebinding either needs its own per-part UI this pass
    /// doesn't build (see KeybindRow.cs's own header comment).</summary>
    private static readonly string[] RebindableActions =
    {
        "Fire", "Aim", "Jump", "Interact", "Sprint", "Crouch", "Reload", "Pause", "SonarPulse", "NeuralDilate",
    };

    private static void BuildKeybindTab(Transform parent, DefaultControls.Resources resources)
    {
        float y = -230f;
        const float rowSpacing = 58f;

        foreach (var action in RebindableActions)
        {
            CreateRowLabel(action, parent, y);

            var valueGO = new GameObject("Value_" + action, typeof(RectTransform));
            valueGO.transform.SetParent(parent, false);
            SetRect(valueGO.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-220f, y), new Vector2(160f, 40f));
            var valueText = valueGO.AddComponent<Text>();
            valueText.font = BodyFont;
            valueText.fontSize = 24;
            valueText.color = AccentCyan;
            valueText.alignment = TextAnchor.MiddleRight;
            valueText.text = "-";

            var rowBtnGO = CreateMenuButton("Rebind_" + action, "Rebind", parent, resources, new Vector2(-40f, y), new Vector2(140f, 40f));
            // Anchor to the right edge like the toggle/slider rows, not
            // center - CreateMenuButton anchors top-center by default, so
            // override just this instance's anchor/pivot.
            SetRect(rowBtnGO.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, y), new Vector2(140f, 40f));

            var keybindRow = rowBtnGO.AddComponent<KeybindRow>();
            keybindRow.ActionName = action;
            keybindRow.ValueText = valueText;
            keybindRow.DeviceKind = InputManager.DeviceKind.KeyboardMouse;

            // Override the AccessibleButton's spoken label to "Rebind X" -
            // its default would just read the on-screen "Rebind" text,
            // identical for every row, which is useless to a screen-reader
            // user trying to tell rows apart.
            var accessible = rowBtnGO.GetComponent<AccessibleButton>();
            var accessibleSo = new SerializedObject(accessible);
            accessibleSo.FindProperty("label").stringValue = $"Rebind {action}";
            accessibleSo.ApplyModifiedProperties();

            y -= rowSpacing;
        }
    }

    private static AudioRefs BuildAudioTab(Transform parent, DefaultControls.Resources resources)
    {
        float y = -230f;
        const float rowSpacing = 80f;
        var refs = new AudioRefs();

        (_, refs.master) = CreateSettingSlider("MasterVolume", "Master Volume", parent, resources, y, 0f, 1f, false); y -= rowSpacing;
        (_, refs.music) = CreateSettingSlider("MusicVolume", "Music Volume", parent, resources, y, 0f, 1f, false); y -= rowSpacing;
        (_, refs.sfx) = CreateSettingSlider("SfxVolume", "SFX Volume", parent, resources, y, 0f, 1f, false); y -= rowSpacing;
        (_, refs.dialogue) = CreateSettingSlider("DialogueVolume", "Dialogue Volume", parent, resources, y, 0f, 1f, false);

        return refs;
    }

    private static AccessibilityRefs BuildAccessibilityTab(Transform parent, DefaultControls.Resources resources)
    {
        float y = -230f;
        const float rowSpacing = 70f;
        var refs = new AccessibilityRefs();

        var colorblindLabels = new[] { "None", "Protanopia", "Deuteranopia", "Tritanopia", "High Contrast Monochrome" };
        (_, refs.colorblindMode) = CreateSettingSlider("ColorblindMode", "Colorblind Mode", parent, resources, y, 0f, 4f, true, colorblindLabels); y -= rowSpacing;
        (_, refs.highContrastOutlines) = CreateSettingToggle("HighContrastOutlines", "High-Contrast Outlines", parent, resources, y); y -= rowSpacing;
        (_, refs.cameraBob) = CreateSettingToggle("CameraBob", "Camera Bob", parent, resources, y); y -= rowSpacing;
        (_, refs.screenShake) = CreateSettingSlider("ScreenShake", "Screen Shake", parent, resources, y, 0f, 1f, false); y -= rowSpacing;
        (_, refs.reduceMotion) = CreateSettingToggle("ReduceMotion", "Reduce Motion", parent, resources, y); y -= rowSpacing;
        (_, refs.audioDescription) = CreateSettingToggle("AudioDescription", "Audio Description (cutscenes)", parent, resources, y); y -= rowSpacing;
        (_, refs.subtitles) = CreateSettingToggle("Subtitles", "Subtitles", parent, resources, y);

        return refs;
    }
}
