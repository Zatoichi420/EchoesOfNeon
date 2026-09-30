// One-off capture tool for the built main menu (MainMenuUISetup.cs) - renders
// each screen/tab to a PNG via a temporary ScreenSpaceCamera render, since the
// scene's real Canvas is ScreenSpaceOverlay (which ignores cameras entirely
// and only renders in Play Mode's actual screen buffer). Does not save the
// scene afterward - the render-mode swap is capture-only, never persisted.
// Run via:
//   Unity.exe -batchmode -projectPath <path>
//     -executeMethod MenuScreenshotCapture.CaptureAll -quit -logFile <log>
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class MenuScreenshotCapture
{
    private const string OutDir = "/Users/orlandojohnson/Desktop/EchoesOfNeon_ConceptArt/UI_Screens";
    private const int Width = 1920;
    private const int Height = 1080;

    public static void CaptureAll()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity", OpenSceneMode.Single);
        Directory.CreateDirectory(OutDir);

        var canvasGO = GameObject.Find("MenuCanvas");
        var canvas = canvasGO.GetComponent<Canvas>();
        var cameraGO = GameObject.Find("Camera");
        var camera = cameraGO.GetComponent<Camera>();

        // Overlay canvases render straight to the screen buffer and ignore
        // any camera - switch to ScreenSpaceCamera just for this capture so
        // Camera.Render() actually produces the UI.
        var originalMode = canvas.renderMode;
        var originalCamera = canvas.worldCamera;
        var originalPlaneDistance = canvas.planeDistance;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 1f;
        camera.orthographic = false;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 5f;

        // Transform.Find (not GameObject.Find, which skips inactive
        // objects) - OptionsPanel and its tab children start inactive by
        // design, so GameObject.Find can never see them.
        var mainMenuPanel = canvasGO.transform.Find("MainMenuPanel").gameObject;
        var optionsPanel = canvasGO.transform.Find("OptionsPanel").gameObject;
        var controllerPanel = optionsPanel.transform.Find("ControllerPanel").gameObject;
        var keybindPanel = optionsPanel.transform.Find("KeybindPanel").gameObject;
        var audioPanel = optionsPanel.transform.Find("AudioPanel").gameObject;
        var accessibilityPanel = optionsPanel.transform.Find("AccessibilityPanel").gameObject;

        Capture("01_MainMenu.png", () =>
        {
            mainMenuPanel.SetActive(true);
            optionsPanel.SetActive(false);
        }, camera);

        Capture("02_Options_Controller.png", () =>
        {
            mainMenuPanel.SetActive(false);
            optionsPanel.SetActive(true);
            controllerPanel.SetActive(true);
            keybindPanel.SetActive(false);
            audioPanel.SetActive(false);
            accessibilityPanel.SetActive(false);
        }, camera);

        Capture("03_Options_Keybindings.png", () =>
        {
            controllerPanel.SetActive(false);
            keybindPanel.SetActive(true);
        }, camera);

        Capture("04_Options_Audio.png", () =>
        {
            keybindPanel.SetActive(false);
            audioPanel.SetActive(true);
        }, camera);

        Capture("05_Options_Accessibility.png", () =>
        {
            audioPanel.SetActive(false);
            accessibilityPanel.SetActive(true);
        }, camera);

        // Restore, then discard - do not persist the capture-only render
        // mode swap back into the scene file.
        canvas.renderMode = originalMode;
        canvas.worldCamera = originalCamera;
        canvas.planeDistance = originalPlaneDistance;

        Debug.Log("[MenuScreenshotCapture] Done. Not saving scene changes.");
    }

    private static void Capture(string fileName, System.Action setActiveStates, Camera camera)
    {
        setActiveStates();

        Canvas.ForceUpdateCanvases();
        foreach (var rt in Object.FindObjectsByType<RectTransform>(FindObjectsSortMode.None))
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        Canvas.ForceUpdateCanvases();

        var rendTex = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
        camera.targetTexture = rendTex;
        camera.Render();

        var prevActive = RenderTexture.active;
        RenderTexture.active = rendTex;
        var tex = new Texture2D(Width, Height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
        tex.Apply();
        RenderTexture.active = prevActive;

        camera.targetTexture = null;
        Object.DestroyImmediate(rendTex);

        var bytes = tex.EncodeToPNG();
        var path = Path.Combine(OutDir, fileName);
        File.WriteAllBytes(path, bytes);
        Object.DestroyImmediate(tex);

        Debug.Log($"[MenuScreenshotCapture] Wrote {path} ({bytes.Length} bytes)");
    }
}
