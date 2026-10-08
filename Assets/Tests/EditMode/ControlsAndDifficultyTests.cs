using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using NUnit.Framework;

namespace EchoesOfNeon.Tests
{
    public class ControlsAndDifficultyTests
    {
        [Test]
        public void ControlsGuide_UsesCurrentBindings_AndSkipsUnbound()
        {
            var text = ControlsGuide.Compose(a => a == "Jump" ? "Space" : a == "Move:Up" ? "I" : a == "Fire" ? "-" : null);
            StringAssert.StartsWith("Controls. ", text);
            StringAssert.Contains("Move forward: I.", text);
            StringAssert.Contains("Jump: Space.", text);
            StringAssert.DoesNotContain("Strike or fire", text);
        }

        [Test]
        public void EveryGuideAction_IsAKnownInputAction()
        {
            // Names must match InputManager actions or the guide silently drops them.
            var known = new System.Collections.Generic.HashSet<string>
            { "Move", "Fire", "Aim", "Jump", "Interact", "Sprint", "Crouch", "Reload", "Pause", "SonarPulse", "NeuralDilate",
              "Fend", "Whistle", "EchoStep", "Special", "CycleSpecial", "Howl", "Journal", "Status", "RepeatLast", "Hint" };
            foreach (var (action, _) in ControlsGuide.Entries)
            {
                InputManager.SplitActionName(action, out var name, out _);
                Assert.Contains(name, new System.Collections.Generic.List<string>(known), action);
            }
        }

        [Test]
        public void SplitActionName_HandlesPartsAndPlainNames()
        {
            InputManager.SplitActionName("Move:Up", out var a, out var p);
            Assert.AreEqual("Move", a); Assert.AreEqual("Up", p);
            InputManager.SplitActionName("Jump", out a, out p);
            Assert.AreEqual("Jump", a); Assert.IsNull(p);
        }

        [Test]
        public void Difficulty_TablesMatchNames_AndGetEasierMonotonically()
        {
            Assert.AreEqual(DifficultyRules.DifficultyNames.Length, DifficultyRules.DamageTaken.Length);
            Assert.AreEqual(DifficultyRules.TimingNames.Length, DifficultyRules.EnemyTelegraph.Length);
            Assert.AreEqual(DifficultyRules.TimingNames.Length, DifficultyRules.FendWindow.Length);
            for (int i = 1; i < DifficultyRules.DamageTaken.Length; i++) Assert.Greater(DifficultyRules.DamageTaken[i], DifficultyRules.DamageTaken[i - 1]);
            for (int i = 1; i < DifficultyRules.EnemyTelegraph.Length; i++) Assert.Greater(DifficultyRules.EnemyTelegraph[i], DifficultyRules.EnemyTelegraph[i - 1]);
            Assert.AreEqual(1f, DifficultyRules.DamageTaken[2], "Normal must be 1x");
            Assert.AreEqual(1f, DifficultyRules.EnemyTelegraph[0], "Standard timing must be 1x");
        }

        [Test]
        public void Difficulty_OutOfRangeSettingsAreClamped()
        {
            var s = new AccessibilitySettings { difficulty = 99, timingAssist = -5 };
            Assert.AreEqual(DifficultyRules.DamageTaken[3], DifficultyRules.DamageTakenMultiplier(s));
            Assert.AreEqual(DifficultyRules.EnemyTelegraph[0], DifficultyRules.EnemyTelegraphMultiplier(s));
            Assert.AreEqual(1f, DifficultyRules.DamageTakenMultiplier(null));
        }
    }
}
