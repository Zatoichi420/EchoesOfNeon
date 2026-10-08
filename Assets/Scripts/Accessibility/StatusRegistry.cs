using System;
using System.Collections.Generic;

namespace EchoesOfNeon.Accessibility
{
    /// <summary>Systems register a short spoken summary of their own state
    /// here; the Status key reads them all out. Lets a player pull
    /// information they missed instead of depending on one-shot announcements.</summary>
    public static class StatusRegistry
    {
        private static readonly Dictionary<string, Func<string>> Providers = new Dictionary<string, Func<string>>();

        public static int Count => Providers.Count;

        public static void Register(string key, Func<string> provider) { if (provider != null) Providers[key] = provider; }
        public static void Unregister(string key) => Providers.Remove(key);
        public static void Clear() => Providers.Clear();

        public static string Compose()
        {
            var parts = new List<string>();
            foreach (var p in Providers.Values)
            {
                string s;
                try { s = p(); } catch { continue; }
                if (!string.IsNullOrWhiteSpace(s)) parts.Add(s.Trim());
            }
            return string.Join(" ", parts);
        }
    }
}
