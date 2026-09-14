using EchoesOfNeon.Core;
using UnityEngine;

namespace EchoesOfNeon.AI
{
    /// <summary>
    /// Plays the spoken enemy barks (generated 2026-09-13, in
    /// Assets/Audio/Dialogue/) off TacticalEnemyAI's state changes. Kept as a
    /// separate component subscribing to OnStateChanged rather than living
    /// inside the AI class, so audio stays decoupled from behaviour - same
    /// reasoning as ISonarPingable/IDamageable.
    ///
    /// These barks are the *diegetic* layer, deliberately distinct from
    /// AccessibilityManager's spoken state announcements ("Enemy alerted"):
    /// the announcement tells a screen-reader user WHAT changed, the bark is a
    /// positional, in-world voice telling them WHERE it came from. Both firing
    /// on the same transition is intentional, not redundancy - but whether
    /// they talk over each other in practice can only be judged by ear, so
    /// it's flagged in Docs/playtest-log.md rather than guessed at here.
    ///
    /// Patrol chatter is on a slow timer rather than a transition, because
    /// that content ("...another dead shift.") is idle muttering, not a
    /// reaction. It doubles as a genuine stealth affordance: a guard who
    /// audibly mutters while patrolling is a guard a blind player can locate
    /// and avoid by ear alone.
    /// </summary>
    [RequireComponent(typeof(TacticalEnemyAI))]
    public class EnemyBarkPlayer : MonoBehaviour
    {
        [Header("Clips (one is picked at random per bark)")]
        [SerializeField] private AudioClip[] patrolBarks;
        [SerializeField] private AudioClip[] investigateBarks;
        [SerializeField] private AudioClip[] alertBarks;

        [Header("Audibility")]
        [Tooltip("World-space radius the bark carries - separate from the AI's own hearing/vision ranges.")]
        [SerializeField] private float barkRadius = 25f;

        [Header("Pacing")]
        [Tooltip("Floor on the gap between any two barks from this enemy, so rapid state flapping can't chatter.")]
        [SerializeField] private float minSecondsBetweenBarks = 2.5f;
        [SerializeField] private float patrolChatterMinInterval = 12f;
        [SerializeField] private float patrolChatterMaxInterval = 25f;

        private TacticalEnemyAI _ai;
        private float _lastBarkTime = float.NegativeInfinity;
        private float _nextPatrolChatterTime;
        private bool _warnedNoAcousticSystem;

        private void Awake()
        {
            _ai = GetComponent<TacticalEnemyAI>();
            ScheduleNextPatrolChatter();
        }

        private void OnEnable()
        {
            if (_ai != null) _ai.OnStateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            if (_ai != null) _ai.OnStateChanged -= HandleStateChanged;
        }

        private void Update()
        {
            // Idle chatter only while actually patrolling, and only when the AI
            // is still alive/active (TacticalEnemyAI disables itself on death,
            // which stops its Update but not necessarily this one).
            if (_ai == null || !_ai.enabled) return;
            if (_ai.CurrentState != TacticalEnemyAI.State.Patrol) return;

            if (Time.time >= _nextPatrolChatterTime)
            {
                PlayRandom(patrolBarks);
                ScheduleNextPatrolChatter();
            }
        }

        private void HandleStateChanged(TacticalEnemyAI.State state)
        {
            switch (state)
            {
                case TacticalEnemyAI.State.Investigate:
                    PlayRandom(investigateBarks);
                    break;
                case TacticalEnemyAI.State.Alert:
                    PlayRandom(alertBarks);
                    break;
                case TacticalEnemyAI.State.Patrol:
                    // Standing down is covered by the accessibility
                    // announcement; a bored patrol mutter the instant an enemy
                    // loses interest would read as a reaction it isn't.
                    // Push the idle timer out so chatter doesn't land
                    // immediately on top of that transition either.
                    ScheduleNextPatrolChatter();
                    break;
            }
        }

        private void ScheduleNextPatrolChatter()
        {
            _nextPatrolChatterTime = Time.time + Random.Range(patrolChatterMinInterval, patrolChatterMaxInterval);
        }

        private void PlayRandom(AudioClip[] clips)
        {
            // An empty array is a legitimate configuration (an enemy type that
            // simply doesn't bark), so it stays silent without complaint.
            if (clips == null || clips.Length == 0) return;
            if (Time.time - _lastBarkTime < minSecondsBetweenBarks) return;

            // A missing AcousticEventSystem is NOT legitimate - it means the
            // scene is misconfigured, and without this warning the only
            // symptom is barks never playing with no reason given. Silent
            // failure is this project's documented recurring bug class (see
            // Docs/code-review-checklist.md), so say something - once per
            // enemy, not once per bark attempt, which would spam every
            // patrol tick.
            if (AcousticEventSystem.Instance == null)
            {
                if (!_warnedNoAcousticSystem)
                {
                    _warnedNoAcousticSystem = true;
                    Debug.LogWarning($"[EnemyBarkPlayer] No AcousticEventSystem in scene - '{name}' will never bark.", this);
                }
                return;
            }

            var clip = clips[Random.Range(0, clips.Length)];
            if (clip == null) return;

            _lastBarkTime = Time.time;
            // Spatialized, and deliberately NOT AcousticEventSystem.Emit() -
            // a bark should be heard, not treated as a noise event other
            // enemies investigate (which would cascade).
            AcousticEventSystem.Instance.PlayOneShotAt(clip, transform.position, barkRadius);
        }
    }
}
