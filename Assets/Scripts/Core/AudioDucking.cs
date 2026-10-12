using UnityEngine;

namespace EchoesOfNeon.Core
{
    /// <summary>
    /// Auditory hierarchy: while something critical is being said or heard ("Marcus is down", "Ledge ahead"),
    /// background layers (music, ambience beds) drop so it cannot be masked. Gameplay cues (enemy hums, wind-ups,
    /// locators, Cookie) are NOT ducked - they carry information. Pure state with an explicit clock for tests;
    /// the deepest active duck wins, and the release ramps back over a quarter second so it never pumps.
    /// </summary>
    public static class AudioDucking
    {
        public const float Release = 0.25f;
        private static float _until = float.NegativeInfinity, _depth = 1f;

        /// <summary>Hold background at `multiplier` (0..1) for `seconds` from `now`.</summary>
        public static void Duck(float multiplier, float seconds, float now)
        {
            multiplier = Mathf.Clamp01(multiplier);
            if (now >= _until) { _depth = multiplier; _until = now + seconds; return; }
            _depth = Mathf.Min(_depth, multiplier);
            _until = Mathf.Max(_until, now + seconds);
        }

        public static void Duck(float multiplier, float seconds) => Duck(multiplier, seconds, Time.unscaledTime);

        public static float Multiplier(float now)
        {
            if (now < _until) return _depth;
            float t = (now - _until) / Release;
            return t >= 1f ? 1f : Mathf.Lerp(_depth, 1f, t);
        }

        public static float Multiplier() => Multiplier(Time.unscaledTime);

        public static void Reset() { _until = float.NegativeInfinity; _depth = 1f; }
    }
}
