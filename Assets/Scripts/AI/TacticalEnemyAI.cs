using System;
using System.Collections.Generic;
using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using EchoesOfNeon.Optics;
using EchoesOfNeon.Weapons;
using UnityEngine;

namespace EchoesOfNeon.AI
{
    /// <summary>
    /// Phase 6: minimal tactical enemy - Patrol/Investigate/Alert state
    /// machine. Simple point-to-point movement, deliberately no NavMesh -
    /// avoids a bake step in batch-mode tooling and is fine for a graybox
    /// test range; revisit if real level geometry needs pathfinding around
    /// obstacles.
    ///
    /// Ties Phases 4-6 together: implements ISonarPingable (Phase 4) so
    /// when the player's sonar reveals this enemy, it "answers back" with a
    /// positional AcousticEmitter event (Phase 5) - a blind player actually
    /// hears where the ping found something, not just feels a generic
    /// pulse. Also subscribes to AcousticEventSystem directly - loud nearby
    /// noise (e.g. unsuppressed gunfire) pulls it into Investigate even
    /// without a sonar ping.
    /// </summary>
    [RequireComponent(typeof(AcousticEmitter))]
    public class TacticalEnemyAI : MonoBehaviour, ISonarPingable, IDamageable
    {
        /// <summary>Public so audio/VFX reactions (EnemyBarkPlayer) can switch
        /// on it without this class needing to know they exist - same
        /// decoupling as ISonarPingable/IDamageable.</summary>
        public enum State { Patrol, Investigate, Alert }

        /// <summary>Fires only on an actual change of state, never on a
        /// same-state re-entry. Transition-specific *narration* deliberately
        /// stays at the call sites below (the wording differs by how the
        /// state was entered - spotted vs. shot at, for instance); this event
        /// is for reactions that only care about the destination state.</summary>
        public event Action<State> OnStateChanged;

        public State CurrentState => _state;

        [Header("Patrol")]
        [SerializeField] private List<Transform> patrolPoints = new List<Transform>();
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float waypointTolerance = 0.3f;

        [Header("Vision")]
        [SerializeField] private Transform eye;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float visionRange = 15f;
        [SerializeField] private float visionAngle = 60f; // full cone angle, degrees
        [SerializeField] private LayerMask visionMask = ~0;

        [Header("Investigate / Alert")]
        [SerializeField] private float investigateSpeed = 3.5f;

        [Header("Health")]
        [SerializeField] private float maxHealth = 60f;

        private State _state = State.Patrol;
        private int _patrolIndex;
        private Vector3 _investigateTarget;
        private float _health;
        private AcousticEmitter _emitter;

        private void Awake()
        {
            _emitter = GetComponent<AcousticEmitter>();
            _health = maxHealth;
        }

        private void OnEnable()
        {
            if (AcousticEventSystem.Instance != null)
                AcousticEventSystem.Instance.OnAcousticEvent += HandleAcousticEvent;
        }

        private void OnDisable()
        {
            if (AcousticEventSystem.Instance != null)
                AcousticEventSystem.Instance.OnAcousticEvent -= HandleAcousticEvent;
        }

        private void Update()
        {
            if (_health <= 0f) return;

            switch (_state)
            {
                case State.Patrol: UpdatePatrol(); break;
                case State.Investigate: UpdateInvestigate(); break;
                case State.Alert: UpdateAlert(); break;
            }

            if (_state != State.Alert && CanSeePlayer() && SetState(State.Alert))
                Announce("Enemy spotted you.");
        }

        private void UpdatePatrol()
        {
            if (patrolPoints.Count == 0) return;
            MoveTowards(patrolPoints[_patrolIndex].position, patrolSpeed);
            if (Vector3.Distance(transform.position, patrolPoints[_patrolIndex].position) <= waypointTolerance)
                _patrolIndex = (_patrolIndex + 1) % patrolPoints.Count;
        }

        private void UpdateInvestigate()
        {
            MoveTowards(_investigateTarget, investigateSpeed);
            if (Vector3.Distance(transform.position, _investigateTarget) <= waypointTolerance
                && SetState(State.Patrol))
                Announce("Enemy stands down.");
        }

        private void UpdateAlert()
        {
            if (playerTransform == null) { SetState(State.Patrol); return; }
            MoveTowards(playerTransform.position, investigateSpeed);
            _investigateTarget = playerTransform.position;
            // Lost sight - go check the last known position rather than instantly forgetting.
            if (!CanSeePlayer() && SetState(State.Investigate))
                Announce("Enemy lost sight of you.");
        }

        private void MoveTowards(Vector3 target, float speed)
        {
            Vector3 flatTarget = new Vector3(target.x, transform.position.y, target.z);
            transform.position = Vector3.MoveTowards(transform.position, flatTarget, speed * Time.deltaTime);
            Vector3 dir = flatTarget - transform.position;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(dir);
        }

        private bool CanSeePlayer()
        {
            if (playerTransform == null || eye == null) return false;
            Vector3 toPlayer = playerTransform.position - eye.position;
            float distance = toPlayer.magnitude;
            if (distance > visionRange) return false;
            if (Vector3.Angle(eye.forward, toPlayer) > visionAngle * 0.5f) return false;
            return !Physics.Raycast(eye.position, toPlayer.normalized, distance, visionMask);
        }

        private void HandleAcousticEvent(AcousticEvent evt)
        {
            if (_state == State.Alert) return; // already knows exactly where the player is
            float distance = Vector3.Distance(transform.position, evt.Position);
            if (distance > evt.Loudness) return; // outside this event's audible radius
            _investigateTarget = evt.Position;
            if (SetState(State.Investigate))
                Announce("Enemy investigating a sound.");
        }

        // --- ISonarPingable (Phase 4) ---
        public void OnSonarPing(float delaySeconds, float distance)
        {
            // OculusSensorySuite's coroutine already waited out the sonar's
            // travel delay before calling this - answer back immediately so
            // a blind player hears exactly where the ping found something.
            _emitter.Emit();
        }

        // --- IDamageable ---
        public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal)
        {
            _health -= amount;
            if (_health <= 0f)
            {
                enabled = false; // no death system yet (Phase 6+) - just stop moving
                Announce("Enemy down.");
                return;
            }
            _investigateTarget = hitPoint;
            if (SetState(State.Alert))
                Announce("Enemy alerted.");
        }

        /// <summary>The single place `_state` is ever written. Returns true
        /// only if the state actually changed, so callers can gate
        /// transition-specific narration on it rather than each tracking its
        /// own "was I already in this state" flag. Fires OnStateChanged for
        /// decoupled reactions (audio barks, future VFX).</summary>
        private bool SetState(State next)
        {
            if (_state == next) return false;
            _state = next;
            OnStateChanged?.Invoke(next);
            return true;
        }

        /// <summary>Every state-change announcement in this file goes through
        /// here rather than calling AccessibilityManager.Announce directly,
        /// so the null-check (no AccessibilityManager in a headless/edit-mode
        /// context) lives in one place. AccessibilityManager.Announce itself
        /// already no-ops quietly when no screen reader is active.</summary>
        private static void Announce(string text)
        {
            if (AccessibilityManager.Instance != null)
                AccessibilityManager.Instance.Announce(text);
        }
    }
}
