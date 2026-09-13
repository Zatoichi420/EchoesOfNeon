// One-time setup script: gives the project a real company name/bundle
// identifier (was still "DefaultCompany" with no identifier ever set - every
// Apple platform requires a real one to build/sign at all) and wires up this
// Mac's standing Apple Developer Team ID for automatic signing. Run via:
//   Unity.app/Contents/MacOS/Unity -batchmode -nographics -projectPath <path>
//     -executeMethod PlatformIdentitySetup.Apply -quit -logFile <log>
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public static class PlatformIdentitySetup
{
    /// <summary>Switching the active build target and building within the
    /// same batchmode process can leave script compilation for the new
    /// target in a state Unity itself reports as "script class layout
    /// incompatible between the editor and the player" - doing the switch as
    /// its own process (forcing a full recompile against the new target's
    /// defines/backend, persisted to Library) before a later, separate build
    /// invocation avoids it.</summary>
    public static void SwitchToTvOS() =>
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.tvOS, BuildTarget.tvOS);

    public static void SwitchToMac() =>
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);

    private const string CompanyName = "Zatoichi420";
    private const string BundleIdentifier = "com.zatoichi420.echoesofneon";
    // Standing Apple Developer Team ID for this Mac (N724Z4725Y), used for
    // every Xcode-signed project on this machine.
    private const string AppleDeveloperTeamID = "N724Z4725Y";

    public static void Apply()
    {
        PlayerSettings.companyName = CompanyName;

        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, BundleIdentifier);
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, BundleIdentifier);
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.tvOS, BundleIdentifier);

        PlayerSettings.iOS.appleDeveloperTeamID = AppleDeveloperTeamID;
        PlayerSettings.iOS.appleEnableAutomaticSigning = true;

        Debug.Log($"[PlatformIdentitySetup] companyName={PlayerSettings.companyName}, "
            + $"bundleId={BundleIdentifier}, team={AppleDeveloperTeamID}");
    }
}
