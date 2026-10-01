using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using EchoesOfNeon.Weapons;
using UnityEngine;

namespace EchoesOfNeon.SideScroll
{
    /// <summary>
    /// Marcus in the 2.5D side-scroller prototype: 3D body, movement locked to the X/Y plane (z = 0).
    /// Controls (all rebindable through InputManager): Move = left/right, Jump, Fire = cane strike (button-mash
    /// 3-hit combo), Fend = cane fend-off (a short timed window that deflects incoming attacks and staggers the
    /// attacker), Whistle = sends Cookie after the nearest enemy, SonarPulse = spoken scan of what is around.
    /// Jump is forgiving by design (coyote time + input buffer) because platform timing has to be judged by ear.
    /// Everything runs through Tick(dt) with public Set*/command methods so tests can drive it without a keyboard.
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

        public float Health { get; private set; }
        public bool IsDead => Health <= 0f;
        public int Facing { get; private set; } = 1;                // +1 right, -1 left
        public bool IsGrounded => _cc.isGrounded;
        public Vector3 LastSafePosition { get; private set; }
        public int ComboCount { get; private set; }
        public bool IsFending => _fendTimer > 0f;
        public int FendsSucceeded { get; private set; }
        public int StrikesLanded { get; private set; }

        private CharacterController _cc;
        private float _moveInput, _vy, _coyote, _jumpBuf, _strikeCd, _comboTimer, _fendTimer, _fendCd;
        private bool _sprint, _wasGrounded = true;
        private AudioSource _sfx;
        private InputManager _input;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _cc.minMoveDistance = 0f;
            Health = maxHealth;
            LastSafePosition = transform.position;
            var go = new GameObject("PlayerSfx"); go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0, 1f, 0);
            _sfx = go.AddComponent<AudioSource>(); _sfx.spatialBlend = 0f; _sfx.playOnAwake = false;  // own sounds: centered
        }

        private void OnEnable()
        {
            _input = InputManager.Instance;
            if (_input == null) return;
            _input.OnMove += OnMove; _input.OnJump += RequestJump; _input.OnFirePressed += Strike;
            _input.OnFendPressed += Fend; _input.OnWhistlePressed += OnWhistleKey; _input.OnSonarPulse += OnSonarKey;
            _input.OnSprintPressed += OnSprintDown; _input.OnSprintReleased += OnSprintUp;
        }

        private void OnDisable()
        {
            if (_input == null) return;
            _input.OnMove -= OnMove; _input.OnJump -= RequestJump; _input.OnFirePressed -= Strike;
            _input.OnFendPressed -= Fend; _input.OnWhistlePressed -= OnWhistleKey; _input.OnSonarPulse -= OnSonarKey;
            _input.OnSprintPressed -= OnSprintDown; _input.OnSprintReleased -= OnSprintUp;
        }

        private void OnMove(Vector2 v) => SetMoveInput(v.x);
        private void OnWhistleKey() => Whistle();
        private void OnSonarKey() => SonarScan();
        private void OnSprintDown() => _sprint = true;
        private void OnSprintUp() => _sprint = false;
        public void SetMoveInput(float x) => _moveInput = Mathf.Abs(x) < 0.2f ? 0f : Mathf.Sign(x);   // digital: walking is the only speed control
        public void RequestJump() => _jumpBuf = jumpBuffer;

        private void Update() => Tick(Time.deltaTime);

        public void Tick(float dt)
        {
            if (IsDead) return;
            _strikeCd -= dt; _fendCd -= dt; _fendTimer -= dt;
            if (_comboTimer > 0f && (_comboTimer -= dt) <= 0f) ComboCount = 0;

            if (_moveInput != 0f) Facing = (int)_moveInput;
            if (_cc.isGrounded) { _coyote = coyoteTime; if (_vy < 0f) _vy = -2f; } else _coyote -= dt;
            _jumpBuf -= dt;
            if (_jumpBuf > 0f && _coyote > 0f)
            {
                _vy = Mathf.Sqrt(-2f * gravity * jumpHeight); _jumpBuf = 0f; _coyote = 0f;
                Play(SideScrollTones.Jump, 0.5f);
            }
            _vy += gravity * dt;

            float speed = _sprint ? sprintSpeed : walkSpeed;
            var move = new Vector3(_moveInput * speed, _vy, 0f) * dt;
            _cc.Move(move);
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
            _strikeCd = strikeCooldown;
            ComboCount = _comboTimer > 0f ? ComboCount + 1 : 1; _comboTimer = comboResetTime;
            bool finisher = ComboCount >= 3; if (finisher) ComboCount = 0;
            Play(SideScrollTones.Swing, 0.5f);
            var target = NearestEnemyInFront(strikeRange);
            if (target == null) return;
            StrikesLanded++;
            target.TakeDamage(finisher ? comboFinisherDamage : strikeDamage, transform.position, Vector3.right * -Facing);
            if (finisher) target.Stagger(0.8f);
            Play(SideScrollTones.CaneHit, finisher ? 0.9f : 0.6f);
        }

        /// <summary>Starts the fend window. An enemy strike landing inside the window is deflected: no damage,
        /// the attacker is staggered, and a bright clack plays. Mashing the button is penalised only by the cooldown.</summary>
        public void Fend()
        {
            if (IsDead || _fendCd > 0f) return;
            _fendTimer = fendWindow; _fendCd = fendCooldown + fendWindow;
            Play(SideScrollTones.Swing, 0.35f);
        }

        public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (IsDead) return;
            if (IsFending)
            {
                FendsSucceeded++;
                Play(SideScrollTones.Fend, 1f);
                var attacker = NearestEnemyAnywhere(3.5f);
                if (attacker != null) attacker.Stagger(fendStaggerSeconds);
                _fendTimer = 0f;
                AccessibilityManager.Instance?.Announce("Fended off");
                return;
            }
            Health = Mathf.Max(0f, Health - amount);
            Play(SideScrollTones.PlayerHurt, 1f);
            if (IsDead) AccessibilityManager.Instance?.Announce("Marcus is down");
        }

        // -------------------------------------------------------------- Whistle

        public bool Whistle()
        {
            if (IsDead) return false;
            Play(SideScrollTones.Whistle, 0.7f);
            if (cookie == null) return false;
            if (!cookie.IsReady) { AccessibilityManager.Instance?.Announce("Cookie is not ready"); return false; }
            var target = FindWhistleTarget();
            if (target == null) { AccessibilityManager.Instance?.Announce("No enemy in range"); return false; }
            cookie.Attack(target);
            AccessibilityManager.Instance?.Announce("Cookie, get him");
            return true;
        }

        private SideScrollEnemy FindWhistleTarget()
        {
            SideScrollEnemy best = null; float bestScore = float.MaxValue;
            foreach (var e in SideScrollEnemy.All)
            {
                if (e == null || e.IsDown) continue;
                float dx = e.transform.position.x - transform.position.x;
                float dist = Mathf.Abs(dx);
                if (dist > whistleRange) continue;
                float score = dist + (Mathf.Sign(dx) == Facing ? 0f : 3f);        // prefer the way he is facing
                if (score < bestScore) { bestScore = score; best = e; }
            }
            return best;
        }

        // ---------------------------------------------------------------- Sonar

        /// <summary>Spoken scan: "two enemies ahead, one behind. Nearest ahead, 6 metres." Counts are by facing.</summary>
        public string SonarScan()
        {
            int ahead = 0, behind = 0; float nearest = float.MaxValue; bool nearestAhead = true;
            foreach (var e in SideScrollEnemy.All)
            {
                if (e == null || e.IsDown) continue;
                float dx = e.transform.position.x - transform.position.x;
                if (Mathf.Abs(dx) > whistleRange) continue;
                bool isAhead = Mathf.Sign(dx) == Facing;
                if (isAhead) ahead++; else behind++;
                if (Mathf.Abs(dx) < nearest) { nearest = Mathf.Abs(dx); nearestAhead = isAhead; }
            }
            string msg = (ahead + behind == 0) ? "Clear."
                : $"{Count(ahead, "ahead")}{(ahead > 0 && behind > 0 ? ", " : "")}{Count(behind, "behind")}. Nearest {(nearestAhead ? "ahead" : "behind")}, {Mathf.RoundToInt(nearest)} metres.";
            Play(SideScrollTones.Sonar, 0.4f);
            AccessibilityManager.Instance?.Announce(msg);
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
                if (Mathf.Abs(dx) > range || Mathf.Abs(e.transform.position.y - transform.position.y) > 1.5f) continue;
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
            Health = Mathf.Max(1f, Health - 15f);
            Play(SideScrollTones.PlayerHurt, 0.8f);
            AccessibilityManager.Instance?.Announce("You fell. Back at the last safe ground.");
        }

        private void Play(AudioClip clip, float vol) { if (clip != null && _sfx != null) _sfx.PlayOneShot(clip, vol); }
    }
}
