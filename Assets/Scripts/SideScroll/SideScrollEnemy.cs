using System;
using System.Collections.Generic;
using EchoesOfNeon.Weapons;
using UnityEngine;

namespace EchoesOfNeon.SideScroll
{
    public enum EnemyState { Patrol, Chase, WindUp, Strike, Recover, Staggered, Down }

    /// <summary>Street-level enemy archetypes. Each has its own audio signature (hum, wind-up, bark) and counter:
    /// PipeThug overhead swing (fend); KnifeRunner fast lunge (jump/step); BottleThrower lobbed bottle (fend reflects it);
    /// ShieldGuard immune to light hits from the front (Cookie/Titan Bite or Sonic Pulse); ZipGunPunk charged shot along
    /// the plane (jump it or fend); AlleyRat weak and fast, comes in threes (Flash-Blind); DroneHawk aerial dive (jump
    /// strike); BruteBouncer slow unblockable slam (jump it - it misses airborne targets).</summary>
    public enum EnemyKind { PipeThug, KnifeRunner, BottleThrower, ShieldGuard, ZipGunPunk, AlleyRat, DroneHawk, BruteBouncer }

    [RequireComponent(typeof(CharacterController))]
    public class SideScrollEnemy : MonoBehaviour, IDamageable
    {
        public static readonly List<SideScrollEnemy> All = new List<SideScrollEnemy>();
        public static event Action<SideScrollEnemy> Killed;

        public EnemyKind kind = EnemyKind.PipeThug;
        public SideScrollPlayer target;
        public float patrolHalfWidth = 4f;
        public float gravity = -22f;

        // Configured per kind in Configure()
        public float maxHealth = 40f, patrolSpeed = 1.5f, chaseSpeed = 3.2f, noticeRange = 9f, strikeRange = 1.5f;
        public float windUpSeconds = 0.5f, recoverSeconds = 0.8f, strikeDamage = 12f;
        public int xpValue = 20;
        public bool ranged, aerial, unblockable, frontGuard;
        public float rangeMin = 4f, rangeMax = 8f, projectileSpeed = 8f;
        public float humHz = 140f, windFrom = 220f, windTo = 660f, barkFrom = 420f, barkTo = 260f;

        public float Health { get; private set; }
        public EnemyState State { get; private set; } = EnemyState.Patrol;
        public bool IsDown => State == EnemyState.Down;
        public int Facing { get; private set; } = 1;
        public int StrikesThrown { get; private set; }
        public bool GuardBroken => _guardBrokenTimer > 0f;

        private CharacterController _cc;
        private AudioSource _voice, _hum;
        private float _timer, _vy, _homeX, _patrolDir = 1f, _guardBrokenTimer, _homeY;
        private bool _lastHitByCookie, _configured;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>(); _cc.minMoveDistance = 0f;
            if (!_configured) Configure(kind);
            Health = maxHealth; _homeX = transform.position.x; _homeY = transform.position.y;
            _voice = MakeSource("EnemyVoice", false);
            _hum = MakeSource("EnemyHum", true); _hum.clip = aerial ? SideScrollTones.DroneHum : SideScrollTones.HumFor((int)kind, humHz); _hum.volume = 0.18f;
        }

        public void Configure(EnemyKind k)
        {
            kind = k; _configured = true;
            switch (k)
            {
                case EnemyKind.PipeThug: break;   // defaults
                case EnemyKind.KnifeRunner: maxHealth = 25; chaseSpeed = 5.6f; strikeRange = 1.6f; windUpSeconds = 0.3f; recoverSeconds = 0.6f; strikeDamage = 9; xpValue = 18; humHz = 260f; windFrom = 700f; windTo = 1500f; barkFrom = 900f; barkTo = 700f; break;
                case EnemyKind.BottleThrower: maxHealth = 20; ranged = true; chaseSpeed = 2.4f; windUpSeconds = 0.6f; recoverSeconds = 1.0f; strikeDamage = 10; xpValue = 20; humHz = 190f; windFrom = 500f; windTo = 900f; break;
                case EnemyKind.ShieldGuard: maxHealth = 70; frontGuard = true; chaseSpeed = 2.4f; windUpSeconds = 0.6f; strikeDamage = 14; xpValue = 35; humHz = 100f; windFrom = 150f; windTo = 150f; barkFrom = 200f; barkTo = 180f; break;
                case EnemyKind.ZipGunPunk: maxHealth = 25; ranged = true; rangeMin = 5f; rangeMax = 11f; chaseSpeed = 2.6f; windUpSeconds = 0.8f; recoverSeconds = 1.2f; strikeDamage = 14; xpValue = 30; humHz = 330f; windFrom = 300f; windTo = 1800f; break;
                case EnemyKind.AlleyRat: maxHealth = 10; chaseSpeed = 4.4f; strikeRange = 1.2f; windUpSeconds = 0.25f; recoverSeconds = 0.5f; strikeDamage = 4; xpValue = 8; humHz = 420f; windFrom = 1100f; windTo = 1300f; barkFrom = 1400f; barkTo = 1100f; noticeRange = 11f; break;
                case EnemyKind.DroneHawk: maxHealth = 20; aerial = true; chaseSpeed = 3.6f; strikeRange = 1.5f; windUpSeconds = 0.7f; recoverSeconds = 1.2f; strikeDamage = 12; xpValue = 28; humHz = 330f; windFrom = 300f; windTo = 1400f; break;
                case EnemyKind.BruteBouncer: maxHealth = 120; unblockable = true; patrolSpeed = 1f; chaseSpeed = 1.7f; strikeRange = 2.0f; windUpSeconds = 0.9f; recoverSeconds = 1.4f; strikeDamage = 30; xpValue = 60; humHz = 55f; windFrom = 90f; windTo = 160f; barkFrom = 90f; barkTo = 60f; break;
            }
            Health = maxHealth;
        }

        private AudioSource MakeSource(string n, bool loop)
        {
            var go = new GameObject(n); go.transform.SetParent(transform, false); go.transform.localPosition = Vector3.up;
            var s = go.AddComponent<AudioSource>();
            s.spatialBlend = 1f; s.rolloffMode = AudioRolloffMode.Linear; s.minDistance = 2f; s.maxDistance = 22f;
            s.loop = loop; s.playOnAwake = false;
            EchoesOfNeon.Core.AudioRouting.Bind(s, EchoesOfNeon.Core.AudioCategory.Sfx);
            return s;
        }

        private void OnEnable() { All.Add(this); }
        private void OnDisable() { All.Remove(this); }
        private void Start() { if (_hum != null && _hum.clip != null) _hum.Play(); }
        private void Update() => Tick(Time.deltaTime);

        public void Tick(float dt)
        {
            if (IsDown) return;
            if (_guardBrokenTimer > 0f) _guardBrokenTimer -= dt;
            float dx = target != null ? target.transform.position.x - transform.position.x : 0f;
            float dist = Mathf.Abs(dx);
            float notice = noticeRange * (target != null && target.IsVeiled ? 0.5f : 1f);
            bool sees = target != null && !target.IsDead && dist <= notice && Mathf.Abs(target.transform.position.y - transform.position.y) < (aerial ? 6f : 2.5f);
            float move = 0f; float vmove = 0f;
            _timer -= dt;

            switch (State)
            {
                case EnemyState.Patrol:
                    if (sees) { State = EnemyState.Chase; _voice.PlayOneShot(SideScrollTones.BarkFor((int)kind, barkFrom, barkTo), 0.7f); break; }
                    if (transform.position.x > _homeX + patrolHalfWidth) _patrolDir = -1f;
                    else if (transform.position.x < _homeX - patrolHalfWidth) _patrolDir = 1f;
                    move = _patrolDir * patrolSpeed;
                    if (aerial) vmove = (_homeY + 3f - transform.position.y);
                    break;
                case EnemyState.Chase:
                    if (!sees) { State = EnemyState.Patrol; break; }
                    Face(dx);
                    if (ranged)
                    {
                        if (dist < rangeMin) move = -Mathf.Sign(dx) * chaseSpeed;
                        else if (dist > rangeMax) move = Mathf.Sign(dx) * chaseSpeed;
                        else BeginWindUp();
                    }
                    else if (dist <= strikeRange) BeginWindUp();
                    else move = Mathf.Sign(dx) * chaseSpeed;
                    if (aerial) vmove = Mathf.Clamp((target.transform.position.y + 3f) - transform.position.y, -3f, 3f);
                    break;
                case EnemyState.WindUp:
                    if (aerial) vmove = Mathf.Clamp((target.transform.position.y + 3f) - transform.position.y, -3f, 3f);
                    if (_timer <= 0f) { State = EnemyState.Strike; _timer = aerial ? 0.45f : 0.12f; BeginStrike(); }
                    break;
                case EnemyState.Strike:
                    if (kind == EnemyKind.KnifeRunner) move = Facing * 8f;
                    if (aerial) { vmove = -9f; move = Facing * 3f; }
                    if (_timer <= 0f) { if (aerial || kind == EnemyKind.KnifeRunner || !ranged) EndStrike(); State = EnemyState.Recover; _timer = recoverSeconds; }
                    break;
                case EnemyState.Recover:
                    if (aerial) vmove = (_homeY + 3f - transform.position.y);
                    if (_timer <= 0f) State = EnemyState.Chase;
                    break;
                case EnemyState.Staggered:
                    if (aerial) vmove = (_homeY + 1.5f - transform.position.y);
                    if (_timer <= 0f) State = EnemyState.Chase;
                    break;
            }
            if (move != 0f && !(State == EnemyState.Chase && ranged && Mathf.Abs(dx) < rangeMin)) Facing = (int)Mathf.Sign(move);
            if (State == EnemyState.Chase && ranged && dist < rangeMin) Face(dx);

            if (aerial) { _cc.Move(new Vector3(move, vmove, 0f) * dt); }
            else
            {
                if (_cc.isGrounded && _vy < 0f) _vy = -2f;
                _vy += gravity * dt;
                _cc.Move(new Vector3(move, _vy, 0f) * dt);
            }
            var p = transform.position; p.z = 0f; transform.position = p;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, Facing > 0 ? 90f : -90f, 0), 1f - Mathf.Exp(-14f * dt));
            UpdateHum();
        }

        private void BeginWindUp()
        {
            State = EnemyState.WindUp; _timer = windUpSeconds * EchoesOfNeon.Accessibility.DifficultyRules.CurrentEnemyTelegraph;
            Face(target.transform.position.x - transform.position.x);
            _voice.PlayOneShot(kind == EnemyKind.DroneHawk ? SideScrollTones.DroneDive : kind == EnemyKind.ZipGunPunk ? SideScrollTones.GunCharge : kind == EnemyKind.BottleThrower ? SideScrollTones.Bottle
                : SideScrollTones.WindupFor((int)kind, windFrom, windTo, Mathf.Max(0.3f, windUpSeconds)), 0.9f);
        }

        private void BeginStrike()
        {
            StrikesThrown++;
            if (target == null) return;
            if (kind == EnemyKind.BottleThrower) { SideScrollProjectile.Spawn(this, transform.position + Vector3.up * 1.4f + Vector3.right * Facing, Facing, projectileSpeed, strikeDamage); return; }
            if (kind == EnemyKind.ZipGunPunk)
            {
                _voice.PlayOneShot(SideScrollTones.GunShot, 1f);
                float dx = target.transform.position.x - transform.position.x;
                if (Mathf.Sign(dx) == Facing && Mathf.Abs(dx) <= 14f && Mathf.Abs(target.transform.position.y - transform.position.y) < 1.1f)
                    target.TakeHit(strikeDamage, transform.position, false, this, null);
                return;
            }
            if (!aerial && kind != EnemyKind.KnifeRunner) MeleeHit();     // knife/drone resolve at the end of their lunge/dive
            if (kind == EnemyKind.BruteBouncer) _voice.PlayOneShot(SideScrollTones.Slam, 1f);
        }

        private void EndStrike() { if (aerial || kind == EnemyKind.KnifeRunner) MeleeHit(); }

        private void MeleeHit()
        {
            if (target == null) return;
            float dx = target.transform.position.x - transform.position.x;
            float dy = Mathf.Abs(target.transform.position.y - transform.position.y);
            bool inReach = Mathf.Abs(dx) <= strikeRange + 0.5f && dy < (aerial ? 2.0f : 1.5f);
            if (kind == EnemyKind.BruteBouncer && !target.IsGrounded) inReach = false;      // slam misses a jumping target
            if (inReach) target.TakeHit(strikeDamage, transform.position, unblockable, this, null);
        }

        private void Face(float dx) { if (Mathf.Abs(dx) > 0.05f) Facing = (int)Mathf.Sign(dx); }

        private void UpdateHum()
        {
            if (_hum == null || target == null) return;
            float dy = Mathf.Clamp(transform.position.y - target.transform.position.y, -4f, 6f);
            _hum.pitch = Mathf.Pow(2f, dy / 5f);            // +5 m of height = one octave up; below = lower
        }

        public void Stagger(float seconds)
        {
            if (IsDown) return;
            State = EnemyState.Staggered; _timer = seconds;
        }

        public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal) => Hit(amount, hitPoint.x, false, false);

        /// <summary>Resolves one incoming hit. Returns true if it was BLOCKED (shield). heavy = Titan Bite / specials,
        /// which break a Shield Guard's guard. Cookie's hits (byCookie) credit her with the XP.</summary>
        public bool Hit(float amount, float fromX, bool heavy, bool byCookie)
        {
            if (IsDown) return false;
            if (frontGuard && !GuardBroken && (fromX - transform.position.x) * Facing > 0f)
            {
                if (!heavy) { _voice.PlayOneShot(SideScrollTones.Block, 0.9f); return true; }
                _guardBrokenTimer = 3.5f; Stagger(1.5f);
            }
            _lastHitByCookie = byCookie;
            Health -= amount;
            if (Health <= 0f) { Die(); return false; }
            if (State == EnemyState.Patrol) State = EnemyState.Chase;
            if (State == EnemyState.WindUp) { State = EnemyState.Staggered; _timer = 0.35f; }     // hits interrupt wind-ups
            return false;
        }

        private void Die()
        {
            Health = 0f; State = EnemyState.Down; _hum.Stop();
            _voice.PlayOneShot(SideScrollTones.EnemyDown, 1f);
            var prog = SideScrollProgression.Instance;
            if (prog != null)
            {
                prog.AwardMarcusXp(xpValue, kind.ToString());
                prog.AwardCookieXp(_lastHitByCookie ? xpValue : Mathf.Max(1, xpValue / 3));
            }
            Killed?.Invoke(this);
        }
    }

    /// <summary>A thrown bottle. Travels along the plane with a rising whistle; a fend REFLECTS it back at the thrower.</summary>
    public class SideScrollProjectile : MonoBehaviour
    {
        public static readonly List<SideScrollProjectile> All = new List<SideScrollProjectile>();
        private SideScrollEnemy _owner; private float _dir, _speed, _damage, _life = 4f; private bool _reflected; private AudioSource _src;

        public static SideScrollProjectile Spawn(SideScrollEnemy owner, Vector3 pos, int dir, float speed, float damage)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Sphere); g.name = "Bottle";
            UnityEngine.Object.Destroy(g.GetComponent<Collider>());
            g.transform.position = pos; g.transform.localScale = Vector3.one * 0.25f;
            var p = g.AddComponent<SideScrollProjectile>(); p._owner = owner; p._dir = dir; p._speed = speed; p._damage = damage;
            p._src = g.AddComponent<AudioSource>(); p._src.spatialBlend = 1f; p._src.rolloffMode = AudioRolloffMode.Linear; p._src.maxDistance = 20f;
            EchoesOfNeon.Core.AudioRouting.Bind(p._src, EchoesOfNeon.Core.AudioCategory.Sfx); p._src.clip = SideScrollTones.Bottle; p._src.loop = true; p._src.Play();
            return p;
        }

        private void OnEnable() { All.Add(this); }
        private void OnDisable() { All.Remove(this); }
        public void Reflect() { _dir = -_dir; _reflected = true; _life = 3f; }
        public float X => transform.position.x;

        private void Update()
        {
            transform.position += Vector3.right * (_dir * _speed * Time.deltaTime);
            if ((_life -= Time.deltaTime) <= 0f) { Destroy(gameObject); return; }
            if (!_reflected)
            {
                var pl = _owner != null ? _owner.target : null;
                if (pl == null) return;
                var d = pl.transform.position + Vector3.up * 0.9f - transform.position;
                if (Mathf.Abs(d.x) < 0.6f && Mathf.Abs(d.y) < 1.0f) { pl.TakeHit(_damage, transform.position, false, null, this); if (!_reflected) Destroy(gameObject); }
            }
            else if (_owner != null && !_owner.IsDown && Mathf.Abs(_owner.transform.position.x - transform.position.x) < 0.7f)
            { _owner.Hit(_damage * 1.5f, transform.position.x, false, false); Destroy(gameObject); }
        }
    }
}
