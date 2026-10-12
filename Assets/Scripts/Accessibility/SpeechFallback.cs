using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace EchoesOfNeon.Accessibility
{
    /// <summary>Speaks text itself when no screen reader is running, using
    /// the OS voice.
    /// - macOS: the built-in `say` command, one process per line.
    /// - Windows: one long-lived hidden PowerShell process driving the
    ///   built-in System.Speech synthesizer (SAPI). Lines go in on stdin, so
    ///   there is no per-line startup delay; each new line cancels the last,
    ///   matching `say`'s new-interrupts-old behaviour.
    /// Other platforms return false. Skipped in batch mode so automated test
    /// runs never talk out loud. Platform is checked at runtime (not #if) so
    /// the Windows path compiles, and is unit-tested, on a Mac.</summary>
    public static class SpeechFallback
    {
        public static bool Enabled = true;

        private static Process _current;   // macOS: the running `say`
        private static Process _windows;   // Windows: the persistent speech host

        private static bool IsMac =>
            Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor;
        private static bool IsWindows =>
            Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor;

        public static bool IsSupported => Enabled && !Application.isBatchMode && (IsMac || IsWindows);

        public static bool Speak(string text)
        {
            if (string.IsNullOrEmpty(text) || !IsSupported) return false;
            return IsWindows ? SpeakWindows(text) : SpeakMac(text);
        }

        /// <summary>Cuts off whatever is being spoken (new focus interrupts old).</summary>
        public static void Stop()
        {
            try { if (_current != null && !_current.HasExited) _current.Kill(); }
            catch { /* already gone */ }
            _current = null;
            if (_windows != null) SendLine("");   // empty line = cancel only
        }

        // ------------------------------------------------------------- macOS

        private static bool SpeakMac(string text)
        {
            Stop();
            try
            {
                var info = new ProcessStartInfo("/usr/bin/say") { UseShellExecute = false, CreateNoWindow = true };
                info.ArgumentList.Add("--");
                info.ArgumentList.Add(text);
                _current = Process.Start(info);
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning("[SpeechFallback] could not start 'say': " + e.Message);
                return false;
            }
        }

        // ------------------------------------------------------------- Windows

        /// <summary>The speech host. Reads lines until stdin closes; each line
        /// cancels current speech, and a non-empty line is then spoken.</summary>
        public const string WindowsHostScript =
            "Add-Type -AssemblyName System.Speech;" +
            "$s = New-Object System.Speech.Synthesis.SpeechSynthesizer;" +
            "$s.SetOutputToDefaultAudioDevice();" +
            "while (($l = [Console]::In.ReadLine()) -ne $null) {" +
            " $s.SpeakAsyncCancelAll();" +
            " if ($l.Length -gt 0) { [void]$s.SpeakAsync($l) } }";

        /// <summary>PowerShell -EncodedCommand wants base64 of UTF-16LE. Using it
        /// (rather than a script file) avoids execution-policy prompts and quoting.</summary>
        public static string EncodeForPowerShell(string script) =>
            Convert.ToBase64String(Encoding.Unicode.GetBytes(script));

        /// <summary>One line per utterance: newlines in the text would split it.</summary>
        public static string ToSingleLine(string text) =>
            text.Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ').Trim();

        private static bool SpeakWindows(string text)
        {
            if (!EnsureWindowsHost()) return false;
            return SendLine(ToSingleLine(text));
        }

        private static bool EnsureWindowsHost()
        {
            if (_windows != null && !_windows.HasExited) return true;
            try
            {
                var info = new ProcessStartInfo("powershell.exe")
                {
                    UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                };
                info.ArgumentList.Add("-NoProfile");
                info.ArgumentList.Add("-NonInteractive");
                info.ArgumentList.Add("-EncodedCommand");
                info.ArgumentList.Add(EncodeForPowerShell(WindowsHostScript));
                _windows = Process.Start(info);
                if (_windows == null) return false;
                _windows.StandardInput.AutoFlush = true;
                Application.quitting -= ShutdownWindowsHost;
                Application.quitting += ShutdownWindowsHost;
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning("[SpeechFallback] could not start Windows speech: " + e.Message);
                _windows = null;
                return false;
            }
        }

        private static bool SendLine(string line)
        {
            try { _windows.StandardInput.WriteLine(line); return true; }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning("[SpeechFallback] Windows speech host gone: " + e.Message);
                _windows = null;   // restarted on the next Speak
                return false;
            }
        }

        private static void ShutdownWindowsHost()
        {
            try { if (_windows != null && !_windows.HasExited) { _windows.StandardInput.Close(); _windows.WaitForExit(500); if (!_windows.HasExited) _windows.Kill(); } }
            catch { /* already gone */ }
            _windows = null;
        }
    }
}
