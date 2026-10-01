// Real Play Mode test of the side-scroller prototype (movement, cane, fend, Cookie, health pickups, rest points, XP /
// levels, respawn, exploration, augments, strain, and every street-enemy archetype). Written as a coroutine driven by
// EditorApplication.update so each step reads top-to-bottom. Drives Marcus through his public command methods.
// Run WITHOUT -quit/-nographics:
//   Unity -batchmode -projectPath <path> -executeMethod SideScrollPlayTest.Run -logFile <log>
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EchoesOfNeon.SideScroll;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SideScrollPlayTest
{
    private static readonly Stack<IEnumerator> _stack = new Stack<IEnumerator>(); private static bool _finished, _pass = true, _prevOpts, _started;
    private static EnterPlayModeOptions _prevOptions; private static double _t0;
    private static SideScrollPlayer P; private static SideScrollCookie C; private static SideScrollEdgeSensor E; private static SideScrollAbilities AB; private static SideScrollProgression PR;
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
    private static IEnumerator Wait(float s) { double end = Time.realtimeSinceStartupAsDouble + s; while (Time.realtimeSinceStartupAsDouble < end) yield return null; }
    private static IEnumerator Until(Func<bool> cond, float timeout) { double end = Time.realtimeSinceStartupAsDouble + timeout; while (!cond() && Time.realtimeSinceStartupAsDouble < end) yield return null; }
    private static void Teleport(Component c, float x, float y = 0.05f) { var cc = c.GetComponent<CharacterController>(); if (cc != null) cc.enabled = false; c.transform.position = new Vector3(x, y, 0); if (cc != null) cc.enabled = true; }
    private static SideScrollEnemy Foe(string name) { var f = SideScrollEnemy.All.FirstOrDefault(e => e.name == name); if (f == null) throw new InvalidOperationException("no enemy named " + name + "; have: " + string.Join(",", SideScrollEnemy.All.Select(e => e.name))); return f; }
    private static T Obj<T>(string name) where T : Component { var o = UnityEngine.Object.FindObjectsByType<T>(FindObjectsSortMode.None).FirstOrDefault(x => x.name == name); if (o == null) throw new InvalidOperationException("no " + typeof(T).Name + " named " + name + "; locators now: " + string.Join(",", UnityEngine.Object.FindObjectsByType<LocatorObject>(FindObjectsSortMode.None).Select(l => l.name + "@" + l.transform.position.x)) + " playerX=" + P.transform.position.x); return o; }

    private static void Tick()
    {
        if (!Application.isPlaying || _finished) return;
        if (!_started)
        {
            P = UnityEngine.Object.FindFirstObjectByType<SideScrollPlayer>(); PR = SideScrollProgression.Instance;
            if (P == null || PR == null) { if (EditorApplication.timeSinceStartup > 60) Finish("scene never ready"); return; }
            QualitySettings.vSyncCount = 0; Application.targetFrameRate = 60; Application.runInBackground = true;
            PR.persist = false; PR.marcus = new LevelTrack(); PR.cookie = new LevelTrack();      // never touch the real save
            E = P.GetComponent<SideScrollEdgeSensor>(); AB = P.GetComponent<SideScrollAbilities>(); C = UnityEngine.Object.FindFirstObjectByType<SideScrollCookie>();
            _started = true; _t0 = Time.realtimeSinceStartupAsDouble; _stack.Push(Script()); return;
        }
        if (Time.realtimeSinceStartupAsDouble - _t0 > 170) { Finish("timed out"); return; }
        try
        {
            if (_stack.Count == 0) { Finish(null); return; }
            var top = _stack.Peek();
            if (!top.MoveNext()) { _stack.Pop(); if (_stack.Count == 0) Finish(null); }
            else if (top.Current is IEnumerator nested) _stack.Push(nested);
        }
        catch (Exception ex) { Debug.Log("[SSTest] EXCEPTION " + ex); Finish("exception: " + ex.Message); }
    }

    private static IEnumerator Script()
    {
        // ---------------- basics ----------------
        Debug.Log("[SSTest] locators at start: " + string.Join(",", UnityEngine.Object.FindObjectsByType<LocatorObject>(FindObjectsSortMode.None).Select(l => l.GetType().Name + ":" + l.name)));
        yield return Wait(1f);
        Check("grounded", P.IsGrounded, $"y={P.transform.position.y:0.00}");
        float x0 = P.transform.position.x; P.SetMoveInput(1); yield return Wait(1f); P.SetMoveInput(0);
        Check("walk", P.transform.position.x - x0 > 3f && Mathf.Abs(P.transform.position.z) < 0.01f, $"moved {P.transform.position.x - x0:0.0} m");
        float y0 = P.transform.position.y; P.RequestJump(); yield return Wait(0.35f);
        Check("jump", P.transform.position.y - y0 > 0.8f, $"rose {P.transform.position.y - y0:0.00} m"); yield return Wait(0.9f);

        // ---------------- edge sensor ----------------
        Teleport(P, 26.5f); Teleport(C, 24.5f); P.SetMoveInput(1);
        yield return Until(() => E.NearestGapDistance >= 0f || P.transform.position.x > 29.6f, 5f); P.SetMoveInput(0);
        Check("edge sensor", E.NearestGapDistance >= 0f, $"gap {E.NearestGapDistance:0.0} m ahead at x={P.transform.position.x:0.0}");
        yield return Wait(1.2f); Check("edge ticks audible", E.TicksPlayed > 0, $"ticks={E.TicksPlayed}");

        // ---------------- combat: fend + combo + XP ----------------
        Teleport(P, 5f); Teleport(C, 3.4f); float hp0 = P.Health; var A = Foe("Enforcer_A");
        yield return Until(() => A.State == EnemyState.WindUp, 12f);
        Check("enemy notices + winds up", A.State == EnemyState.WindUp, $"state={A.State}");
        yield return Wait(0.3f); P.Fend(); yield return Wait(1.2f);
        Check("fend deflects", P.FendsSucceeded >= 1 && Mathf.Approximately(P.Health, hp0), $"fends={P.FendsSucceeded}, health {P.Health:0}/{hp0:0}");
        Check("fend awards xp", PR.marcus.xp >= 3, $"xp={PR.marcus.xp}");
        double cEnd = Time.realtimeSinceStartupAsDouble + 10;
        while (!A.IsDown && Time.realtimeSinceStartupAsDouble < cEnd)
        {
            float dx = A.transform.position.x - P.transform.position.x;
            if (Mathf.Abs(dx) > 1.5f) P.SetMoveInput(Mathf.Sign(dx)); else { P.SetMoveInput(0); P.Strike(); }
            yield return null;
        }
        P.SetMoveInput(0);
        Check("cane combo kills", A.IsDown, $"strikes landed={P.StrikesLanded}, A hp={A.Health:0} state={A.State} dx={A.transform.position.x - P.transform.position.x:0.0} facing={P.Facing} playerHp={P.Health:0}");
        Check("kill awards xp", PR.marcus.xp >= 20, $"xp={PR.marcus.xp}, cookie xp={PR.cookie.xp}");
        Check("special meter fills", AB.SpecialMeter > 10f, $"special={AB.SpecialMeter:0}");

        // ---------------- whistle ----------------
        Teleport(P, 17f); Teleport(C, 15.4f); yield return Wait(0.5f); var B = Foe("Enforcer_B"); float cookieXp0 = PR.cookie.xp;
        bool sent = P.Whistle(); Check("whistle sends Cookie", sent && C.State == CookieState.Charging, $"sent={sent}");
        yield return Until(() => C.BitesLanded > 0, 6f);
        Check("Cookie bites", C.BitesLanded > 0 && B.Health < B.maxHealth, $"bites={C.BitesLanded}, enemy hp {B.Health:0}/{B.maxHealth:0}");
        Check("Cookie earns xp", PR.cookie.xp > cookieXp0, $"cookie xp {cookieXp0} -> {PR.cookie.xp}");
        yield return Until(() => C.State == CookieState.Following, 8f);
        Check("Cookie returns + cooldown", C.State == CookieState.Following && !C.IsReady, $"state={C.State}, ready={C.IsReady}");
        Check("sonar scan", P.SonarScan().Length > 0, "ok");

        // ---------------- health pickups ----------------
        var med = Obj<HealthPickup>("MedCell_1");
        P.TakeUnavoidable(50f); float hurt = P.Health; Teleport(P, 8f); yield return Wait(0.4f);
        Check("med-cell heals", P.Health >= hurt + 24f && med == null || P.Health >= hurt + 24f, $"health {hurt:0} -> {P.Health:0}");
        Check("med-cell consumed", med == null, med == null ? "gone" : "still there");
        var med2 = Obj<HealthPickup>("MedCell_2"); P.Heal(999f); float full = P.Health; Teleport(P, 38f); yield return Wait(0.4f);
        Check("med-cell kept when full", med2 != null && Mathf.Approximately(P.Health, full), $"health {P.Health:0}, pickup {(med2 != null ? "kept" : "wasted")}");

        // ---------------- rest point + respawn ----------------
        var rest = Obj<RestPoint>("RestPoint_Workshop"); P.TakeUnavoidable(60f); AB.AddStrain(50f); Teleport(P, 2.5f); yield return Wait(0.3f);
        float low = P.Health; rest.Interact(); yield return Wait(0.2f);
        Check("rest point restores", Mathf.Approximately(P.Health, P.MaxHealth) && AB.Strain == 0f && low < P.MaxHealth, $"health {low:0} -> {P.Health:0}, strain {AB.Strain:0}");
        Teleport(P, 20f); yield return Wait(0.2f); P.TakeUnavoidable(9999f); bool down = P.IsDead;
        yield return Until(() => P.Respawns > 0, 4f); yield return Wait(0.3f);
        Check("down + respawn at rest point", down && P.Respawns == 1 && !P.IsDead && Mathf.Abs(P.transform.position.x - 2.5f) < 1f && P.Health > 0f,
            $"respawns={P.Respawns}, x={P.transform.position.x:0.0}, health={P.Health:0}/{P.MaxHealth:0}");

        // ---------------- exploration xp ----------------
        var beacon = Obj<ExplorationBeacon>("Beacon_Alley"); int before = PR.marcus.xp; Teleport(P, 15f); yield return Wait(0.4f);
        Check("exploration awards xp", beacon.GetComponent<MeshRenderer>() != null && PR.marcus.xp != before, $"xp {before} -> {PR.marcus.xp} (level {PR.MarcusLevel})");

        // ---------------- levels ----------------
        PR.marcus = new LevelTrack(); PR.cookie = new LevelTrack(); float maxHp1 = P.MaxHealth; int ups = 0; PR.OnMarcusLevelUp += l => ups++;
        PR.AwardMarcusXp(60);
        Check("level up", PR.MarcusLevel == 2 && ups == 1 && PR.Has(MarcusAbility.EchoStep) && P.MaxHealth > maxHp1 - 0.01f, $"level {PR.MarcusLevel}, max hp {maxHp1:0}->{P.MaxHealth:0}");
        Check("clue per level", PR.LastClue == LoreClues.ForLevel(2) && PR.LastClue.Length > 20, PR.LastClue);
        PR.AwardMarcusXp(100000); PR.AwardCookieXp(100000);
        Check("level cap 10", PR.MarcusLevel == 10 && PR.CookieLevel == 10, $"marcus {PR.MarcusLevel}, cookie {PR.CookieLevel}");
        P.Heal(999f); AB.Refresh(); Teleport(P, 47f, 2.6f); Teleport(C, 45.5f); yield return Wait(0.5f);

        // ---------------- augments ----------------
        float ex = P.transform.position.x; P.SetMoveInput(1); P.EchoStep(); yield return Wait(0.1f);
        Check("Echo Step dodges", AB.IsInvulnerable, "invulnerable during dash"); yield return Wait(0.4f); P.SetMoveInput(0);
        Check("Echo Step dashes", P.transform.position.x - ex > 2.5f, $"moved {P.transform.position.x - ex:0.0} m");
        Teleport(P, 47f, 2.6f); AB.ClearStrain();
        AB.AddStrain(150f); Check("overstrain is a real cost", AB.IsOverstrained && AB.Strain < 100f && P.Health < P.MaxHealth, $"strain {AB.Strain:0}, overstrained={AB.IsOverstrained}");
        yield return Wait(4.2f);

        // ---------------- street enemies ----------------
        P.Heal(999f); AB.ClearStrain();
        var seen = new Dictionary<string, string>();
        foreach (var kv in new[] { ("KnifeRunner_1", 40f), ("BottleThrower_1", 52f), ("ShieldGuard_1", 60f), ("ZipGunPunk_1", 66f), ("AlleyRat_1", 72f), ("DroneHawk_1", 80f), ("BruteBouncer_1", 86f) })
        {
            var en = Foe(kv.Item1); Teleport(P, (kv.Item1.StartsWith("AlleyRat") || kv.Item1.StartsWith("Knife")) ? kv.Item2 + 5f : kv.Item2 - 6.5f); Teleport(C, kv.Item2 - 8f);
            P.TakeUnavoidable(0f); P.Heal(999f);
            bool reached = false; EnemyState last = en.State; double end = Time.realtimeSinceStartupAsDouble + 9;
            while (Time.realtimeSinceStartupAsDouble < end && !reached) { if (SideScrollEnemy.All.Any(o => o.kind == en.kind && (o.State == EnemyState.WindUp || o.State == EnemyState.Strike))) reached = true; else yield return null; }
            Check("enemy archetype " + en.kind, reached, $"{en.kind} state={en.State} hp={en.Health:0} dx={en.transform.position.x - P.transform.position.x:0.0} playerX={P.transform.position.x:0.0} playerHp={P.Health:0}");
            en.Stagger(0.1f);
        }
        // shield guard: light hit from its front is blocked, a heavy (Titan) hit lands and breaks the guard
        var sg = Foe("ShieldGuard_1"); Teleport(P, 60f - 2f); yield return Wait(0.3f);
        float shp = sg.Health; bool blocked = sg.Hit(10f, sg.transform.position.x + sg.Facing * 2f, false, false);
        Check("shield guard blocks light front hit", blocked && sg.Health == shp, $"blocked={blocked}, hp {shp:0}->{sg.Health:0}");
        sg.Hit(10f, sg.transform.position.x + sg.Facing * 2f, true, true);
        Check("heavy hit breaks the guard", sg.Health < shp && sg.GuardBroken, $"hp {sg.Health:0}, guardBroken={sg.GuardBroken}");

        // ---------------- specials ----------------
        Teleport(P, 40f); yield return Wait(0.2f); AB.AddSpecial(100f);
        var kr = Foe("KnifeRunner_1"); kr.Hit(0f, 0, false, false); Teleport(kr, 43f);
        AB.CycleSpecial(); bool fired = AB.UseSpecial();
        Check("special fires and spends meter", fired && AB.SpecialMeter < 100f, $"fired={fired}, meter {AB.SpecialMeter:0}, selected {AB.Selected}");
    }

    private static void Finish(string failReason)
    {
        if (failReason != null) { Debug.Log("[SSTest] FAIL: " + failReason); _pass = false; }
        Debug.Log("[SSTest] RESULT " + (_pass ? "PASS" : "FAIL") + "\n" + string.Join("\n", Results));
        _finished = true; EditorApplication.update -= Tick; EditorApplication.isPlaying = false;
    }
}
