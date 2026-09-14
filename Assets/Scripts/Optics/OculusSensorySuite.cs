using System;
using System.Collections;
using System.Collections.Generic;
using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using UnityEngine;

namespace EchoesOfNeon.Optics
{
    /// <summary>Implement on anything a sonar pulse should reveal (enemies,
    /// interactables, sound sources). Kept decoupled from
    /// OculusSensorySuite - no VFX/audio system exists yet (Phase 5/6) to
    /// react to a ping, so this is the seam a future one hooks into.</summary>
    public interface ISonarPingable
    {
        void OnSonarPing(float delaySeconds, float distance);
    }

    /// <summary>
    /// Phase 4: Marcus's cybernetic Oculus Sensory Suite - sonar-pulse
    /// echolocation, Neural Dilate (perception-overclock slow-mo), and
    /// predictive ballistics. This is gameplay accessibility, not screen-
    /// reader accessibility (see AccessibilityManager for that) - per the
    /// project's design philosophy, these are the in-lore mechanics that
    /// make actual play possible without sight, not a menu option.
    /// </summary>
    public class OculusSensorySuite : MonoBehaviour
    {
        public static OculusSensorySuite Instance { get; private set; }

        [Header("References")]
        [SerializeField] private Transform originTransform; // camera/head - sonar center, ballistics origin

        [Header("Sonar Pulse")]
        [SerializeField] private float sonarRadius = 25f;
        [SerializeField] private float sonarWaveSpeed = 15f; // units/sec - controls each hit's reveal delay
        [SerializeField] private float sonarCooldown = 1.5f;
        [SerializeField] private LayerMask sonarMask = ~0;

        [Header("Neural Dilate")]
        [SerializeField] private float dilateTransitionSpeed = 6f; // how fast Time.timeScale eases toward its target

        [Header("Predictive Ballistics")]
        [SerializeField] private int maxBounces = 3;
        [SerializeField] private float maxTrajectoryDistance = 60f;
        [SerializeField] private LayerMask ballisticsMask = ~0;

        public event Action<Collider, float, float> OnSonarPingDetected; // hit collider, delaySeconds, distance
        public event Action<bool> OnNeuralDilateStateChanged;
        public event Action<IReadOnlyList<Vector3>> OnTrajectoryUpdated; // empty list = no active preview

        public bool IsDilating { get; private set; }

        private InputManager _input;
        private AccessibilityManager _accessibility;
        private float _sonarCooldownTimer;
        private float _targetTimeScale = 1f;
        private float _baseFixedDeltaTime;
        private readonly List<Vector3> _trajectoryBuffer = new List<Vector3>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _baseFixedDeltaTime = Time.fixedDeltaTime;
        }

        private void Start()
        {
            _input = InputManager.Instance;
            _accessibility = AccessibilityManager.Instance;

            if (_input == null)
            {
                Debug.LogWarning("[OculusSensorySuite] No InputManager found - the suite will not respond to input.");
                return;
            }
            _input.OnSonarPulse += HandleSonarPulse;
            _input.OnNeuralDilatePressed += HandleDilatePressed;
            _input.OnNeuralDilateReleased += HandleDilateReleased;
        }

        private void OnDestroy()
        {
            if (_input != null)
            {
                _input.OnSonarPulse -= HandleSonarPulse;
                _input.OnNeuralDilatePressed -= HandleDilatePressed;
                _input.OnNeuralDilateReleased -= HandleDilateReleased;
            }
            // Don't leave the game permanently in slow motion if this object
            // goes away mid-dilate (scene change, etc).
            if (IsDilating)
            {
                Time.timeScale = 1f;
                Time.fixedDeltaTime = _baseFixedDeltaTime;
            }
        }

        private void Update()
        {
            if (_sonarCooldownTimer > 0f) _sonarCooldownTimer -= Time.unscaledDeltaTime;
            UpdateTimeScale();
            UpdateTrajectoryPrediction();
        }

        // --- Sonar Pulse ---

        private void HandleSonarPulse()
        {
            if (_sonarCooldownTimer > 0f) return;
            _sonarCooldownTimer = sonarCooldown;

            Vector3 origin = originTransform != null ? originTransform.position : transform.position;
            var hits = Physics.OverlapSphere(origin, sonarRadius, sonarMask);
            foreach (var hit in hits)
            {
                float distance = Vector3.Distance(origin, hit.transform.position);
                float delay = distance / Mathf.Max(sonarWaveSpeed, 0.01f);
                OnSonarPingDetected?.Invoke(hit, delay, distance);

                var pingable = hit.GetComponentInParent<ISonarPingable>();
                if (pingable != null)
                    StartCoroutine(DispatchPingAfterDelay(pingable, delay, distance));
            }
        }

        // Not static (was before) - needs _accessibility to announce the
        // contact at the same moment it actually "answers back" via its own
        // AcousticEmitter, not a raw OnSonarPingDetected event fired for
        // every collider in the sonar radius (ground, walls, props included),
        // which would be spammy. Only pingable hits reach this coroutine at all.
        private IEnumerator DispatchPingAfterDelay(ISonarPingable pingable, float delay, float distance)
        {
            yield return new WaitForSecondsRealtime(delay);
            pingable.OnSonarPing(delay, distance);
            _accessibility?.Announce($"Sonar contact, {distance:F0} meters.");
        }

        // --- Neural Dilate ---

        private void HandleDilatePressed()
        {
            IsDilating = true;
            float target = _accessibility != null ? _accessibility.Settings.neuralDilateTimescale : 0.3f;
            _targetTimeScale = Mathf.Clamp(target, 0.05f, 1f);
            OnNeuralDilateStateChanged?.Invoke(true);
            // Time.timeScale changes globally with zero non-visual signal
            // otherwise - a blind player would have no way to know the world
            // just slowed down around them.
            _accessibility?.Announce("Neural Dilate engaged.");
        }

        private void HandleDilateReleased()
        {
            IsDilating = false;
            _targetTimeScale = 1f;
            OnNeuralDilateStateChanged?.Invoke(false);
            _accessibility?.Announce("Neural Dilate disengaged.");
        }

        private void UpdateTimeScale()
        {
            if (Mathf.Approximately(Time.timeScale, _targetTimeScale)) return;

            Time.timeScale = Mathf.MoveTowards(Time.timeScale, _targetTimeScale, dilateTransitionSpeed * Time.unscaledDeltaTime);

            // Keep the real-world FixedUpdate rate constant by scaling
            // fixedDeltaTime alongside timeScale (Unity's own recommended
            // pattern for slow-mo) - otherwise physics gets choppy at low
            // timeScale instead of smoothly slow.
            Time.fixedDeltaTime = _baseFixedDeltaTime * Time.timeScale;
        }

        // --- Predictive Ballistics ---

        private void UpdateTrajectoryPrediction()
        {
            bool aiming = _accessibility != null && _accessibility.IsAiming;
            if (!aiming || originTransform == null)
            {
                if (_trajectoryBuffer.Count > 0)
                {
                    _trajectoryBuffer.Clear();
                    OnTrajectoryUpdated?.Invoke(_trajectoryBuffer);
                }
                return;
            }

            _trajectoryBuffer.Clear();
            Vector3 pos = originTransform.position;
            Vector3 dir = originTransform.forward;
            _trajectoryBuffer.Add(pos);
            float remaining = maxTrajectoryDistance;

            for (int bounce = 0; bounce <= maxBounces && remaining > 0f; bounce++)
            {
                if (Physics.Raycast(pos, dir, out var hit, remaining, ballisticsMask))
                {
                    _trajectoryBuffer.Add(hit.point);
                    remaining -= hit.distance;
                    dir = Vector3.Reflect(dir, hit.normal);
                    pos = hit.point + dir * 0.01f; // nudge off the surface so the next raycast doesn't immediately re-hit it
                }
                else
                {
                    _trajectoryBuffer.Add(pos + dir * remaining);
                    remaining = 0f;
                }
            }

            OnTrajectoryUpdated?.Invoke(_trajectoryBuffer);
        }
    }
}
