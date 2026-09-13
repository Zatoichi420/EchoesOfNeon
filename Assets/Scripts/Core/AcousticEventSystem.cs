using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfNeon.Core
{
    public enum AcousticEventType { Gunfire, Footstep, Mechanical }

    /// <summary>Color coding per MEMORY.md's design pillars: orange =
    /// Gunfire/Explosions, cyan = Footsteps/Movement, yellow = Mechanical/
    /// Alert Barks - SoundVisualizerCompass reads this same mapping.</summary>
    public readonly struct AcousticEvent
    {
        public readonly Vector3 Position;
        public readonly AcousticEventType Type;
        public readonly float Loudness; // world-space radius this event is meant to be perceptible within
        public readonly Transform Source;

        public AcousticEvent(Vector3 position, AcousticEventType type, float loudness, Transform source)
        {
            Position = position;
            Type = type;
            Loudness = loudness;
            Source = source;
        }
    }

    /// <summary>
    /// Phase 5: single source of truth for "something made noise here" -
    /// decoupled event bus, same architecture as InputManager/
    /// OculusSensorySuite. AcousticEmitter calls Emit(); SoundVisualizerCompass
    /// (UI) and, later, TacticalEnemyAI (Phase 6) are the things that listen.
    ///
    /// Also plays a real 3D-spatialized audio cue per event - Assets has no
    /// SFX yet, so a short procedurally generated tone stands in per type
    /// until real sound design exists. This is the actual accessibility
    /// payload for a blind player (stereo/spatial panning conveys direction
    /// independent of any HUD); the compass is a secondary visual aid for
    /// other conditions (colorblind/low-vision), not what makes the game
    /// playable without sight - that's this class's audio path plus the
    /// Oculus Sensory Suite's sonar.
    /// </summary>
    public class AcousticEventSystem : MonoBehaviour
    {
        public static AcousticEventSystem Instance { get; private set; }

        public event Action<AcousticEvent> OnAcousticEvent;

        [Header("Placeholder Tone Pitches (Hz) - until real SFX exist")]
        [SerializeField] private float gunfirePitch = 220f;
        [SerializeField] private float footstepPitch = 90f;
        [SerializeField] private float mechanicalPitch = 440f;
        [SerializeField] private float toneDuration = 0.12f;

        [Header("Spatialization")]
        [SerializeField] private float minAudibleDistance = 2f;

        private readonly Dictionary<AcousticEventType, AudioClip> _placeholderClips = new Dictionary<AcousticEventType, AudioClip>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildPlaceholderClips();
        }

        private void BuildPlaceholderClips()
        {
            _placeholderClips[AcousticEventType.Gunfire] = GenerateTone(gunfirePitch, toneDuration);
            _placeholderClips[AcousticEventType.Footstep] = GenerateTone(footstepPitch, toneDuration * 0.5f);
            _placeholderClips[AcousticEventType.Mechanical] = GenerateTone(mechanicalPitch, toneDuration);
        }

        private static AudioClip GenerateTone(float frequency, float duration)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
            var samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                // Linear fade-out envelope so the placeholder tone doesn't click at the end.
                float envelope = 1f - (i / (float)sampleCount);
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.5f;
            }
            var clip = AudioClip.Create($"PlaceholderTone_{frequency}Hz", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>Raised by AcousticEmitter. loudness is the world-space
        /// radius this event is meant to be perceptible within - listeners
        /// decide what to do with events outside their own range.</summary>
        public void Emit(Vector3 position, AcousticEventType type, float loudness, Transform source)
        {
            var evt = new AcousticEvent(position, type, loudness, source);
            OnAcousticEvent?.Invoke(evt);
            PlayPlaceholderTone(evt);
        }

        private void PlayPlaceholderTone(AcousticEvent evt)
        {
            if (!_placeholderClips.TryGetValue(evt.Type, out var clip) || clip == null) return;

            // A dedicated one-shot AudioSource, not AudioSource.PlayClipAtPoint -
            // spatialBlend must be forced to 1 (fully 3D) so stereo panning
            // actually conveys direction; that's the real accessibility
            // payload here, not a nice-to-have.
            var go = new GameObject("AcousticOneShot");
            go.transform.position = evt.Position;
            var source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = 1f;
            source.minDistance = minAudibleDistance;
            source.maxDistance = Mathf.Max(evt.Loudness, minAudibleDistance + 1f);
            source.rolloffMode = AudioRolloffMode.Linear;
            source.Play();
            Destroy(go, clip.length + 0.1f);
        }
    }
}
