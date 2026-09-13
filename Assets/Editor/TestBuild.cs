using UnityEditor;
using UnityEditor.Build.Reporting;

public static class TestBuild
{
    public static void BuildWindowsTest()
    {
        Run(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/TestRange_Proto.unity" },
            locationPathName = "Builds/TestRange/EchoesOfNeon_Test.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
        });
    }

    /// <summary>macOS Standalone - produces a directly-runnable .app, same as
    /// the Windows .exe (no separate Xcode step, unlike iOS/tvOS below).</summary>
    public static void BuildMacTest()
    {
        Run(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/TestRange_Proto.unity" },
            locationPathName = "Builds/TestRange-Mac/EchoesOfNeon_Test.app",
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None,
        });
    }

    /// <summary>tvOS - like all Apple mobile/TV platforms, this produces an
    /// Xcode project (not a runnable binary); it still needs `xcodebuild` (or
    /// Xcode itself) to actually compile and deploy it to a device/simulator.</summary>
    public static void BuildTvOSTest()
    {
        Run(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/TestRange_Proto.unity" },
            locationPathName = "Builds/TestRange-tvOS",
            target = BuildTarget.tvOS,
            options = BuildOptions.None,
        });
    }

    private static void Run(BuildPlayerOptions options)
    {
        BuildReport report = BuildPipeline.BuildPlayer(options);
        UnityEngine.Debug.Log("[TestBuild] Build result: " + report.summary.result
            + " | errors=" + report.summary.totalErrors
            + " | warnings=" + report.summary.totalWarnings
            + " | size=" + report.summary.totalSize + " bytes");
    }
}
