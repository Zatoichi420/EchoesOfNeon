using System;
using System.Text;
using EchoesOfNeon.Accessibility;
using NUnit.Framework;

namespace EchoesOfNeon.Tests
{
    /// <summary>The Windows speech path can't run on the Mac, so test the pieces that can go wrong silently:
    /// the encoded command must round-trip, and text must stay on one stdin line.</summary>
    public class SpeechFallbackTests
    {
        [Test] public void EncodedCommand_RoundTripsAsUtf16()
        {
            var enc = SpeechFallback.EncodeForPowerShell(SpeechFallback.WindowsHostScript);
            Assert.AreEqual(SpeechFallback.WindowsHostScript, Encoding.Unicode.GetString(Convert.FromBase64String(enc)));
        }

        [Test] public void HostScript_UsesSystemSpeech_AndCancelsBeforeSpeaking()
        {
            StringAssert.Contains("System.Speech", SpeechFallback.WindowsHostScript);
            Assert.Less(SpeechFallback.WindowsHostScript.IndexOf("SpeakAsyncCancelAll", StringComparison.Ordinal),
                        SpeechFallback.WindowsHostScript.IndexOf("SpeakAsync($l)", StringComparison.Ordinal));
        }

        [Test] public void MultiLineText_BecomesOneLine() =>
            Assert.AreEqual("Health 40. Low health", SpeechFallback.ToSingleLine("Health 40.\r\nLow health\n"));

        [Test] public void BatchMode_NeverSpeaks() =>
            Assert.IsFalse(SpeechFallback.Speak("test"), "Test runs are batch mode and must stay silent");
    }
}
