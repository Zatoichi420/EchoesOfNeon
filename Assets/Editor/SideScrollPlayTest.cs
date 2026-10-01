// Real Play Mode test of the side-scroller prototype. Drives Marcus through his public command methods (not a
// virtual keyboard - the input plumbing is already covered by PlayModeMovementTest) and checks: walking, jumping,
// the cane edge sensor at the pit, enemy wind-up -> fend deflects it, a 3-hit cane combo kills, and the whistle
// sends Cookie to bite another enemy. Run WITHOUT -quit/-nographics:
//   Unity -batchmode -projectPath <path> -executeMethod SideScrollPlayTest.Run -logFile <log>
using System.Collections.Generic;
using EchoesOfNeon.SideScroll;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SideScrollPlayTest
{
    private static double _start = -1, _phaseStart; private static int _phase;
    private static bool _finished, _pass = true, _prevOpts; private static EnterPlayModeOptions _prevOptions;
    private static SideScrollPlayer P; private static SideScrollEnemy A, B; private static SideScrollCookie C; private static SideScrollEdgeSensor E;
    private static float _x0, _y0, _hp0; private static double _windUpSeen = -1;
    private static readonly List<string> Results = new List<string>();

    public static void Run()
    {
        _prevOpts = EditorSettings.enterPlayModeOptionsEnabled; _prevOptions = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
        EditorSceneManager.OpenScene(SideScrollSceneSetup.ScenePath, OpenSceneMode.Single);
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

    private static void Check(string name, bool ok, string detail) { Results.Add($"{(ok ? "PASS" : "FAIL")} {name}: {detail}"); Debug.Log($"[SSTest] {(ok ? "PASS" : "FAIL")} {name}: {detail}"); if (!ok) _pass = false; }
    private static void Next() { _phase++; _phaseStart = Time.realtimeSinceStartupAsDouble; }
    private static void Teleport(Component c, float x) { var cc = c.GetComponent<CharacterController>(); if (cc != null) cc.enabled = false; c.transform.position = new Vector3(x, 0.05f, 0); if (cc != null) cc.enabled = true; }

    private static void Tick()
    {
        if (!Application.isPlaying || _finished) return;
        if (_start < 0)
        {
            P = Object.FindFirstObjectByType<SideScrollPlayer>(); if (P == null) { if (EditorApplication.timeSinceStartup > 60) Finish("scene never ready"); return; }
            QualitySettings.vSyncCount = 0; Application.targetFrameRate = 60; Application.runInBackground = true;
            E = P.GetComponent<SideScrollEdgeSensor>(); C = Object.FindFirstObjectByType<SideScrollCookie>();
            foreach (var e in SideScrollEnemy.All) { if (e.name == "Enforcer_A") A = e; if (e.name == "Enforcer_B") B = e; }
            _start = Time.realtimeSinceStartupAsDouble; _phaseStart = _start; return;
        }
        double t = Time.realtimeSinceStartupAsDouble - _phaseStart;
        if (Time.realtimeSinceStartupAsDouble - _start > 90) { Finish("timed out in phase " + _phase); return; }

        switch (_phase)
        {
            case 0: // settle
                if (t > 1.0) { Check("grounded", P.IsGrounded, $"y={P.transform.position.y:0.00}"); _x0 = P.transform.position.x; P.SetMoveInput(1); Next(); } break;
            case 1: // walk right 1 s
                if (t > 1.0) { float d = P.transform.position.x - _x0; P.SetMoveInput(0); Check("walk", d > 3f && Mathf.Abs(P.transform.position.z) < 0.01f, $"moved {d:0.0} m, z={P.transform.position.z:0.000}"); _y0 = P.transform.position.y; P.RequestJump(); Next(); } break;
            case 2: // jump apex
                if (t > 0.35) { float h = P.transform.position.y - _y0; Check("jump", h > 0.8f, $"rose {h:0.00} m"); Next(); } break;
            case 3: // go to the pit edge
                if (t > 0.9) { Teleport(P, 26.5f); Teleport(C, 24.5f); P.SetMoveInput(1); Next(); } break;
            case 4: // approach the pit, edge ticks should start; stop before falling
                if (E.NearestGapDistance >= 0f || P.transform.position.x > 29.6f)
                { Check("edge sensor", E.NearestGapDistance >= 0f, $"gap {E.NearestGapDistance:0.0} m ahead at x={P.transform.position.x:0.0}"); P.SetMoveInput(0); Next(); }
                else if (t > 5) { Check("edge sensor", false, "never detected the pit"); P.SetMoveInput(0); Next(); }
                break;
            case 5: // let ticks play
                if (t > 1.2) { Check("edge ticks audible", E.TicksPlayed > 0, $"ticks={E.TicksPlayed}"); Teleport(P, 5f); Teleport(C, 3.4f); P.SetMoveInput(0); _hp0 = P.Health; Next(); } break;
            case 6: // enemy A (x=12) notices and winds up
                if (A.State == EnemyState.WindUp)
                {
                    if (_windUpSeen < 0) { _windUpSeen = Time.realtimeSinceStartupAsDouble; Check("enemy notices + winds up", true, $"state=WindUp at dist {Mathf.Abs(A.transform.position.x - P.transform.position.x):0.0}"); }
                    if (Time.realtimeSinceStartupAsDouble - _windUpSeen >= 0.3) { P.Fend(); Next(); }
                }
                else if (A.State == EnemyState.Chase && Mathf.Abs(A.transform.position.x - P.transform.position.x) > 1.4f) { }
                if (t > 12 && _windUpSeen < 0) { Check("enemy notices + winds up", false, "never reached WindUp, state=" + A.State); Next(); }
                break;
            case 7: // the strike lands during the fend window
                if (t > 1.2) { Check("fend deflects", P.FendsSucceeded >= 1 && Mathf.Approximately(P.Health, _hp0), $"fends={P.FendsSucceeded}, health {P.Health:0}/{_hp0:0}, enemy state={A.State}"); Next(); } break;
            case 8: // cane combo kills A: face it, strike x4 with cooldown gaps
                { float dx = A.transform.position.x - P.transform.position.x; if (Mathf.Abs(dx) > 1.5f) P.SetMoveInput(Mathf.Sign(dx)); else { P.SetMoveInput(0); P.Strike(); } }
                if (A.IsDown) { Check("cane combo kills", true, $"strikes landed={P.StrikesLanded}"); P.SetMoveInput(0); Next(); }
                else if (t > 10) { Check("cane combo kills", false, $"enemy health {A.Health:0}, state={A.State}"); Next(); }
                break;
            case 9: // whistle: B at x=22, Marcus at ~14 -> within range 16
                if (t > 0.5) { Teleport(P, 17f); Teleport(C, 15.4f); P.SetMoveInput(0); Next(); } break;
            case 10:
                if (t > 0.5) { float bh = B.Health; bool sent = P.Whistle(); Check("whistle sends Cookie", sent && C.State == CookieState.Charging, $"sent={sent}, cookie={C.State}, enemy hp {bh:0}"); Next(); } break;
            case 11:
                if (C.BitesLanded > 0 || t > 6) { Check("Cookie bites", C.BitesLanded > 0 && B.Health < B.maxHealth, $"bites={C.BitesLanded}, enemy hp {B.Health:0}/{B.maxHealth:0}, enemy state={B.State}"); Next(); } break;
            case 12:
                if (C.IsReady == false && (C.State == CookieState.Following || t > 8)) { Check("Cookie returns + cooldown", C.State == CookieState.Following && !C.IsReady, $"state={C.State}, ready={C.IsReady}"); Next(); }
                else if (t > 10) { Check("Cookie returns + cooldown", false, $"state={C.State}, ready={C.IsReady}"); Next(); }
                break;
            case 13:
                { string msg = P.SonarScan(); Check("sonar scan", msg.Contains("enem") || msg == "Clear.", msg); Finish(null); } break;
        }
    }

    private static void Finish(string failReason)
    {
        if (failReason != null) { Debug.Log("[SSTest] FAIL: " + failReason); _pass = false; }
        Debug.Log("[SSTest] RESULT " + (_pass ? "PASS" : "FAIL") + "\n" + string.Join("\n", Results));
        _finished = true; EditorApplication.update -= Tick; EditorApplication.isPlaying = false;
    }
}
