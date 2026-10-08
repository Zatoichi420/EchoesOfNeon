using System.IO;
using EchoesOfNeon.Core;
using EchoesOfNeon.UI;
using NUnit.Framework;
using UnityEngine;

namespace EchoesOfNeon.Tests
{
    public class CaptionServiceTests
    {
        [TestCase(0f, "ahead")]
        [TestCase(45f, "ahead and to your right")]
        [TestCase(-45f, "ahead and to your left")]
        [TestCase(90f, "to your right")]
        [TestCase(-90f, "to your left")]
        [TestCase(135f, "behind and to your right")]
        [TestCase(-135f, "behind and to your left")]
        [TestCase(180f, "behind you")]
        [TestCase(-180f, "behind you")]
        [TestCase(350f, "ahead")]
        public void BearingWord_NamesTheDirection(float angle, string expected) =>
            Assert.AreEqual(expected, CaptionService.BearingWord(angle));

        [Test]
        public void Formats()
        {
            Assert.AreEqual("Guard (to your left): \"Who's there?\"", CaptionService.FormatSpoken("Guard", "Who's there?", "to your left"));
            Assert.AreEqual("Guard: \"Hi\"", CaptionService.FormatSpoken("Guard", "Hi", null));
            Assert.AreEqual("[Gunfire, behind you]", CaptionService.FormatSound(AcousticEventType.Gunfire, "behind you"));
            Assert.AreEqual("[Footsteps]", CaptionService.FormatSound(AcousticEventType.Footstep, null));
        }

        [Test]
        public void EveryRecordedBark_HasATranscript()
        {
            var dir = Path.Combine(Application.dataPath, "Audio", "Dialogue");
            var barks = Directory.GetFiles(dir, "bark_*.mp3");
            Assert.Greater(barks.Length, 0);
            foreach (var f in barks)
                Assert.IsNotNull(CaptionService.TranscriptFor(Path.GetFileNameWithoutExtension(f)),
                    Path.GetFileName(f) + " has no transcript in CaptionService - deaf players would get nothing for this line.");
        }
    }
}
