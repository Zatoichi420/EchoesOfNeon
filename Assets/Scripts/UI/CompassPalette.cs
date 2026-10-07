using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using UnityEngine;

namespace EchoesOfNeon.UI
{
    /// <summary>Pure color/size rules for SoundVisualizerCompass, kept out of
    /// the MonoBehaviour so EditMode tests can check them. Colorblind sets
    /// are drawn from the Okabe-Ito palette, which stays distinguishable
    /// under the common color-vision deficiencies.</summary>
    public static class CompassPalette
    {
        private static readonly Color Orange = new Color(0.90f, 0.62f, 0.00f);
        private static readonly Color SkyBlue = new Color(0.34f, 0.71f, 0.91f);
        private static readonly Color Vermillion = new Color(0.84f, 0.37f, 0.00f);
        private static readonly Color Pink = new Color(0.80f, 0.47f, 0.65f);

        public static Color ColorFor(AcousticEventType type, ColorblindMode mode)
        {
            switch (mode)
            {
                case ColorblindMode.Protanopia:
                case ColorblindMode.Deuteranopia:
                    return Pick(type, Orange, SkyBlue, Color.white);
                case ColorblindMode.Tritanopia:
                    return Pick(type, Vermillion, Color.white, Pink);
                case ColorblindMode.HighContrastMonochrome:
                    return Pick(type, Color.white, new Color(0.7f, 0.7f, 0.7f), new Color(0.45f, 0.45f, 0.45f));
                default:
                    return Pick(type, new Color(1f, 0.55f, 0f), Color.cyan, Color.yellow);
            }
        }

        /// <summary>Gunfire biggest, footsteps smallest - shape cue that
        /// works with any palette.</summary>
        public static float SizeScaleFor(AcousticEventType type)
        {
            switch (type)
            {
                case AcousticEventType.Gunfire: return 1.4f;
                case AcousticEventType.Footstep: return 0.8f;
                default: return 1f;
            }
        }

        private static Color Pick(AcousticEventType type, Color gunfire, Color footstep, Color mechanical)
        {
            switch (type)
            {
                case AcousticEventType.Gunfire: return gunfire;
                case AcousticEventType.Footstep: return footstep;
                case AcousticEventType.Mechanical: return mechanical;
                default: return Color.white;
            }
        }
    }
}
