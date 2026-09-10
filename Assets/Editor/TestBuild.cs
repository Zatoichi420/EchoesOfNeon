using UnityEditor;
using UnityEditor.Build.Reporting;

public static class TestBuild
{
    public static void BuildWindowsTest()
    {
        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/TestRange_Proto.unity" },
            locationPathName = "Builds/TestRange/EchoesOfNeon_Test.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        UnityEngine.Debug.Log("[TestBuild] Build result: " + report.summary.result
            + " | errors=" + report.summary.totalErrors
            + " | warnings=" + report.summary.totalWarnings
            + " | size=" + report.summary.totalSize + " bytes");
    }
}
