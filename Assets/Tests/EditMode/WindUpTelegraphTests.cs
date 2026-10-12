using EchoesOfNeon.SideScroll;
using NUnit.Framework;
using UnityEngine;

namespace EchoesOfNeon.Tests
{
    /// <summary>Every audio wind-up needs a visual twin (zero-audio play). Only the wind-up shows the marker.</summary>
    public class WindUpTelegraphTests
    {
        [Test] public void ShowsOnlyDuringWindUp()
        {
            foreach (EnemyState s in System.Enum.GetValues(typeof(EnemyState)))
                Assert.AreEqual(s == EnemyState.WindUp, WindUpTelegraph.ShouldShow(s), s.ToString());
        }

        [Test] public void GrowsTowardTheStrike()
        {
            Assert.AreEqual(0.6f, WindUpTelegraph.ScaleAt(0f, 1f), 1e-4);
            Assert.AreEqual(1f, WindUpTelegraph.ScaleAt(1f, 1f), 1e-4);
            Assert.AreEqual(1f, WindUpTelegraph.ScaleAt(0f, 0f), 1e-4);
        }

        [Test] public void UnblockableDiffersByShape_NotJustColour()
        {
            Assert.AreNotEqual(WindUpTelegraph.ShapeFor(true), WindUpTelegraph.ShapeFor(false));
            Assert.AreNotEqual(WindUpTelegraph.FendableColor, WindUpTelegraph.UnblockableColor);
        }

        [Test] public void NeverUsesTheCyanAllyColour()
        {
            foreach (var c in new[] { WindUpTelegraph.FendableColor, WindUpTelegraph.UnblockableColor })
                Assert.Less(c.b, 0.5f, "Cyan/blue is reserved for allies and cybernetics");
        }
    }
}
