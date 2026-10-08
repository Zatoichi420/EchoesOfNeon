using System.Diagnostics;
using UnityEngine;

namespace EchoesOfNeon.Accessibility
{
    /// <summary>Speaks text itself when no screen reader is running, using
    /// the OS voice. Only macOS standalone/editor is implemented (the
    /// built-in `say` command); other platforms return false and stay
    /// silent until a native speech plugin exists. Skipped in batch mode so
    /// automated test runs never talk out loud.</summary>
    public static class SpeechFallback
    {
        public static bool Enabled = true;

        private static Process _current;

        public static bool IsSupported
        {
            get
            {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                return Enabled && !Application.isBatchMode;
#else
                return false;
#endif
            }
        }

        public static bool Speak(string text)
        {
            if (string.IsNullOrEmpty(text) || !IsSupported) return false;
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            Stop();
            try
            {
                var info = new ProcessStartInfo("/usr/bin/say") { UseShellExecute = false, CreateNoWindow = true };
                info.ArgumentList.Add("--");
                info.ArgumentList.Add(text);
                _current = Process.Start(info);
                return true;
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning("[SpeechFallback] could not start 'say': " + e.Message);
                return false;
            }
#else
            return false;
#endif
        }

        /// <summary>Cuts off whatever is being spoken (new focus interrupts old).</summary>
        public static void Stop()
        {
            try { if (_current != null && !_current.HasExited) _current.Kill(); }
            catch { /* already gone */ }
            _current = null;
        }
    }
}
