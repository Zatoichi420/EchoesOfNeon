// One-time setup script: adds the packages Phase 1 needs via Package Manager's
// own dependency resolution, rather than hand-pinning version numbers in
// manifest.json that might not actually exist/resolve. Run via:
//   Unity.exe -batchmode -nographics -projectPath <path> -executeMethod PackageSetup.InstallPhase1Packages -logFile <log>
// Deliberately does NOT pass -quit on that invocation - this script calls
// EditorApplication.Exit itself once the (async) package operation truly
// finishes, since -quit would otherwise race ahead of Client.AddAndRemove
// completing.
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public static class PackageSetup
{
    private static AddAndRemoveRequest _request;

    public static void InstallPhase1Packages()
    {
        // com.unity.textmeshpro is deliberately excluded - as a standalone
        // package it's incompatible with this Unity version (6000.6.0f1);
        // TMP functionality now ships as part of the core UI packages
        // instead and doesn't need a separate add.
        var toAdd = new[]
        {
            "com.unity.inputsystem",
            "com.unity.render-pipelines.universal",
        };
        Debug.Log("[PackageSetup] Requesting packages: " + string.Join(", ", toAdd));
        _request = Client.AddAndRemove(packagesToAdd: toAdd);
        EditorApplication.update += Tick;
    }

    /// <summary>Phase 5: SoundVisualizerCompass needs UnityEngine.UI
    /// (Image/CanvasScaler) - com.unity.modules.ui alone only provides the
    /// base Canvas/RectTransform rendering infra, not those classes.</summary>
    public static void InstallPhase5Packages()
    {
        var toAdd = new[] { "com.unity.ugui" };
        Debug.Log("[PackageSetup] Requesting packages: " + string.Join(", ", toAdd));
        _request = Client.AddAndRemove(packagesToAdd: toAdd);
        EditorApplication.update += Tick;
    }

    private static void Tick()
    {
        if (!_request.IsCompleted)
            return;

        EditorApplication.update -= Tick;

        if (_request.Status == StatusCode.Success)
        {
            Debug.Log("[PackageSetup] SUCCESS: all packages added.");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError("[PackageSetup] FAILED: " + _request.Error?.message);
            EditorApplication.Exit(1);
        }
    }
}
