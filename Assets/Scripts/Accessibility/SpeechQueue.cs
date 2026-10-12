using System.Collections.Generic;

namespace EchoesOfNeon.Accessibility
{
    public enum SpeechPriority { Low = 0, Normal = 1, High = 2, Critical = 3 }

    /// <summary>
    /// Decides what gets spoken when several announcements compete, so a menu or pickup message can never cut off
    /// "Marcus is down". Pure logic with an injected clock, so it is unit-testable.
    /// Rules: an idle queue speaks at once. While speaking, a message of equal or higher priority interrupts
    /// (Normal and above only); a lower one waits its turn. Critical is never interrupted except by Critical.
    /// Waiting messages expire (Low 3s, Normal 6s) so stale chatter is dropped, the queue holds at most 4, and
    /// the next to speak is always the highest priority, oldest first.
    /// </summary>
    public class SpeechQueue
    {
        private struct Item { public string Text; public SpeechPriority Priority; public float At; }

        public const int MaxPending = 4;
        private readonly List<Item> _pending = new List<Item>();
        private SpeechPriority _current;
        private float _busyUntil;

        public int PendingCount => _pending.Count;
        public SpeechPriority CurrentPriority => _current;

        /// <summary>Rough speaking time, so the queue knows when the voice is free.</summary>
        public static float EstimateSeconds(string text) => 0.35f + 0.065f * (text?.Length ?? 0);

        /// <summary>Returns the text to speak right now, or null if it was queued or dropped.</summary>
        public string Submit(string text, SpeechPriority priority, float now)
        {
            if (string.IsNullOrEmpty(text)) return null;
            bool busy = now < _busyUntil;
            bool interrupts = busy && priority >= SpeechPriority.Normal && priority >= _current
                              && (_current != SpeechPriority.Critical || priority == SpeechPriority.Critical);
            if (!busy || interrupts) return Begin(text, priority, now);

            _pending.Add(new Item { Text = text, Priority = priority, At = now });
            while (_pending.Count > MaxPending) _pending.RemoveAt(LowestOldest());
            return null;
        }

        /// <summary>Call every frame. Returns the next queued text once the voice is free, else null.</summary>
        public string Tick(float now)
        {
            if (now < _busyUntil || _pending.Count == 0) return null;
            _pending.RemoveAll(i => Expired(i, now));
            if (_pending.Count == 0) return null;
            int best = 0;
            for (int i = 1; i < _pending.Count; i++)
                if (_pending[i].Priority > _pending[best].Priority) best = i;   // strict >, so oldest wins ties
            var item = _pending[best]; _pending.RemoveAt(best);
            return Begin(item.Text, item.Priority, now);
        }

        private string Begin(string text, SpeechPriority priority, float now)
        {
            _current = priority; _busyUntil = now + EstimateSeconds(text);
            return text;
        }

        private static bool Expired(Item i, float now) =>
            (i.Priority == SpeechPriority.Low && now - i.At > 3f) || (i.Priority == SpeechPriority.Normal && now - i.At > 6f);

        private int LowestOldest()
        {
            int worst = 0;
            for (int i = 1; i < _pending.Count; i++)
                if (_pending[i].Priority < _pending[worst].Priority) worst = i;
            return worst;
        }
    }
}
