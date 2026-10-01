using EchoesOfNeon.Accessibility;
using UnityEngine;

namespace EchoesOfNeon.SideScroll
{
    public enum CookieState { Following, Charging, Biting, Returning }

    /// <summary>
    /// Cookie in the side-scroller. She trots alongside Marcus (kept findable by ear with pawsteps and a collar
    /// jingle). When he whistles she dashes at the chosen enemy, bites (damage + stagger), and trots back; a
    /// cooldown follows, and a rising chime tells the player she is ready again. Moves along X and snaps to the
    /// ground by raycast; state logic is in Tick(dt) so it can be tested without rendering.
    /// </summary>
    public class SideScrollCookie : MonoBehaviour
    {
        public Transform leader;
        public Animator animator;
        public AudioClip[] pawsteps, collar;
        public float followSpeed = 4.4f, chargeSpeed = 10f;
        public float followOffset = 1.6f;            // trots this far behind Marcus
        public float biteRange = 0.9f;
        public float biteDamage = 28f, biteStagger = 1.4f;
        public float cooldownSeconds = 7f;
        public float catchUpDistance = 12f;

        public CookieState State { get; private set; } = CookieState.Following;
        public bool IsReady => State == CookieState.Following && _cooldown <= 0f;
        public float CurrentSpeed { get; private set; }
        public int BitesLanded { get; private set; }

        private SideScrollEnemy _target;
        private float _cooldown, _biteTimer, _stepTimer, _collarTimer;
        private bool _wasReady = true;
        private AudioSource _steps, _collar, _voice;

        private void Awake()
        {
            _steps = MakeSource("PawSteps"); _collar = MakeSource("Collar"); _voice = MakeSource("CookieVoice");
        }

        private AudioSource MakeSource(string n)
        {
            var go = new GameObject(n); go.transform.SetParent(transform, false); go.transform.localPosition = Vector3.up * 0.4f;
            var s = go.AddComponent<AudioSource>();
            s.spatialBlend = 1f; s.rolloffMode = AudioRolloffMode.Linear; s.minDistance = 2f; s.maxDistance = 25f; s.playOnAwake = false;
            return s;
        }

        private void Update() => Tick(Time.deltaTime);

        public void Attack(SideScrollEnemy enemy)
        {
            if (!IsReady || enemy == null || enemy.IsDown) return;
            _target = enemy; State = CookieState.Charging;
            _voice.PlayOneShot(SideScrollTones.Bark, 0.9f);
        }

        public void Tick(float dt)
        {
            if (leader == null) return;
            if (_cooldown > 0f) _cooldown -= dt;
            if (!_wasReady && IsReady) { _voice.PlayOneShot(SideScrollTones.ReadyChime, 0.6f); AccessibilityManager.Instance?.Announce("Cookie is ready"); }
            _wasReady = IsReady;

            float speed = 0f; float dir = 0f;
            float lx = leader.position.x, x = transform.position.x;
            switch (State)
            {
                case CookieState.Following:
                {
                    float behindX = lx - FacingOf(leader) * followOffset;
                    float d = behindX - x;
                    if (Mathf.Abs(d) > catchUpDistance) { Teleport(behindX); break; }
                    if (Mathf.Abs(d) > 0.3f) { dir = Mathf.Sign(d); speed = followSpeed * Mathf.Clamp01(Mathf.Abs(d) / 1.5f + 0.2f); }
                    break;
                }
                case CookieState.Charging:
                    if (_target == null || _target.IsDown) { State = CookieState.Returning; break; }
                    float dx = _target.transform.position.x - x;
                    if (Mathf.Abs(dx) <= biteRange) { State = CookieState.Biting; _biteTimer = 0.25f; Bite(); break; }
                    dir = Mathf.Sign(dx); speed = chargeSpeed; break;
                case CookieState.Biting:
                    _biteTimer -= dt; if (_biteTimer <= 0f) State = CookieState.Returning; break;
                case CookieState.Returning:
                {
                    float d = lx - x;
                    if (Mathf.Abs(d) <= followOffset + 0.4f) { State = CookieState.Following; _cooldown = cooldownSeconds; _target = null; break; }
                    dir = Mathf.Sign(d); speed = chargeSpeed * 0.7f; break;
                }
            }

            CurrentSpeed = speed;
            if (dir != 0f)
            {
                var p = transform.position; p.x += dir * speed * dt; p.z = 0f;
                if (Physics.Raycast(p + Vector3.up * 2f, Vector3.down, out var hit, 6f)) p.y = hit.point.y;
                transform.position = p;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, dir > 0 ? 90f : -90f, 0), 1f - Mathf.Exp(-16f * dt));
            }
            if (animator != null) animator.SetFloat("Speed", speed);
            Audio(dt, speed);
        }

        private void Bite()
        {
            if (_target == null) return;
            _target.TakeDamage(biteDamage, transform.position, Vector3.zero);
            _target.Stagger(biteStagger);
            BitesLanded++;
            _voice.PlayOneShot(SideScrollTones.Bite, 1f);
        }

        private void Teleport(float x) { var p = transform.position; p.x = x; transform.position = p; }
        private static float FacingOf(Transform t) => Vector3.Dot(t.forward, Vector3.right) >= 0f ? 1f : -1f;

        private void Audio(float dt, float speed)
        {
            _collarTimer += dt;
            if (speed > 0.2f)
            {
                _stepTimer += dt;
                if (_stepTimer >= Mathf.Lerp(0.5f, 0.24f, Mathf.Clamp01(speed / 10f))) { _stepTimer = 0f; PlayRandom(_steps, pawsteps, 0.7f); }
                if (_collarTimer >= 1.0f) { _collarTimer = 0f; PlayRandom(_collar, collar, 0.4f); }
            }
            else if (_collarTimer >= 2.5f) { _collarTimer = 0f; PlayRandom(_collar, collar, 0.22f); }
        }

        private static void PlayRandom(AudioSource s, AudioClip[] c, float v)
        {
            if (s == null || c == null || c.Length == 0) return;
            s.pitch = Random.Range(0.95f, 1.05f); s.PlayOneShot(c[Random.Range(0, c.Length)], v);
        }
    }
}
