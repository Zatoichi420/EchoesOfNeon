using System.Collections.Generic;
using EchoesOfNeon.Accessibility;
using UnityEngine;

namespace EchoesOfNeon.SideScroll
{
    public enum SpecialMove { SonicPulse, FlashBlind, Overclock, ExecutionChain, EchoStorm }

    /// <summary>
    /// Marcus's augment powers. The SPECIAL METER fills as he lands hits (+6), fends (+10) and Cookie bites (+5); a spoken
    /// "special ready" marks the first time the selected special becomes affordable. Using augments builds STRAIN (the
    /// bloodline waking, a REAL cost): above 60 a warning pulse plays, above 85 his fend window shrinks, at 100 he is
    /// Overstrained - he takes damage, loses specials for 4 s and drops back to 70. Strain bleeds off when he rests.
    /// Echo Step is a separate dash on its own key; specials are chosen with CycleSpecial and fired with Special.
    /// </summary>
    [RequireComponent(typeof(SideScrollPlayer))]
    public class SideScrollAbilities : MonoBehaviour
    {
        public float SpecialMeter { get; private set; }
        public float Strain { get; private set; }
        public bool IsOverstrained => _lockout > 0f;
        public bool IsInvulnerable => _invuln > 0f;
        public bool IsOverclocked => _overclock > 0f;
        public bool SonarLocked { get; private set; }
        public SpecialMove? Selected => _unlocked.Count == 0 ? (SpecialMove?)null : _unlocked[_sel % _unlocked.Count];
        public float FendWindowMultiplier => Strain >= 85f ? 0.6f : 1f;
        public float SpeedMultiplier => IsOverclocked ? 1.4f : 1f;
        public float DamageMultiplier => IsOverclocked ? 1.5f : 1f;
        public float StrikeCooldownMultiplier => IsOverclocked ? 0.7f : 1f;

        private SideScrollPlayer _p; private SideScrollProgression _prog; private AudioSource _sfx;
        private readonly List<SpecialMove> _unlocked = new List<SpecialMove>();
        private readonly Dictionary<SpecialMove, float> _cd = new Dictionary<SpecialMove, float>();
        private float _echoCd, _invuln, _overclock, _lockout, _strainIdle, _warnTimer, _beat, _lockTimer;
        private bool _readyAnnounced;

        private static readonly float[] Cost = { 15f, 20f, 40f, 25f, 60f };            // by SpecialMove
        private static readonly float[] Cooldown = { 4f, 8f, 30f, 10f, 45f };
        private static readonly string[] Name = { "Sonic Pulse", "Flash Blind", "Overclock", "Execution Chain", "Echo Storm" };

        private void Awake()
        {
            _p = GetComponent<SideScrollPlayer>();
            var go = new GameObject("AbilitySfx"); go.transform.SetParent(transform, false); _sfx = go.AddComponent<AudioSource>(); EchoesOfNeon.Core.AudioRouting.Bind(_sfx, EchoesOfNeon.Core.AudioCategory.Sfx); _sfx.spatialBlend = 0f;
            foreach (SpecialMove m in System.Enum.GetValues(typeof(SpecialMove))) _cd[m] = 0f;
        }

        private void Start() { _prog = SideScrollProgression.Instance; Refresh(); if (_prog != null) _prog.OnMarcusLevelUp += _ => Refresh(); }

        public void Refresh()
        {
            _unlocked.Clear();
            if (_prog == null) return;
            if (_prog.Has(MarcusAbility.SonicPulse)) _unlocked.Add(SpecialMove.SonicPulse);
            if (_prog.Has(MarcusAbility.FlashBlind)) _unlocked.Add(SpecialMove.FlashBlind);
            if (_prog.Has(MarcusAbility.Overclock)) _unlocked.Add(SpecialMove.Overclock);
            if (_prog.Has(MarcusAbility.ExecutionChain)) _unlocked.Add(SpecialMove.ExecutionChain);
            if (_prog.Has(MarcusAbility.EchoStorm)) _unlocked.Add(SpecialMove.EchoStorm);
            if (_sel >= _unlocked.Count) _sel = 0;
        }
        private int _sel;

        // ------------------------------------------------------------------ meter + strain

        public void AddSpecial(float amount)
        {
            float before = SpecialMeter; SpecialMeter = Mathf.Min(100f, SpecialMeter + amount);
            var s = Selected;
            if (s.HasValue && before < Cost[(int)s.Value] && SpecialMeter >= Cost[(int)s.Value] && !_readyAnnounced)
            { _readyAnnounced = true; Play(SideScrollTones.SpecialReady, 0.7f); Say("Special ready"); }
        }

        public void AddStrain(float amount)
        {
            float mult = _prog != null ? _prog.StrainCostMultiplier : 1f;
            float before = Strain; Strain = Mathf.Min(100f, Strain + amount * mult); _strainIdle = 0f;
            if (before < 60f && Strain >= 60f) Say("Strain high");
            if (Strain >= 100f)
            {
                _lockout = 4f; Strain = 70f; Play(SideScrollTones.Overstrain, 1f); Say("Overstrained");
                _p.TakeUnavoidable(5f);
            }
        }

        public void ClearStrain() { Strain = 0f; _lockout = 0f; }

        // ------------------------------------------------------------------ actions

        public bool CycleSpecial()
        {
            if (_unlocked.Count == 0) { Say("No specials yet"); return false; }
            _sel = (_sel + 1) % _unlocked.Count; _readyAnnounced = SpecialMeter >= Cost[(int)_unlocked[_sel]];
            Say($"{Name[(int)_unlocked[_sel]]}, costs {Cost[(int)_unlocked[_sel]]:0}"); return true;
        }

        public bool EchoStep(int dir)
        {
            if (_prog == null || !_prog.Has(MarcusAbility.EchoStep) || _echoCd > 0f || _p.IsDead) return false;
            _echoCd = 1.2f; _invuln = 0.3f; _p.Dash(dir, 3.2f);
            Play(SideScrollTones.EchoStep, 0.8f); AddStrain(3f); return true;
        }

        public bool ArmSonarLock()
        {
            if (_prog == null || !_prog.Has(MarcusAbility.SonarLock)) return false;
            SonarLocked = true; _lockTimer = 6f; Play(SideScrollTones.LockOn, 0.8f); return true;
        }

        /// <summary>Consumes the armed Sonar Lock (called by Strike): the next hit lands as a critical.</summary>
        public float ConsumeCrit() { if (!SonarLocked) return 1f; SonarLocked = false; return 2f; }

        public bool UseSpecial()
        {
            var s = Selected;
            if (!s.HasValue) { Say("No special equipped"); return false; }
            if (_p.IsDead) return false;
            if (IsOverstrained) { Say("Overstrained"); return false; }
            var m = s.Value; int i = (int)m;
            if (_cd[m] > 0f) { Say("Cooling down"); return false; }
            if (SpecialMeter < Cost[i]) { Say("Not enough special"); return false; }
            SpecialMeter -= Cost[i]; _cd[m] = Cooldown[i]; _readyAnnounced = SpecialMeter >= Cost[i];
            float dmg = _p.strikeDamage * _p.DamageScale;
            switch (m)
            {
                case SpecialMove.SonicPulse:
                    Play(SideScrollTones.SonicPulse, 1f);
                    foreach (var e in Foes(6f, frontOnly: true)) { e.Hit(8f * _p.DamageScale, _p.transform.position.x, true, false); e.Stagger(1.5f); }
                    AddStrain(8f); break;
                case SpecialMove.FlashBlind:
                    Play(SideScrollTones.FlashBlind, 1f);
                    foreach (var e in Foes(7f, false)) e.Stagger(2.5f);
                    AddStrain(12f); break;
                case SpecialMove.Overclock:
                    _overclock = 6f; _beat = 0f; Play(SideScrollTones.Overclock, 1f); AddStrain(30f); break;
                case SpecialMove.ExecutionChain:
                    {
                        var t = NearestFront(2.8f);
                        if (t != null) for (int k = 0; k < 3; k++) { t.Hit(dmg * 1.2f, _p.transform.position.x, true, false); PlayPitched(SideScrollTones.ChainHit, 1f, 1f + 0.25f * k); }
                        if (t != null) t.Stagger(1.2f);
                        _p.TryCookieTandem();
                        AddStrain(10f); break;
                    }
                case SpecialMove.EchoStorm:
                    Play(SideScrollTones.EchoStorm, 1f);
                    foreach (var e in Foes(14f, false)) { e.Hit(15f * _p.DamageScale, _p.transform.position.x, true, false); e.Stagger(3f); }
                    _p.TryCookieTandem(); AddStrain(25f); break;
            }
            Say(Name[i]);
            return true;
        }

        // ------------------------------------------------------------------ tick

        private void Update() => Tick(Time.deltaTime);

        public void Tick(float dt)
        {
            _echoCd -= dt; _invuln -= dt; _lockout -= dt; _strainIdle += dt;
            foreach (var m in new List<SpecialMove>(_cd.Keys)) if (_cd[m] > 0f) _cd[m] -= dt;
            if (SonarLocked && (_lockTimer -= dt) <= 0f) SonarLocked = false;
            if (_overclock > 0f)
            {
                _overclock -= dt; _beat -= dt; if (_beat <= 0f) { _beat = 0.9f; Play(SideScrollTones.Heartbeat, 0.8f); }
                Strain = Mathf.Min(99f, Strain + 4f * dt * (_prog != null ? _prog.StrainCostMultiplier : 1f)); _strainIdle = 0f;
                if (_overclock <= 0f) Say("Overclock ends");
            }
            if (Strain >= 60f && (_warnTimer -= dt) <= 0f) { _warnTimer = 2f; Play(SideScrollTones.StrainWarn, 0.5f); }
            if (_strainIdle > 2.5f && Strain > 0f && _overclock <= 0f) Strain = Mathf.Max(0f, Strain - 5f * dt);
        }

        // ------------------------------------------------------------------ helpers
        private IEnumerable<SideScrollEnemy> Foes(float range, bool frontOnly)
        {
            var list = new List<SideScrollEnemy>();
            foreach (var e in SideScrollEnemy.All)
            {
                if (e == null || e.IsDown) continue;
                float dx = e.transform.position.x - _p.transform.position.x;
                if (Mathf.Abs(dx) > range) continue;
                if (frontOnly && Mathf.Sign(dx) != _p.Facing) continue;
                list.Add(e);
            }
            return list;
        }
        private SideScrollEnemy NearestFront(float range) { SideScrollEnemy b = null; float bd = range; foreach (var e in Foes(range, true)) { float d = Mathf.Abs(e.transform.position.x - _p.transform.position.x); if (d < bd) { bd = d; b = e; } } return b; }
        private void Play(AudioClip c, float v) { if (c != null) _sfx.PlayOneShot(c, v); }
        private void PlayPitched(AudioClip c, float v, float pitch) { _sfx.pitch = pitch; _sfx.PlayOneShot(c, v); _sfx.pitch = 1f; }
        private static void Say(string s) { AccessibilityManager.Instance?.Announce(s); }
        public string StatusLine() => $"Special {SpecialMeter:0} percent. Strain {Strain:0} percent." + (Selected.HasValue ? " Selected: " + Name[(int)Selected.Value] + "." : "");
    }
}
