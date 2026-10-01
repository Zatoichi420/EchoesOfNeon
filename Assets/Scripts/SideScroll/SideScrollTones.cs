using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfNeon.SideScroll
{
    /// <summary>
    /// Procedural placeholder cues for the side-scroller prototype (same stand-in approach as AcousticEventSystem's
    /// tones, until real sound design). Every gameplay-critical event has its own distinct timbre/pitch contour,
    /// because in a side-scroller the player reads the game purely by ear: windup = rising, hit = low thud,
    /// fend = bright metallic clack, ledge = short high tick, whistle = falling-rising sweep.
    /// </summary>
    public static class SideScrollTones
    {
        private const int Rate = 44100;
        private static readonly Dictionary<string, AudioClip> Cache = new Dictionary<string, AudioClip>();

        public static AudioClip Windup => Get("windup", () => Sweep(220f, 660f, 0.45f, 0.45f));
        public static AudioClip Swing => Get("swing", () => Noise(0.12f, 0.35f, 1500f));
        public static AudioClip CaneHit => Get("canehit", () => Thud(140f, 0.16f, 0.8f));
        public static AudioClip Fend => Get("fend", () => Clack(1250f, 0.22f));
        public static AudioClip PlayerHurt => Get("hurt", () => Thud(90f, 0.3f, 0.9f));
        public static AudioClip EnemyDown => Get("down", () => Sweep(300f, 70f, 0.5f, 0.6f));
        public static AudioClip Whistle => Get("whistle", () => Sweep(900f, 1700f, 0.5f, 0.4f, vibrato: true));
        public static AudioClip Bark => Get("bark", () => Sweep(420f, 260f, 0.18f, 0.8f, harsh: true));
        public static AudioClip Bite => Get("bite", () => Thud(220f, 0.12f, 0.9f));
        public static AudioClip ReadyChime => Get("ready", () => Sweep(700f, 1400f, 0.25f, 0.3f));
        public static AudioClip LedgeTick => Get("ledge", () => Sine(1800f, 0.06f, 0.35f));
        public static AudioClip Land => Get("land", () => Thud(110f, 0.1f, 0.5f));
        public static AudioClip Jump => Get("jump", () => Sweep(200f, 420f, 0.12f, 0.3f));
        public static AudioClip EnemyHum => Get("hum", () => Hum(140f, 1.0f));
        public static AudioClip Sonar => Get("sonar", () => Sweep(500f, 500f, 0.2f, 0.3f));

        // ---- recovery / progression / abilities ----
        public static AudioClip Locator => Get("locator", () => Sine(1200f, 0.07f, 0.4f));
        public static AudioClip HealGet => Get("healget", () => Sweep(500f, 1000f, 0.3f, 0.5f));
        public static AudioClip RestHum => Get("resthum", () => Hum(220f, 1.0f));
        public static AudioClip RestDone => Get("restdone", () => Sweep(400f, 800f, 0.7f, 0.5f));
        public static AudioClip ExplorePing => Get("explore", () => Sweep(900f, 1350f, 0.25f, 0.4f));
        public static AudioClip LevelUp => Get("levelup", () => Sweep(400f, 1600f, 0.8f, 0.55f));
        public static AudioClip SpecialReady => Get("specready", () => Sweep(1000f, 1500f, 0.2f, 0.4f));
        public static AudioClip EchoStep => Get("echostep", () => Noise(0.18f, 0.5f, 800f));
        public static AudioClip FlashBlind => Get("flash", () => Sweep(2400f, 600f, 0.4f, 0.6f, harsh: true));
        public static AudioClip SonicPulse => Get("pulse", () => Thud(60f, 0.5f, 1f));
        public static AudioClip Overclock => Get("overclock", () => Sweep(150f, 900f, 0.6f, 0.6f));
        public static AudioClip Heartbeat => Get("heartbeat", () => Build("heartbeat", 0.9f, (t, p) => (Mathf.Sin(2 * Mathf.PI * 60f * t) * (Mathf.Exp(-t * 22f) + 0.7f * Mathf.Exp(-(t - 0.28f) * 22f) * (t > 0.28f ? 1f : 0f))) * 0.7f));
        public static AudioClip StrainWarn => Get("strain", () => Sweep(300f, 180f, 0.5f, 0.5f, vibrato: true));
        public static AudioClip Overstrain => Get("overstrain", () => Sweep(800f, 80f, 0.8f, 0.8f, harsh: true));
        public static AudioClip ChainHit => Get("chain", () => Thud(300f, 0.12f, 0.9f));
        public static AudioClip EchoStorm => Get("storm", () => Sweep(100f, 1800f, 1.0f, 0.8f, harsh: true));
        public static AudioClip Howl => Get("howl", () => Sweep(500f, 900f, 0.9f, 0.7f, vibrato: true));
        public static AudioClip TitanBite => Get("titan", () => Thud(70f, 0.35f, 1f));
        public static AudioClip CollarPing => Get("collarping", () => Sine(2200f, 0.12f, 0.35f));
        public static AudioClip Block => Get("block", () => Clack(500f, 0.2f));
        public static AudioClip LockOn => Get("lockon", () => Sine(1500f, 0.1f, 0.4f));
        public static AudioClip Bottle => Get("bottle", () => Sweep(600f, 1400f, 0.5f, 0.35f));
        public static AudioClip GunCharge => Get("guncharge", () => Sweep(300f, 1800f, 0.8f, 0.35f, harsh: true));
        public static AudioClip GunShot => Get("gunshot", () => Thud(500f, 0.1f, 0.9f));
        public static AudioClip DroneHum => Get("drone", () => Hum(330f, 1.0f));
        public static AudioClip DroneDive => Get("dronedive", () => Sweep(300f, 1400f, 0.6f, 0.5f));
        public static AudioClip Slam => Get("slam", () => Thud(45f, 0.5f, 1f));
        public static AudioClip Rally => Get("rally", () => Sweep(600f, 900f, 0.4f, 0.4f, vibrato: true));

        /// <summary>Per-archetype windup / hum / bark clips - each street enemy has its own audio signature.</summary>
        public static AudioClip WindupFor(int kind, float from, float to, float seconds) => Get("wu" + kind, () => Sweep(from, to, seconds, 0.45f, harsh: kind % 2 == 0));
        public static AudioClip HumFor(int kind, float hz) => Get("hum" + kind, () => Hum(hz, 1.0f));
        public static AudioClip BarkFor(int kind, float from, float to) => Get("bark" + kind, () => Sweep(from, to, 0.18f, 0.7f, harsh: true));

        private static AudioClip Get(string key, System.Func<AudioClip> make)
        {
            if (!Cache.TryGetValue(key, out var c) || c == null) { c = make(); Cache[key] = c; }
            return c;
        }

        private static AudioClip Build(string name, float seconds, System.Func<float, float, float> f)
        {
            int n = Mathf.Max(1, Mathf.RoundToInt(Rate * seconds));
            var d = new float[n];
            for (int i = 0; i < n; i++) d[i] = f(i / (float)Rate, i / (float)n);
            var clip = AudioClip.Create("SS_" + name, n, 1, Rate, false);
            clip.SetData(d, 0);
            return clip;
        }

        private static AudioClip Sine(float hz, float dur, float vol) =>
            Build("sine" + hz, dur, (t, p) => Mathf.Sin(2 * Mathf.PI * hz * t) * (1 - p) * vol);

        private static AudioClip Sweep(float from, float to, float dur, float vol, bool vibrato = false, bool harsh = false)
        {
            float phase = 0f;
            return Build("sweep" + from + "_" + to, dur, (t, p) =>
            {
                float hz = Mathf.Lerp(from, to, p) * (vibrato ? 1f + 0.04f * Mathf.Sin(2 * Mathf.PI * 18f * t) : 1f);
                phase += 2 * Mathf.PI * hz / Rate;
                float s = Mathf.Sin(phase);
                if (harsh) s = Mathf.Sign(s) * 0.6f + s * 0.4f;
                float env = Mathf.Min(1f, p * 20f) * (1 - p * 0.6f);
                return s * env * vol;
            });
        }

        private static AudioClip Thud(float hz, float dur, float vol) =>
            Build("thud" + hz, dur, (t, p) => (Mathf.Sin(2 * Mathf.PI * hz * (1 - 0.5f * p) * t) + 0.3f * (Random.value * 2 - 1)) * Mathf.Pow(1 - p, 2.5f) * vol);

        private static AudioClip Clack(float hz, float dur) =>
            Build("clack", dur, (t, p) => (Mathf.Sin(2 * Mathf.PI * hz * t) + 0.6f * Mathf.Sin(2 * Mathf.PI * hz * 2.76f * t)) * Mathf.Pow(1 - p, 4f) * 0.5f);

        private static AudioClip Noise(float dur, float vol, float cutoff) =>
            Build("noise", dur, (t, p) => (Random.value * 2 - 1) * Mathf.Sin(Mathf.PI * p) * vol);

        private static AudioClip Hum(float hz, float dur) =>
            Build("hum", dur, (t, p) => (Mathf.Sin(2 * Mathf.PI * hz * t) + 0.4f * Mathf.Sin(2 * Mathf.PI * hz * 2f * t)) * 0.25f);
    }
}
