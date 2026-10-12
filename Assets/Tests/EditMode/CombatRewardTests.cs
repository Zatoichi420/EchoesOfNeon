using EchoesOfNeon.SideScroll;
using NUnit.Framework;

namespace EchoesOfNeon.Tests
{
    public class CombatRewardTests
    {
        [Test] public void HealthyPair_LowRoll_Drops() =>
            Assert.AreNotEqual(RewardKind.None, CombatRewardSpawner.Decide(0.1f, 0.2f, 1f, 1f, 0));

        [Test] public void HealthyPair_HighRoll_NoDrop() =>
            Assert.AreEqual(RewardKind.None, CombatRewardSpawner.Decide(0.9f, 0.2f, 1f, 1f, 0));

        [Test] public void HurtRaisesTheOdds() =>
            Assert.AreNotEqual(RewardKind.None, CombatRewardSpawner.Decide(0.5f, 0.2f, 0.3f, 1f, 0));

        [Test] public void DryStreak_GuaranteesADrop() =>
            Assert.AreNotEqual(RewardKind.None, CombatRewardSpawner.Decide(0.99f, 0.2f, 1f, 1f, 3));

        [Test] public void PicksTheOneWhoNeedsItMore()
        {
            Assert.AreEqual(RewardKind.MarcusPack, CombatRewardSpawner.Decide(0f, 0.9f, 0.2f, 0.8f, 0));
            Assert.AreEqual(RewardKind.CookieTreat, CombatRewardSpawner.Decide(0f, 0.1f, 0.8f, 0.2f, 0));
        }

        [Test] public void DownedCookie_GetsTheTreat() =>
            Assert.AreEqual(RewardKind.CookieTreat, CombatRewardSpawner.Decide(0f, 0.1f, 0.9f, 0f, 0));
    }
}
