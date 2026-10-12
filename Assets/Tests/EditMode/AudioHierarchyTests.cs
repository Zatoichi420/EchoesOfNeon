using EchoesOfNeon.Core;
using NUnit.Framework;

namespace EchoesOfNeon.Tests
{
    public class AudioHierarchyTests
    {
        [SetUp] public void SetUp() => AudioDucking.Reset();
        [TearDown] public void TearDown() => AudioDucking.Reset();

        [Test] public void NoDuck_IsFullVolume() => Assert.AreEqual(1f, AudioDucking.Multiplier(5f));

        [Test] public void Duck_HoldsThenReleasesSmoothly()
        {
            AudioDucking.Duck(0.35f, 1f, 0f);
            Assert.AreEqual(0.35f, AudioDucking.Multiplier(0.5f), 1e-4);
            float mid = AudioDucking.Multiplier(1f + AudioDucking.Release / 2f);
            Assert.Greater(mid, 0.35f); Assert.Less(mid, 1f);
            Assert.AreEqual(1f, AudioDucking.Multiplier(2f), 1e-4);
        }

        [Test] public void OverlappingDucks_KeepTheDeepestAndLongest()
        {
            AudioDucking.Duck(0.5f, 2f, 0f);
            AudioDucking.Duck(0.2f, 0.5f, 0.1f);
            Assert.AreEqual(0.2f, AudioDucking.Multiplier(1.5f), 1e-4);
        }

        [Test] public void Occlusion_BlockedIsMuffled() =>
            Assert.Less(AudioOcclusion.TargetCutoff(true), 2000f);

        [Test] public void Occlusion_OpenIsUnfiltered() =>
            Assert.GreaterOrEqual(AudioOcclusion.TargetCutoff(false), 20000f);
    }
}
