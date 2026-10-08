using System;
using System.Collections.Generic;
using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using UnityEngine;

namespace EchoesOfNeon.UI
{
    /// <summary>Turns sounds into on-screen text for players who can't hear
    /// them. Two kinds: spoken lines (subtitles, from the transcript table)
    /// and non-speech sound captions ("[Gunfire, to your left]"). Each is
    /// gated by its own setting. CaptionDisplay draws what this raises.</summary>
    public static class CaptionService
    {
        public static event Action<string> OnCaption;

        /// <summary>Transcripts for recorded lines, keyed by clip name.
        /// Source of truth: Docs/Story/Dialogue/scripts/enemy-barks-script.md.</summary>
        private static readonly Dictionary<string, string> Transcripts = new Dictionary<string, string>
        {
            { "bark_patrol_01", "...another dead shift." },
            { "bark_patrol_02", "Ought to run this whole block on autopilot." },
            { "bark_investigate_01", "Who's there?" },
            { "bark_investigate_02", "Thought I heard something." },
            { "bark_alert_01", "Contact! Get eyes on him!" },
            { "bark_alert_02", "He's here — move!" },
        };

        private static readonly Dictionary<AcousticEventType, float> LastSoundCaptionTime = new Dictionary<AcousticEventType, float>();
        private const float MinSecondsBetweenSameSound = 1.5f;

        public static string TranscriptFor(string clipName) =>
            clipName != null && Transcripts.TryGetValue(clipName, out var t) ? t : null;

        /// <summary>Eight-way plain-language direction from a signed angle
        /// (0 = straight ahead, positive = to the right, +-180 = behind).</summary>
        public static string BearingWord(float signedAngle)
        {
            float a = Mathf.Repeat(signedAngle + 180f, 360f) - 180f; // -180..180
            float abs = Mathf.Abs(a);
            string side = a >= 0f ? "right" : "left";
            if (abs < 22.5f) return "ahead";
            if (abs < 67.5f) return "ahead and to your " + side;
            if (abs < 112.5f) return "to your " + side;
            if (abs < 157.5f) return "behind and to your " + side;
            return "behind you";
        }

        public static string FormatSpoken(string speaker, string line, string direction) =>
            string.IsNullOrEmpty(direction) ? $"{speaker}: \"{line}\"" : $"{speaker} ({direction}): \"{line}\"";

        public static string FormatSound(AcousticEventType type, string direction)
        {
            string name = type == AcousticEventType.Gunfire ? "Gunfire"
                        : type == AcousticEventType.Footstep ? "Footsteps"
                        : "Mechanical noise";
            return string.IsNullOrEmpty(direction) ? $"[{name}]" : $"[{name}, {direction}]";
        }

        /// <summary>A recorded line was just played at worldPos.</summary>
        public static void SpokenLine(string speaker, string clipName, Vector3 worldPos)
        {
            var settings = AccessibilityManager.Instance != null ? AccessibilityManager.Instance.Settings : null;
            if (settings == null || !settings.subtitlesEnabled) return;
            var line = TranscriptFor(clipName);
            if (line == null) return; // unknown clip: say nothing rather than invent text
            OnCaption?.Invoke(FormatSpoken(speaker, line, DirectionTo(worldPos)));
        }

        /// <summary>An in-world noise happened (from AcousticEventSystem).</summary>
        public static void SoundEvent(AcousticEvent evt)
        {
            var settings = AccessibilityManager.Instance != null ? AccessibilityManager.Instance.Settings : null;
            if (settings == null || !settings.soundCaptionsEnabled) return;

            float now = Time.unscaledTime;
            if (LastSoundCaptionTime.TryGetValue(evt.Type, out var last) && now - last < MinSecondsBetweenSameSound) return;
            LastSoundCaptionTime[evt.Type] = now;
            OnCaption?.Invoke(FormatSound(evt.Type, DirectionTo(evt.Position)));
        }

        public static void ResetForTests() => LastSoundCaptionTime.Clear();

        private static string DirectionTo(Vector3 worldPos)
        {
            var listener = UnityEngine.Object.FindFirstObjectByType<AudioListener>();
            if (listener == null) return null;
            Vector3 to = worldPos - listener.transform.position; to.y = 0f;
            Vector3 fwd = listener.transform.forward; fwd.y = 0f;
            if (to.sqrMagnitude < 0.01f || fwd.sqrMagnitude < 0.01f) return null;
            return BearingWord(Vector3.SignedAngle(fwd, to, Vector3.up));
        }
    }
}
