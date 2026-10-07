using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using EchoesOfNeon.UI;
using NUnit.Framework;
using System;

namespace EchoesOfNeon.Tests
{
    public class CompassPaletteTests
    {
        [Test]
        public void EveryMode_GivesEachSoundTypeADistinctColor()
        {
            foreach (ColorblindMode mode in Enum.GetValues(typeof(ColorblindMode)))
            {
                var g = CompassPalette.ColorFor(AcousticEventType.Gunfire, mode);
                var f = CompassPalette.ColorFor(AcousticEventType.Footstep, mode);
                var m = CompassPalette.ColorFor(AcousticEventType.Mechanical, mode);
                Assert.AreNotEqual(g, f, mode.ToString());
                Assert.AreNotEqual(g, m, mode.ToString());
                Assert.AreNotEqual(f, m, mode.ToString());
            }
        }

        [Test]
        public void SizeDiffersBySoundType_SoColorIsNeverTheOnlyCue()
        {
            var g = CompassPalette.SizeScaleFor(AcousticEventType.Gunfire);
            var f = CompassPalette.SizeScaleFor(AcousticEventType.Footstep);
            var m = CompassPalette.SizeScaleFor(AcousticEventType.Mechanical);
            Assert.AreNotEqual(g, f);
            Assert.AreNotEqual(g, m);
            Assert.AreNotEqual(f, m);
        }
    }
}
