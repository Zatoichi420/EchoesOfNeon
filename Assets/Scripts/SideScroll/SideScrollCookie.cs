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
        private SideScrollPlayer _pl;
        private float _guardCd, _rallyCd, _pingTimer, _eyeCd, _howlCd, _tandemCd;
        private static SideScrollProgression Prog => SideScrollProgression.Instance;
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
            EchoesOfNeon.Core.AudioRouting.Bind(s, EchoesOfNeon.Core.AudioCategory.Sfx);
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
            if (_pl == null) _pl = leader.GetComponent<SideScrollPlayer>();
            _guardCd -= dt; _rallyCd -= dt; _eyeCd -= dt; _howlCd -= dt; _tandemCd -= dt;
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
                    if (Mathf.Abs(d) <= followOffset + 0.4f) { State = CookieState.Following; _cooldown = Mathf.Max(2f, cooldownSeconds - (Prog != null ? Prog.CookieCooldownReduction : 0f)); _target = null; break; }
                    dir = Mathf.Sign(d); speed = chargeSpeed * 0.7f; break;
                }
            }

            if (State == CookieState.Following) Augments(dt);
            if (State == CookieState.Charging && Prog != null && Prog.Has(CookieAbility.PackLink) && _pl != null && _pl.abilities != null && _pl.abilities.IsOverclocked) speed *= 1.4f;

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
            bool titan = Prog != null && Prog.Has(CookieAbility.TitanBite);
            bool link = Prog != null && Prog.Has(CookieAbility.PackLink) && _pl != null && _pl.abilities != null && _pl.abilities.IsOverclocked;
            float dmg = biteDamage * (Prog != null ? Prog.CookieBiteMultiplier : 1f) * (titan ? 1.6f : 1f) * (link ? 1.5f : 1f);
            bool blocked = _target.Hit(dmg, transform.position.x, titan, true);       // Titan Bite breaks a Shield Guard's guard
            if (!blocked) _target.Stagger(biteStagger);
            BitesLanded++;
            _voice.PlayOneShot(titan ? SideScrollTones.TitanBite : SideScrollTones.Bite, 1f);
            Prog?.AwardCookieXp(6);
            if (_pl != null && _pl.abilities != null) _pl.abilities.AddSpecial(5f);
        }

        // ------------------------------------------------------------- cybernetic augments

        private void Augments(float dt)
        {
            if (Prog == null || _pl == null) return;
            // Rally (Lv7): heals a badly hurt Marcus
            if (Prog.Has(CookieAbility.Rally) && _rallyCd <= 0f && _pl.Health > 0f && _pl.Health < _pl.MaxHealth * 0.3f)
            { _rallyCd = 25f; _voice.PlayOneShot(SideScrollTones.Rally, 0.8f); _pl.Heal(15f, "Cookie nuzzles you"); }
            // Collar Ping (Lv2): chimes from the nearest threat's position so he can locate it by ear
            if (Prog.Has(CookieAbility.CollarPing) && (_pingTimer -= dt) <= 0f)
            {
                _pingTimer = 4f; var e = NearestEnemy(12f);
                if (e != null && AcousticEventSystemAvailable()) EchoesOfNeon.Core.AcousticEventSystem.Instance.PlayOneShotAt(SideScrollTones.CollarPing, e.transform.position + Vector3.up, 16f);
            }
            // Cyber-Eye (Lv4): calls out ledges and fliers
            if (Prog.Has(CookieAbility.CyberEye) && _eyeCd <= 0f)
            {
                var edge = leader.GetComponent<SideScrollEdgeSensor>();
                if (edge != null && edge.NearestGapDistance >= 0f && edge.NearestGapDistance <= 4f) { _eyeCd = 4f; EchoesOfNeon.Accessibility.AccessibilityManager.Instance?.Announce("Ledge ahead"); }
                else
                {
                    foreach (var e in SideScrollEnemy.All)
                        if (e != null && !e.IsDown && e.aerial && Mathf.Abs(e.transform.position.x - leader.position.x) < 8f) { _eyeCd = 4f; EchoesOfNeon.Accessibility.AccessibilityManager.Instance?.Announce("Enemy above"); break; }
                }
            }
        }

        private static bool AcousticEventSystemAvailable() => EchoesOfNeon.Core.AcousticEventSystem.Instance != null;

        private SideScrollEnemy NearestEnemy(float range)
        {
            SideScrollEnemy b = null; float bd = range;
            foreach (var e in SideScrollEnemy.All) { if (e == null || e.IsDown) continue; float d = Mathf.Abs(e.transform.position.x - transform.position.x); if (d < bd) { bd = d; b = e; } }
            return b;
        }

        /// <summary>Guard Mode (Lv6): she bodyblocks and absorbs ONE hit aimed at Marcus every 20 s.</summary>
        public bool TryAbsorbHit(Vector3 from)
        {
            if (Prog == null || !Prog.Has(CookieAbility.GuardMode) || _guardCd > 0f || State != CookieState.Following) return false;
            if (Mathf.Abs(transform.position.x - leader.position.x) > 3f) return false;
            _guardCd = 20f; _voice.PlayOneShot(SideScrollTones.Block, 1f);
            EchoesOfNeon.Accessibility.AccessibilityManager.Instance?.Announce("Cookie takes the hit");
            return true;
        }

        /// <summary>Shock Howl (Lv9): a sonic stun on everything within 8 m of Cookie.</summary>
        public bool Howl()
        {
            if (Prog == null || !Prog.Has(CookieAbility.ShockHowl)) { EchoesOfNeon.Accessibility.AccessibilityManager.Instance?.Announce("Cookie has not learned that yet"); return false; }
            if (_howlCd > 0f) { EchoesOfNeon.Accessibility.AccessibilityManager.Instance?.Announce("Cookie needs a moment"); return false; }
            _howlCd = 14f; _voice.PlayOneShot(SideScrollTones.Howl, 1f);
            foreach (var e in SideScrollEnemy.All) if (e != null && !e.IsDown && Mathf.Abs(e.transform.position.x - transform.position.x) <= 8f) e.Stagger(2f);
            Prog.AwardCookieXp(4);
            return true;
        }

        /// <summary>Tandem Strike (Lv10): when Marcus fires a heavy special, Cookie adds a free bite on the nearest enemy.</summary>
        public bool TandemStrike()
        {
            if (Prog == null || !Prog.Has(CookieAbility.TandemStrike) || _tandemCd > 0f) return false;
            var e = NearestEnemy(10f); if (e == null) return false;
            _tandemCd = 6f; e.Hit(biteDamage * Prog.CookieBiteMultiplier, transform.position.x, true, true); e.Stagger(1f);
            _voice.PlayOneShot(SideScrollTones.TitanBite, 1f); Prog.AwardCookieXp(4);
            return true;
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
