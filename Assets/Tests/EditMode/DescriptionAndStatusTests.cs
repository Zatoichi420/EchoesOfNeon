using EchoesOfNeon.Accessibility;
using EchoesOfNeon.UI;
using NUnit.Framework;
using UnityEngine.Accessibility;

namespace EchoesOfNeon.Tests
{
    public class DescriptionAndStatusTests
    {
        [Test]
        public void Compose_ButtonSliderToggle()
        {
            Assert.AreEqual("Start Game, button",
                AccessibleDescription.Compose("Start Game", AccessibilityRole.Button, null, AccessibilityState.None, null));
            Assert.AreEqual("Volume, slider, 0.5",
                AccessibleDescription.Compose("Volume", AccessibilityRole.Slider, "0.5", AccessibilityState.None, null));
            Assert.AreEqual("Subtitles, toggle, on",
                AccessibleDescription.Compose("Subtitles", AccessibilityRole.Toggle, "on", AccessibilityState.Selected, null));
        }

        [Test]
        public void Compose_DisabledSelectedAndHint()
        {
            Assert.AreEqual("Continue, button, dimmed",
                AccessibleDescription.Compose("Continue", AccessibilityRole.Button, null, AccessibilityState.Disabled, null));
            Assert.AreEqual("Audio, tab, selected",
                AccessibleDescription.Compose("Audio", AccessibilityRole.TabButton, null, AccessibilityState.Selected, null));
            Assert.AreEqual("Jump, button, Press Enter to rebind",
                AccessibleDescription.Compose("Jump", AccessibilityRole.Button, null, AccessibilityState.None, "Press Enter to rebind"));
        }

        [Test]
        public void StatusRegistry_ComposesProviders_SkipsEmptyAndBroken()
        {
            StatusRegistry.Clear();
            StatusRegistry.Register("a", () => "One.");
            StatusRegistry.Register("b", () => "");
            StatusRegistry.Register("c", () => throw new System.Exception("boom"));
            StatusRegistry.Register("d", () => "Two.");
            Assert.AreEqual("One. Two.", StatusRegistry.Compose());
            StatusRegistry.Unregister("a");
            Assert.AreEqual("Two.", StatusRegistry.Compose());
            StatusRegistry.Clear();
        }
    }
}
