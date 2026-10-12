using UnityEngine;

namespace EchoesOfNeon.Core
{
    /// <summary>
    /// Muffles a sound when solid level geometry sits between it and the player's ears, so a guard behind a wall
    /// sounds behind a wall instead of reading as an open-ground threat. Built-in Unity audio only (a low-pass
    /// filter), no middleware. Characters never occlude: any CharacterController, and anything belonging to the
    /// sound's own object or to the ear's owner, is ignored. Checks ten times a second; the cutoff glides.
    /// The ear is Marcus's head (set by the player), falling back to the AudioListener.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioOcclusion : MonoBehaviour
    {
        public static Transform Ear;

        public const float OpenCutoff = 22000f, BlockedCutoff = 900f;
        public float checkInterval = 0.1f, glide = 12f;
        public LayerMask mask = ~0;

        public bool IsBlocked { get; private set; }
        public float Cutoff => _lp != null ? _lp.cutoffFrequency : OpenCutoff;

        private AudioLowPassFilter _lp;
        private float _timer;
        private static readonly RaycastHit[] Hits = new RaycastHit[8];

        public static AudioOcclusion Attach(AudioSource source)
        {
            if (source == null) return null;
            var o = source.GetComponent<AudioOcclusion>();
            return o != null ? o : source.gameObject.AddComponent<AudioOcclusion>();
        }

        public static float TargetCutoff(bool blocked) => blocked ? BlockedCutoff : OpenCutoff;

        /// <summary>True only for real geometry: not a character, not part of the sound's own object or the listener's owner.</summary>
        public static bool IsBlocking(Collider c, Transform sourceRoot, Transform earRoot)
        {
            if (c == null || c is CharacterController) return false;
            var t = c.transform;
            if (sourceRoot != null && t.IsChildOf(sourceRoot)) return false;
            if (earRoot != null && t.IsChildOf(earRoot)) return false;
            return true;
        }

        private void Awake()
        {
            _lp = GetComponent<AudioLowPassFilter>();
            if (_lp == null) _lp = gameObject.AddComponent<AudioLowPassFilter>();
            _lp.cutoffFrequency = OpenCutoff;
        }

        private void Update() => Tick(Time.deltaTime);

        public void Tick(float dt)
        {
            if ((_timer -= dt) <= 0f) { _timer = checkInterval; IsBlocked = Check(); }
            float target = TargetCutoff(IsBlocked);
            // Glide in log-frequency space so open->muffled sounds even.
            float logNow = Mathf.Log(Mathf.Max(20f, _lp.cutoffFrequency)), logTarget = Mathf.Log(target);
            _lp.cutoffFrequency = Mathf.Exp(Mathf.Lerp(logNow, logTarget, 1f - Mathf.Exp(-glide * dt)));
        }

        private bool Check()
        {
            var ear = Ear;
            if (ear == null) { var l = FindAnyObjectByType<AudioListener>(); if (l != null) ear = l.transform; }
            if (ear == null) return false;
            Vector3 from = ear.position, to = transform.position, d = to - from;
            float dist = d.magnitude;
            if (dist < 0.01f) return false;
            int n = Physics.RaycastNonAlloc(from, d / dist, Hits, dist, mask, QueryTriggerInteraction.Ignore);
            var sourceRoot = transform.root; var earRoot = ear.root;
            for (int i = 0; i < n; i++)
                if (IsBlocking(Hits[i].collider, sourceRoot, earRoot)) return true;
            return false;
        }
    }
}
