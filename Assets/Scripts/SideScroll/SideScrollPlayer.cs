using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using EchoesOfNeon.Weapons;
using UnityEngine;

namespace EchoesOfNeon.SideScroll
{
    /// <summary>
    /// Marcus in the 2.5D side-scroller: 3D body, movement locked to the X/Y plane (z = 0).
    /// Controls (all rebindable through InputManager): Move, Jump, Fire = cane strike (3-hit combo), Fend = cane fend-off
    /// (timed deflect + stagger), Whistle = Cookie attack, SonarPulse = spoken scan (and arms Sonar Lock), EchoStep dash,
    /// Special / CycleSpecial (SideScrollAbilities), Howl (Cookie), Crouch held = Shadow Veil, Journal, Status.
    /// Recovery: med-cells (HealthPickup), rest points (RestPoint), Resonance (heal on fend / nearby kill), Cookie's
    /// Rally, and an optional regen assist; at 0 health he is down and gets back up at the last rest point.
    /// Jump is forgiving by design (coyote time + input buffer). Everything runs through Tick(dt) with public
    /// command methods so tests can drive it without a keyboard.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class SideScrollPlayer : MonoBehaviour, IDamageable
    {
        [Header("Movement")]
        public float walkSpeed = 4f;
        public float sprintSpeed = 6.5f;
        public float jumpHeight = 1.6f;
        public float gravity = -22f;
        public float coyoteTime = 0.18f;
        public float jumpBuffer = 0.18f;

        [Header("Health")]
        public float maxHealth = 100f;
        public bool regenAssist;                      // assist: slowly regain health (to half) when out of combat
        public float respawnDelay = 2f;

        [Header("Cane")]
        public float strikeRange = 1.8f;
        public float strikeDamage = 12f;
        public float comboFinisherDamage = 22f;
        public float strikeCooldown = 0.32f;
        public float comboResetTime = 0.9f;
        public float fendWindow = 0.3f;
        public float fendCooldown = 0.6f;
        public float fendStaggerSeconds = 1.1f;

        [Header("Whistle / Cookie")]
        public SideScrollCookie cookie;
        public float whistleRange = 16f;

        public SideScrollProgression progression;
        public SideScrollAbilities abilities;

        public float Health { get; private set; }
        public float MaxHealth => maxHealth + (progression != null ? progression.MaxHealthBonus : 0f);
        public bool IsDead => Health <= 0f;
        public int Facing { get; private set; } = 1;
        public bool IsGrounded => _cc.isGrounded;
        public bool IsVeiled { get; private set; }
        public Vector3 LastSafePosition { get; private set; }
        public Vector3 RestPosition { get; private set; }
        public int ComboCount { get; private set; }
        public bool IsFending => _fendTimer > 0f;
        public int FendsSucceeded { get; private set; }
        public int StrikesLanded { get; private set; }
        public int Respawns { get; private set; }
        public float DamageScale => (progression != null ? progression.DamageMultiplier : 1f) * (abilities != null ? abilities.DamageMultiplier : 1f);

        private CharacterController _cc;
        private float _moveInput, _vy, _coyote, _jumpBuf, _strikeCd, _comboTimer, _fendTimer, _fendCd;
        private float _dashTime, _dashDir, _sinceHurt = 99f, _deadTimer, _lowWarn;
        private bool _sprint, _crouch, _wasGrounded = true, _lowAnnounced;
        private AudioSource _sfx;
        private InputManager _input;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _cc.minMoveDistance = 0f;
            Health = maxHealth;
            LastSafePosition = RestPosition = transform.position;
            var go = new GameObject("PlayerSfx"); go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0, 1f, 0);
            _sfx = go.AddComponent<AudioSource>(); _sfx.spatialBlend = 0f; _sfx.playOnAwake = false;  // own sounds: centered
        }

        private void Start()
        {
            if (progression == null) progression = SideScrollProgression.Instance;
            if (abilities == null) abilities = GetComponent<SideScrollAbilities>();
            if (progression != null) { progression.OnMarcusLevelUp += OnLevelUp; Health = Mathf.Min(MaxHealth, Health + progression.MaxHealthBonus); }
            SideScrollEnemy.Killed += OnEnemyKilled;
        }

        private void OnEnable()
        {
            _input = InputManager.Instance;
            if (_input == null) return;
            _input.OnMove += OnMove; _input.OnJump += RequestJump; _input.OnFirePressed += Strike;
            _input.OnFendPressed += Fend; _input.OnWhistlePressed += OnWhistleKey; _input.OnSonarPulse += OnSonarKey;
            _input.OnSprintPressed += OnSprintDown; _input.OnSprintReleased += OnSprintUp;
            _input.OnCrouchPressed += OnCrouchDown; _input.OnCrouchReleased += OnCrouchUp;
            _input.OnEchoStepPressed += EchoStep; _input.OnSpecialPressed += Special; _input.OnCycleSpecialPressed += CycleSpecial;
            _input.OnHowlPressed += Howl; _input.OnJournalPressed += Journal; _input.OnStatusPressed += OnStatusKey;
        }

        private void OnDisable()
        {
            SideScrollEnemy.Killed -= OnEnemyKilled;
            if (_input == null) return;
            _input.OnMove -= OnMove; _input.OnJump -= RequestJump; _input.OnFirePressed -= Strike;
            _input.OnFendPressed -= Fend; _input.OnWhistlePressed -= OnWhistleKey; _input.OnSonarPulse -= OnSonarKey;
            _input.OnSprintPressed -= OnSprintDown; _input.OnSprintReleased -= OnSprintUp;
            _input.OnCrouchPressed -= OnCrouchDown; _input.OnCrouchReleased -= OnCrouchUp;
            _input.OnEchoStepPressed -= EchoStep; _input.OnSpecialPressed -= Special; _input.OnCycleSpecialPressed -= CycleSpecial;
            _input.OnHowlPressed -= Howl; _input.OnJournalPressed -= Journal; _input.OnStatusPressed -= OnStatusKey;
        }

        private void OnMove(Vector2 v) => SetMoveInput(v.x);
        private void OnStatusKey() => Status();
        private void OnWhistleKey() => Whistle();
        private void OnSonarKey() => SonarScan();
        private void OnSprintDown() => _sprint = true;
        private void OnSprintUp() => _sprint = false;
        private void OnCrouchDown() => SetVeil(true);
        private void OnCrouchUp() => SetVeil(false);
        public void SetVeil(bool on) { _crouch = on; IsVeiled = on && progression != null && progression.Has(MarcusAbility.ShadowVeil); }
        public void SetMoveInput(float x) => _moveInput = Mathf.Abs(x) < 0.2f ? 0f : Mathf.Sign(x);   // digital: walking is the only speed control
        public void RequestJump() => _jumpBuf = jumpBuffer;
        public void EchoStep() { if (abilities != null) abilities.EchoStep(_moveInput != 0f ? (int)_moveInput : Facing); }
        public void Special() { if (abilities != null) abilities.UseSpecial(); }
        public void CycleSpecial() { if (abilities != null) abilities.CycleSpecial(); }
        public void Howl() { if (cookie != null) cookie.Howl(); }
        public void Journal() { if (progression != null) progression.ReadJournal(); }
        public void Dash(int dir, float distance) { _dashDir = dir; _dashTime = 0.18f; _dashSpeed = distance / 0.18f; }
        private float _dashSpeed;

        private void Update() => Tick(Time.deltaTime);

        public void Tick(float dt)
        {
            if (IsDead)
            {
                if ((_deadTimer -= dt) <= 0f) Respawn();
                return;
            }
            _strikeCd -= dt; _fendCd -= dt; _fendTimer -= dt; _sinceHurt += dt; _dashTime -= dt;
            if (_comboTimer > 0f && (_comboTimer -= dt) <= 0f) ComboCount = 0;

            if (regenAssist && _sinceHurt > 4f && Health < MaxHealth * 0.5f) Health = Mathf.Min(MaxHealth * 0.5f, Health + 3f * dt);
            if (Health < MaxHealth * 0.25f && !_lowAnnounced) { _lowAnnounced = true; Say("Low health"); }
            if (Health >= MaxHealth * 0.4f) _lowAnnounced = false;

            if (_moveInput != 0f) Facing = (int)_moveInput;
            if (_cc.isGrounded) { _coyote = coyoteTime; if (_vy < 0f) _vy = -2f; } else _coyote -= dt;
            _jumpBuf -= dt;
            if (_jumpBuf > 0f && _coyote > 0f)
            {
                _vy = Mathf.Sqrt(-2f * gravity * jumpHeight); _jumpBuf = 0f; _coyote = 0f;
                Play(SideScrollTones.Jump, 0.5f);
            }
            _vy += gravity * dt;

            float speed = (_sprint ? sprintSpeed : walkSpeed) * (progression != null ? progression.MoveSpeedMultiplier : 1f)
                          * (abilities != null ? abilities.SpeedMultiplier : 1f) * (IsVeiled ? 0.4f : 1f);
            float hx = _moveInput * speed;
            if (_dashTime > 0f) hx = _dashDir * _dashSpeed;
            _cc.Move(new Vector3(hx, _vy, 0f) * dt);
            var p = transform.position; p.z = 0f; transform.position = p;      // hard lock to the plane
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, Facing > 0 ? 90f : -90f, 0), 1f - Mathf.Exp(-18f * dt));

            if (_cc.isGrounded)
            {
                if (!_wasGrounded) Play(SideScrollTones.Land, 0.5f);
                LastSafePosition = transform.position;
            }
            _wasGrounded = _cc.isGrounded;
            if (transform.position.y < -12f) FallRespawn();
        }

        // ---------------------------------------------------------------- Cane

        public void Strike()
        {
            if (IsDead || _strikeCd > 0f) return;
            _strikeCd = strikeCooldown * (abilities != null ? abilities.StrikeCooldownMultiplier : 1f);
            ComboCount = _comboTimer > 0f ? ComboCount + 1 : 1;
            _comboTimer = comboResetTime + (progression != null ? progression.ComboWindowBonus : 0f);
            bool finisher = ComboCount >= 3; if (finisher) ComboCount = 0;
            Play(SideScrollTones.Swing, 0.5f);
            var target = NearestEnemyInFront(strikeRange);
            if (target == null) return;
            float crit = abilities != null ? abilities.ConsumeCrit() : 1f;
            float dmg = (finisher ? comboFinisherDamage : strikeDamage) * DamageScale * crit;
            if (target.Hit(dmg, transform.position.x, finisher, false)) return;         // blocked by a shield: no stagger, no gain
            StrikesLanded++;
            abilities?.AddSpecial(6f);
            if (finisher) target.Stagger(0.8f);
            Play(SideScrollTones.CaneHit, finisher ? 0.9f : 0.6f);
        }

        public void Fend()
        {
            if (IsDead || _fendCd > 0f) return;
            float w = (fendWindow + (progression != null ? progression.FendWindowBonus : 0f)) * (abilities != null ? abilities.FendWindowMultiplier : 1f);
            _fendTimer = w; _fendCd = fendCooldown + w;
            Play(SideScrollTones.Swing, 0.35f);
        }

        // ---------------------------------------------------------------- damage + recovery

        public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal) => TakeHit(amount, hitPoint, false, null, null);

        /// <summary>Resolves one incoming hit: Echo Step dodges, Cookie's Guard Mode may absorb it, a fend deflects it
        /// (unless the attack is unblockable), otherwise it hurts (reduced by level).</summary>
        public void TakeHit(float amount, Vector3 from, bool unblockable, SideScrollEnemy attacker, SideScrollProjectile projectile)
        {
            if (IsDead) return;
            if (abilities != null && abilities.IsInvulnerable) { Say("Dodged"); return; }
            if (cookie != null && cookie.TryAbsorbHit(from)) return;
            if (IsFending && !unblockable)
            {
                FendsSucceeded++;
                Play(SideScrollTones.Fend, 1f);
                if (projectile != null) projectile.Reflect();
                var a = attacker != null ? attacker : NearestEnemyAnywhere(3.5f);
                if (a != null) a.Stagger(fendStaggerSeconds);
                _fendTimer = 0f;
                abilities?.AddSpecial(10f);
                if (progression != null) { progression.AwardMarcusXp(3, "fend"); if (progression.Has(MarcusAbility.Resonance)) Heal(6f, "Resonance"); }
                Say("Fended off");
                return;
            }
            float dmg = amount * (progression != null ? progression.DamageTakenMultiplier : 1f);
            Health = Mathf.Max(0f, Health - dmg); _sinceHurt = 0f;
            Play(SideScrollTones.PlayerHurt, 1f);
            if (IsDead) Down();
        }

        public void TakeUnavoidable(float amount) { if (IsDead) return; Health = Mathf.Max(0f, Health - amount); _sinceHurt = 0f; Play(SideScrollTones.PlayerHurt, 0.8f); if (IsDead) Down(); }

        public void Heal(float amount, string source = null)
        {
            if (IsDead || amount <= 0f) return;
            float before = Health; Health = Mathf.Min(MaxHealth, Health + amount);
            if (Health > before) { Play(SideScrollTones.HealGet, 0.6f); if (source != null) Say($"{source}, plus {Mathf.RoundToInt(Health - before)} health"); }
        }

        /// <summary>Rest point: full health, strain cleared, remembers this spot as the respawn point.</summary>
        public void Rest(Vector3 where)
        {
            Health = MaxHealth; RestPosition = where; abilities?.ClearStrain(); _lowAnnounced = false;
            Play(SideScrollTones.RestDone, 0.8f); Say("Rested. Health full.");
        }

        private void OnEnemyKilled(SideScrollEnemy e)
        {
            if (IsDead || progression == null || !progression.Has(MarcusAbility.Resonance) || e == null) return;
            if (Mathf.Abs(e.transform.position.x - transform.position.x) < 4f) Heal(8f, "Resonance");
        }

        private void OnLevelUp(int level)
        {
            float gain = 10f + (level == 5 ? 20f : 0f);
            Health = Mathf.Min(MaxHealth, Health + gain);
            Play(SideScrollTones.LevelUp, 0.9f);
        }

        private void Down()
        {
            _deadTimer = respawnDelay; Say("Marcus is down");
        }

        private void Respawn()
        {
            Respawns++;
            _cc.enabled = false; transform.position = RestPosition + Vector3.up * 0.2f; _cc.enabled = true; _vy = 0f;
            Health = MaxHealth * 0.5f; abilities?.ClearStrain(); _lowAnnounced = false; _sinceHurt = 0f;
            Say("Back on your feet at the last rest point.");
        }

        public string Status()
        {
            string s = $"Health {Mathf.RoundToInt(Health)} of {Mathf.RoundToInt(MaxHealth)}. " + (progression != null ? progression.StatusLine() + " " : "") + (abilities != null ? abilities.StatusLine() : "");
            Say(s); return s;
        }

        // -------------------------------------------------------------- Whistle

        public bool Whistle()
        {
            if (IsDead) return false;
            Play(SideScrollTones.Whistle, 0.7f);
            if (cookie == null) return false;
            if (!cookie.IsReady) { Say("Cookie is not ready"); return false; }
            var target = FindWhistleTarget();
            if (target == null) { Say("No enemy in range"); return false; }
            cookie.Attack(target);
            Say("Cookie, get him");
            return true;
        }

        public void TryCookieTandem() { if (cookie != null) cookie.TandemStrike(); }

        private float Range => whistleRange * (progression != null ? progression.SonarRadiusMultiplier : 1f);

        private SideScrollEnemy FindWhistleTarget()
        {
            SideScrollEnemy best = null; float bestScore = float.MaxValue;
            foreach (var e in SideScrollEnemy.All)
            {
                if (e == null || e.IsDown) continue;
                float dx = e.transform.position.x - transform.position.x;
                float dist = Mathf.Abs(dx);
                if (dist > Range) continue;
                float score = dist + (Mathf.Sign(dx) == Facing ? 0f : 3f);        // prefer the way he is facing
                if (score < bestScore) { bestScore = score; best = e; }
            }
            return best;
        }

        // ---------------------------------------------------------------- Sonar

        /// <summary>Spoken scan: "two enemies ahead, one behind. Nearest ahead, 6 metres." Also arms Sonar Lock (Lv3).</summary>
        public string SonarScan()
        {
            int ahead = 0, behind = 0; float nearest = float.MaxValue; bool nearestAhead = true;
            foreach (var e in SideScrollEnemy.All)
            {
                if (e == null || e.IsDown) continue;
                float dx = e.transform.position.x - transform.position.x;
                if (Mathf.Abs(dx) > Range) continue;
                bool isAhead = Mathf.Sign(dx) == Facing;
                if (isAhead) ahead++; else behind++;
                if (Mathf.Abs(dx) < nearest) { nearest = Mathf.Abs(dx); nearestAhead = isAhead; }
            }
            string msg = (ahead + behind == 0) ? "Clear."
                : $"{Count(ahead, "ahead")}{(ahead > 0 && behind > 0 ? ", " : "")}{Count(behind, "behind")}. Nearest {(nearestAhead ? "ahead" : "behind")}, {Mathf.RoundToInt(nearest)} metres.";
            Play(SideScrollTones.Sonar, 0.4f);
            if (abilities != null && abilities.ArmSonarLock()) msg += " Locked.";
            Say(msg);
            return msg;
        }

        private static string Count(int n, string where) => n == 0 ? "" : $"{n} {(n == 1 ? "enemy" : "enemies")} {where}";

        // ---------------------------------------------------------------- helpers

        private SideScrollEnemy NearestEnemyInFront(float range)
        {
            SideScrollEnemy best = null; float bestD = float.MaxValue;
            foreach (var e in SideScrollEnemy.All)
            {
                if (e == null || e.IsDown) continue;
                float dx = e.transform.position.x - transform.position.x;
                if (Mathf.Sign(dx) != Facing && Mathf.Abs(dx) > 0.3f) continue;
                if (Mathf.Abs(dx) > range || Mathf.Abs(e.transform.position.y - transform.position.y) > 1.8f) continue;
                if (Mathf.Abs(dx) < bestD) { bestD = Mathf.Abs(dx); best = e; }
            }
            return best;
        }

        private SideScrollEnemy NearestEnemyAnywhere(float range)
        {
            SideScrollEnemy best = null; float bestD = range;
            foreach (var e in SideScrollEnemy.All)
            {
                if (e == null || e.IsDown) continue;
                float d = Vector3.Distance(e.transform.position, transform.position);
                if (d < bestD) { bestD = d; best = e; }
            }
            return best;
        }

        private void FallRespawn()
        {
            _cc.enabled = false; transform.position = LastSafePosition + Vector3.up * 0.2f; _cc.enabled = true; _vy = 0f;
            TakeUnavoidable(15f); if (!IsDead) Health = Mathf.Max(1f, Health);
            Say("You fell. Back at the last safe ground.");
        }

        private void Play(AudioClip clip, float vol) { if (clip != null && _sfx != null) _sfx.PlayOneShot(clip, vol); }
        private static void Say(string s) { AccessibilityManager.Instance?.Announce(s); }
    }
}
