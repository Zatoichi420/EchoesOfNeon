using EchoesOfNeon.Core;
using UnityEngine;

namespace EchoesOfNeon.Weapons
{
    /// <summary>
    /// Phase 6: the Vanguard 9 suppressed pistol - semi-auto hitscan with
    /// recoil damping and a suppressed (short-radius, not silent) acoustic
    /// footprint via AcousticEmitter (Phase 5).
    ///
    /// Predictive ballistics visualization (the "laser guide") is already
    /// OculusSensorySuite's job (Phase 4, gated on Aim) - this class only
    /// fires the actual shot. Damage goes through IDamageable, the same
    /// decoupled-seam pattern as ISonarPingable/IInteractable.
    ///
    /// Recoil is modeled as raycast-direction bias, not a camera-transform
    /// kick - deliberately doesn't touch TacticalPlayerController's look
    /// code, and matches the project's accessibility-first design (a blind
    /// player relies on audio/sonar feedback, not a visual recoil animation).
    /// </summary>
    [RequireComponent(typeof(AcousticEmitter))]
    public class BallisticWeapon : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform muzzle; // camera pivot - raycast origin/direction

        [Header("Ballistics")]
        [SerializeField] private float damage = 28f;
        [SerializeField] private float range = 60f;
        [SerializeField] private LayerMask hitMask = ~0;

        [Header("Recoil Damping")]
        [SerializeField] private float recoilKickPerShot = 1.5f; // degrees, applied to raycast direction only
        [SerializeField] private float recoilRecoverySpeed = 6f; // degrees/sec
        [SerializeField] private float maxRecoil = 8f;           // caps "climb" on rapid semi-auto fire

        [Header("Suppressed Acoustic Footprint")]
        [SerializeField] private float fireLoudness = 6f; // short radius - it's a suppressor, not silence

        private InputManager _input;
        private AcousticEmitter _emitter;
        private float _currentRecoil;

        private void Awake()
        {
            _emitter = GetComponent<AcousticEmitter>();
        }

        private void Start()
        {
            _input = InputManager.Instance;
            if (_input == null)
            {
                Debug.LogWarning("[BallisticWeapon] No InputManager in scene - weapon disabled.");
                enabled = false;
                return;
            }
            _input.OnFirePressed += Fire; // semi-auto - one shot per press, not per frame held
        }

        private void OnDestroy()
        {
            if (_input != null) _input.OnFirePressed -= Fire;
        }

        private void Update()
        {
            if (_currentRecoil > 0f)
                _currentRecoil = Mathf.MoveTowards(_currentRecoil, 0f, recoilRecoverySpeed * Time.unscaledDeltaTime);
        }

        private void Fire()
        {
            if (muzzle == null) return;

            _currentRecoil = Mathf.Min(_currentRecoil + recoilKickPerShot, maxRecoil);
            Vector3 recoiledDirection = Quaternion.AngleAxis(-_currentRecoil, muzzle.right) * muzzle.forward;

            if (Physics.Raycast(muzzle.position, recoiledDirection, out var hit, range, hitMask))
                hit.collider.GetComponentInParent<IDamageable>()?.TakeDamage(damage, hit.point, hit.normal);

            _emitter.Emit(fireLoudness);
        }
    }
}
