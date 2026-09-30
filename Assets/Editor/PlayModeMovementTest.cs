// End-to-end test in real Play Mode: launches TestRange_Proto, presses W / W+Shift on a virtual keyboard through
// the Input System, and checks the player really moves and Marcus's body plays Idle/Walk/Run.
// Run WITHOUT -quit / -nographics:
//   Unity -batchmode -projectPath <path> -executeMethod PlayModeMovementTest.Run -logFile <log>
using System.IO;
using System.Linq;
using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;

public static class PlayModeMovementTest
{
    private static Keyboard _kb; private static double _start = -1; private static int _phase;
    private static bool _finished, _pass = true; private static string _out;
    private static Vector3 _p0; private static string _clipIdle = "", _clipWalk = "", _clipRun = "", _clipStop = "";
    private static float _walkSpeed, _runSpeed; private static double _lastLog;
    private static bool _prevOpts; private static EnterPlayModeOptions _prevOptions;

    public static void Run()
    {
        _out = "/Users/orlandojohnson/Desktop/EchoesOfNeon_ConceptIdeas/Characters_3D";
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

    private static string Clip(Animator a)
    {
        var c = a.GetCurrentAnimatorClipInfo(0); var n = a.GetNextAnimatorClipInfo(0);
        return (c.Length > 0 ? c[0].clip.name.Split('|').Last() : "?") + (n.Length > 0 ? "->" + n[0].clip.name.Split('|').Last() : "");
    }

    private static void Tick()
    {
        if (!Application.isPlaying || _finished) return;
        if (_start < 0)
        {
            var bridge0 = Object.FindFirstObjectByType<PlayerAnimationBridge>();
            if (bridge0 == null || InputManager0() == null) { if (EditorApplication.timeSinceStartup > 60) Fail("scene never became ready"); return; }
            _start = Time.realtimeSinceStartupAsDouble;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            // Batch mode never has application focus; without these the Input System drops every event.
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            Application.runInBackground = true;
            // Batch mode otherwise runs at ~5000 fps, where walking's per-frame step (4 m/s * 0.0002 s = 0.0008 m) is under
            // CharacterController.minMoveDistance (0.001 m) and is silently dropped; a real player runs at 60+ fps.
            QualitySettings.vSyncCount = 0; Application.targetFrameRate = 60;
            // Two keyboards bound to the same action makes the Input System arbitrate between them; keep only ours.
            foreach (var k in Keyboard.all.ToArray()) InputSystem.RemoveDevice(k);
            _kb = InputSystem.AddDevice<Keyboard>("TestKeyboard");
            var im = EchoesOfNeon.Core.InputManager.Instance;
            im.OnMove += v => Debug.Log($"[PlayEvt] OnMove {v} (t={Time.realtimeSinceStartupAsDouble - _start:0.00})");
            im.OnSprintPressed += () => Debug.Log($"[PlayEvt] OnSprintPressed (t={Time.realtimeSinceStartupAsDouble - _start:0.00})");
            im.OnSprintReleased += () => Debug.Log($"[PlayEvt] OnSprintReleased (t={Time.realtimeSinceStartupAsDouble - _start:0.00})");
            Debug.Log($"[PlayTest] devices: {string.Join(", ", InputSystem.devices.Select(d => d.name))} | focus={Application.isFocused}");
            Debug.Log("[PlayTest] started; body renderers shadow mode = " +
                string.Join(",", bridge0.GetComponentInChildren<PlayerBodyVisibility>() ? new[] { bridge0.GetComponent<PlayerBodyVisibility>().FirstPerson.ToString() } : new[] { "n/a" }));
            return;
        }
        double t = Time.realtimeSinceStartupAsDouble - _start;
        if (t - _lastLog >= 0.5)
        {
            _lastLog = t; var cc = Object.FindFirstObjectByType<PlayerAnimationBridge>().GetComponent<CharacterController>();
            var b0 = Object.FindFirstObjectByType<PlayerAnimationBridge>();
            Debug.Log($"[PlayTrace] t={t:0.0} frame={Time.frameCount} dt={Time.unscaledDeltaTime:0.000} timeScale={Time.timeScale} pos={b0.transform.position:F2} vel={cc.velocity.magnitude:0.00} grounded={cc.isGrounded} sprint={AccessibilityManager.Instance.IsSprinting} bridgeSpeed={b0.SpeedParameter:0.0} animSpeed={b0.animator.GetFloat("Speed"):0.0} clip={Clip(b0.animator)}");
        }
        var bridge = Object.FindFirstObjectByType<PlayerAnimationBridge>(); var player = bridge.transform; var anim = bridge.animator;
        var vis = bridge.GetComponent<PlayerBodyVisibility>(); var acc = AccessibilityManager.Instance;

        var cc2 = player.GetComponent<CharacterController>();
        if (_phase == 0 && t > 2.0) { _clipIdle = Clip(anim); Debug.Log($"[PlayTest] settled: pos={player.position:F2} clip={_clipIdle} Speed={bridge.SpeedParameter:0.0}");
            InputSystem.QueueStateEvent(_kb, new KeyboardState(Key.W)); InputSystem.Update(); _phase = 1; }
        else if (_phase == 1 && t > 2.1) { Debug.Log($"[PlayTest] 0.3s after FIRST W event: vel={cc2.velocity.magnitude:0.00} pos={player.position:F2}");
            InputSystem.QueueStateEvent(_kb, new KeyboardState(Key.W)); InputSystem.Update(); _phase = 2; }   // re-send: first event may be dropped
        else if (_phase == 2 && t > 2.6) { _clipWalk = Clip(anim); _walkSpeed = cc2.velocity.magnitude;
            Debug.Log($"[PlayTest] walking: pos={player.position:F2} speed={_walkSpeed:0.00} m/s clip={_clipWalk} Speed={bridge.SpeedParameter:0.0} sprint={acc.IsSprinting} dt={Time.unscaledDeltaTime:0.0000}");
            InputSystem.QueueStateEvent(_kb, new KeyboardState(Key.W, Key.LeftShift)); InputSystem.Update(); _phase = 3; }
        else if (_phase == 3 && t > 3.0)
        {
            // Third-person snapshot mid-sprint (first-person hides the body from the player camera by design).
            vis.SetFirstPerson(false);
            var cam = new GameObject("SnapCam").AddComponent<Camera>(); cam.fieldOfView = 45;
            cam.transform.position = player.position - player.forward * 3.2f + player.right * 1.4f + Vector3.up * 1.5f;
            cam.transform.LookAt(player.position + Vector3.up * 1.0f);
            var rt = new RenderTexture(960, 540, 24); cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
            var tex = new Texture2D(960, 540, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, 960, 540), 0, 0); tex.Apply();
            File.WriteAllBytes(Path.Combine(_out, "PlayMode_Marcus_sprint.png"), tex.EncodeToPNG());
            RenderTexture.active = null; cam.targetTexture = null; Object.Destroy(cam.gameObject); vis.SetFirstPerson(true);
            _phase = 4;
        }
        else if (_phase == 4 && t > 3.2) { _clipRun = Clip(anim); _runSpeed = cc2.velocity.magnitude;
            Debug.Log($"[PlayTest] sprinting: pos={player.position:F2} speed={_runSpeed:0.00} m/s clip={_clipRun} Speed={bridge.SpeedParameter:0.0} sprint={acc.IsSprinting}");
            InputSystem.QueueStateEvent(_kb, new KeyboardState()); InputSystem.Update(); _phase = 5; }
        else if (_phase == 5 && t > 4.4) { _clipStop = Clip(anim);
            Debug.Log($"[PlayTest] stopped: pos={player.position:F2} clip={_clipStop} Speed={bridge.SpeedParameter:0.0}");
            var shadowOnly = bridge.GetComponentsInChildren<Renderer>().All(r => r.shadowCastingMode == ShadowCastingMode.ShadowsOnly);
            bool ok = _clipIdle.StartsWith("Idle") && _clipWalk.StartsWith("Walk") && _clipRun.StartsWith("Run") && _clipStop.StartsWith("Idle")
                      && _walkSpeed > 2f && _runSpeed > _walkSpeed + 1f && shadowOnly;
            Debug.Log($"[PlayTest] first-person body shadows-only={shadowOnly}");
            Debug.Log("[PlayTest] RESULT " + (ok ? "PASS" : "FAIL") + $" (idle='{_clipIdle}' walk='{_clipWalk}' run='{_clipRun}' stop='{_clipStop}' walkSpeed={_walkSpeed:0.0} runSpeed={_runSpeed:0.0})"); _pass = ok; Finish(); }
        else if (t > 30) Fail("timed out in phase " + _phase);
    }

    private static object InputManager0() => EchoesOfNeon.Core.InputManager.Instance;
    private static void Fail(string why) { Debug.Log("[PlayTest] RESULT FAIL: " + why); _pass = false; Finish(); }
    private static void Finish() { _finished = true; EditorApplication.update -= Tick; EditorApplication.isPlaying = false; }
}
