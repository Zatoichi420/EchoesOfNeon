// Real Play Mode test of Cookie as a guide dog, with an automated "player" driving Marcus through the Input System.
// Scenario: Marcus stands still (Cookie leads off, then Waits) -> Marcus follows (Leading resumes) -> Cookie Arrives
// -> Marcus is teleported far away (Cookie Returns for him, then Leads again). Also checks her 3D audio fired.
// Run WITHOUT -quit / -nographics:  Unity -batchmode -projectPath <path> -executeMethod GuideDogPlayTest.Run -logFile <log>
using System.Collections.Generic;
using System.Linq;
using EchoesOfNeon.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public static class GuideDogPlayTest
{
    private static Keyboard _kb; private static double _start = -1; private static int _phase;
    private static bool _finished, _pass = true, _w;
    private static bool _prevOpts; private static EnterPlayModeOptions _prevOptions;
    private static readonly List<string> Transitions = new List<string>();
    private static string _clipMoving = "", _clipWaiting = ""; private static double _phaseT, _lastChange; private static bool _hooked;

    public static void Run()
    {
        _prevOpts = EditorSettings.enterPlayModeOptionsEnabled; _prevOptions = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
        EditorSceneManager.OpenScene("Assets/Scenes/TestRange_Proto.unity", OpenSceneMode.Single);
        EditorApplication.update += Tick; EditorApplication.playModeStateChanged += OnState;
        EditorApplication.isPlaying = true;
    }

    private static void OnState(PlayModeStateChange s)
    {
        if (s == PlayModeStateChange.EnteredEditMode && _finished)
        {
            EditorSettings.enterPlayModeOptionsEnabled = _prevOpts; EditorSettings.enterPlayModeOptions = _prevOptions;
            EditorApplication.Exit(_pass ? 0 : 1);
        }
    }

    private static string Clip(Animator a) { var c = a.GetCurrentAnimatorClipInfo(0); return c.Length > 0 ? c[0].clip.name.Split('|').Last() : "?"; }

    private static void HoldW(bool w)
    {
        if (w == _w) return; _w = w;
        InputSystem.QueueStateEvent(_kb, w ? new KeyboardState(Key.W) : new KeyboardState()); InputSystem.Update();
    }

    private static void Tick()
    {
        if (!Application.isPlaying || _finished) return;
        var guide = Object.FindFirstObjectByType<CompanionGuide>(); var marcus = Object.FindFirstObjectByType<PlayerAnimationBridge>();
        if (_start < 0)
        {
            if (guide == null || marcus == null || EchoesOfNeon.Core.InputManager.Instance == null) { if (EditorApplication.timeSinceStartup > 60) Fail("scene never became ready"); return; }
            _start = Time.realtimeSinceStartupAsDouble;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            Application.runInBackground = true; QualitySettings.vSyncCount = 0; Application.targetFrameRate = 60;
            foreach (var k in Keyboard.all.ToArray()) InputSystem.RemoveDevice(k);
            _kb = InputSystem.AddDevice<Keyboard>("TestKeyboard");
            return;
        }
        if (!_hooked) { guide.OnStateChanged += (a, b) => { _lastChange = Time.realtimeSinceStartupAsDouble; Transitions.Add($"{a}>{b}"); Debug.Log($"[GuideTest] Cookie {a} -> {b} at t={Time.realtimeSinceStartupAsDouble - _start:0.0}s"); }; _hooked = true; }

        double t = Time.realtimeSinceStartupAsDouble - _start;
        var player = marcus.transform; var audio = guide.GetComponent<CompanionAudio>();
        float dPlayer = Vector3.Distance(new Vector3(player.position.x, 0, player.position.z), new Vector3(guide.transform.position.x, 0, guide.transform.position.z));
        var anim = guide.animator;
        // Cookie's animation once she has been moving for a moment (past the 0.15 s Idle->Walking blend).
        if (guide.CurrentSpeed > 0.1f && Time.realtimeSinceStartupAsDouble - _lastChange > 0.4 && Time.realtimeSinceStartupAsDouble - _start > 0.5) _clipMoving = Clip(anim);

        if (_phase == 0)                       // Marcus idle: Cookie leads off, then must Wait for him
        {
            if (t > 5.0) { _clipWaiting = Clip(anim);
                Debug.Log($"[GuideTest] phase A (Marcus idle): state={guide.State} distToMarcus={dPlayer:0.0} pos={guide.transform.position:F1} clipWhileMoving={_clipMoving} clipNow={_clipWaiting}");
                if (guide.State != GuideState.Waiting) Fail("Cookie should be Waiting for an idle Marcus, was " + guide.State);
                _phase = 1; _phaseT = t; }
        }
        else if (_phase == 1)                  // an automated player that follows Cookie: walks when she waits or is > 2.2 m away
        {
            bool follow = guide.State != GuideState.Arrived && (guide.State == GuideState.Waiting || dPlayer > 2.2f) && player.position.z < 8f;
            HoldW(follow);
            if (guide.State == GuideState.Arrived) { HoldW(false); Debug.Log($"[GuideTest] phase B: Cookie ARRIVED at {guide.transform.position:F1}, Marcus at {player.position:F1} after {t - _phaseT:0.0}s"); _phase = 2; _phaseT = t; }
            else if (t - _phaseT > 30) Fail("Cookie never arrived (state " + guide.State + ")");
        }
        else if (_phase == 2 && t - _phaseT > 1.0)   // Marcus wanders far away: Cookie must come back for him
        {
            var cc = player.GetComponent<CharacterController>(); cc.enabled = false; player.position = new Vector3(0f, 0.1f, -3f); cc.enabled = true;
            Debug.Log($"[GuideTest] phase C: Marcus teleported to {player.position:F1}, Cookie at {guide.transform.position:F1} (distance ~{Vector3.Distance(player.position, guide.transform.position):0.0})");
            _phase = 3; _phaseT = t;
        }
        else if (_phase == 3)
        {
            bool relead = Transitions.Contains("Returning>Leading");
            if (relead || t - _phaseT > 15)
            {
                Debug.Log($"[GuideTest] audio: pawsteps={audio.StepsPlayed} collarWhileMoving={audio.CollarPlays} restRustles={audio.RestRustles}");
                Debug.Log("[GuideTest] transitions: " + string.Join(", ", Transitions));
                bool ok = _pass
                    && new[] { "Leading>Waiting", "Waiting>Leading", "Leading>Arrived", "Arrived>Returning", "Returning>Leading" }.All(Transitions.Contains)
                    && _clipMoving.StartsWith("Walking") && _clipWaiting.StartsWith("Idle")
                    && audio.StepsPlayed >= 6 && audio.CollarPlays >= 3 && audio.RestRustles >= 1;
                Debug.Log("[GuideTest] RESULT " + (ok ? "PASS" : "FAIL")); _pass = ok; Finish();
            }
        }
        if (t > 90) Fail("timed out in phase " + _phase);
    }

    private static void Fail(string why) { Debug.Log("[GuideTest] RESULT FAIL: " + why); _pass = false; Finish(); }
    private static void Finish() { _finished = true; EditorApplication.update -= Tick; EditorApplication.isPlaying = false; }
}
