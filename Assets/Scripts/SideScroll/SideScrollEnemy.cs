using System.Collections.Generic;
using EchoesOfNeon.Weapons;
using UnityEngine;

namespace EchoesOfNeon.SideScroll
{
    public enum EnemyState { Patrol, Chase, WindUp, Strike, Recover, Staggered, Down }

    /// <summary>
    /// A street enforcer for the side-scroller. Brawler-style, not stealth: patrol a stretch, notice Marcus within
    /// notice range, close in, then WIND UP (a rising ~0.5 s tone from the enemy's own position, so the player can
    /// fend or step away by ear), STRIKE, and RECOVER. Fend or Cookie's bite staggers it. While alive it emits a
    /// quiet looping hum whose pitch rises with its height relative to the player, so left/right comes from stereo
    /// pan and above/below from pitch. All logic in Tick(dt) for deterministic testing.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class SideScrollEnemy : MonoBehaviour, IDamageable
    {
        public static readonly List<SideScrollEnemy> All = new List<SideScrollEnemy>();

        public SideScrollPlayer target;
        public float maxHealth = 40f;
        public float patrolSpeed = 1.5f, chaseSpeed = 3.2f;
        public float patrolHalfWidth = 4f;
        public float noticeRange = 9f;
        public float strikeRange = 1.5f;
        public float windUpSeconds = 0.5f, recoverSeconds = 0.8f;
        public float strikeDamage = 12f;
        public float gravity = -22f;

        public float Health { get; private set; }
        public EnemyState State { get; private set; } = EnemyState.Patrol;
        public bool IsDown => State == EnemyState.Down;
        public int Facing { get; private set; } = 1;
        public int StrikesThrown { get; private set; }

        private CharacterController _cc;
        private AudioSource _voice, _hum;
        private float _timer, _vy, _homeX, _patrolDir = 1f, _humPhase;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>(); _cc.minMoveDistance = 0f;
            Health = maxHealth; _homeX = transform.position.x;
            _voice = MakeSource("EnemyVoice", false, 1f);
            _hum = MakeSource("EnemyHum", true, 0.35f); _hum.clip = SideScrollTones.EnemyHum; _hum.volume = 0.18f;
        }

        private AudioSource MakeSource(string n, bool loop, float vol)
        {
            var go = new GameObject(n); go.transform.SetParent(transform, false); go.transform.localPosition = Vector3.up;
            var s = go.AddComponent<AudioSource>();
            s.spatialBlend = 1f; s.rolloffMode = AudioRolloffMode.Linear; s.minDistance = 2f; s.maxDistance = 22f;
            s.loop = loop; s.playOnAwake = false;
            return s;
        }

        private void OnEnable() { All.Add(this); }
        private void OnDisable() { All.Remove(this); }
        private void Start() { if (_hum != null && _hum.clip != null) _hum.Play(); }
        private void Update() => Tick(Time.deltaTime);

        public void Tick(float dt)
        {
            if (IsDown) return;
            float dx = target != null ? target.transform.position.x - transform.position.x : 0f;
            float dist = Mathf.Abs(dx);
            bool sees = target != null && !target.IsDead && dist <= noticeRange && Mathf.Abs(target.transform.position.y - transform.position.y) < 2.5f;
            float move = 0f;
            _timer -= dt;

            switch (State)
            {
                case EnemyState.Patrol:
                    if (sees) { State = EnemyState.Chase; _voice.PlayOneShot(SideScrollTones.Bark, 0.7f); break; }
                    if (transform.position.x > _homeX + patrolHalfWidth) _patrolDir = -1f;
                    else if (transform.position.x < _homeX - patrolHalfWidth) _patrolDir = 1f;
                    move = _patrolDir * patrolSpeed; break;
                case EnemyState.Chase:
                    if (!sees) { State = EnemyState.Patrol; break; }
                    if (dist <= strikeRange) { State = EnemyState.WindUp; _timer = windUpSeconds; Face(dx); _voice.PlayOneShot(SideScrollTones.Windup, 0.9f); break; }
                    Face(dx); move = Mathf.Sign(dx) * chaseSpeed; break;
                case EnemyState.WindUp:
                    if (_timer <= 0f) { State = EnemyState.Strike; _timer = 0.1f; DoStrike(); }
                    break;
                case EnemyState.Strike:
                    if (_timer <= 0f) { State = EnemyState.Recover; _timer = recoverSeconds; }
                    break;
                case EnemyState.Recover:
                    if (_timer <= 0f) State = EnemyState.Chase;
                    break;
                case EnemyState.Staggered:
                    if (_timer <= 0f) State = EnemyState.Chase;
                    break;
            }
            if (move != 0f) Facing = (int)Mathf.Sign(move);

            if (_cc.isGrounded && _vy < 0f) _vy = -2f;
            _vy += gravity * dt;
            _cc.Move(new Vector3(move, _vy, 0f) * dt);
            var p = transform.position; p.z = 0f; transform.position = p;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, Facing > 0 ? 90f : -90f, 0), 1f - Mathf.Exp(-14f * dt));
            UpdateHum();
        }

        private void DoStrike()
        {
            StrikesThrown++;
            if (target == null) return;
            float dx = target.transform.position.x - transform.position.x;
            if (Mathf.Abs(dx) <= strikeRange + 0.4f && Mathf.Abs(target.transform.position.y - transform.position.y) < 1.5f)
                target.TakeDamage(strikeDamage, transform.position, Vector3.right * -Mathf.Sign(dx));
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

        public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (IsDown) return;
            Health -= amount;
            if (Health <= 0f) { Health = 0f; State = EnemyState.Down; _hum.Stop(); _voice.PlayOneShot(SideScrollTones.EnemyDown, 1f); return; }
            if (State == EnemyState.Patrol) State = EnemyState.Chase;
            if (State == EnemyState.WindUp) { State = EnemyState.Staggered; _timer = 0.35f; }     // hits interrupt wind-ups
        }
    }
}
