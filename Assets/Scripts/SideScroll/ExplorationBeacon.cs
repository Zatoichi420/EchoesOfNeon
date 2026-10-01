using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using UnityEngine;

namespace EchoesOfNeon.SideScroll
{
    /// <summary>Exploration reward: a bright ping marks an undiscovered spot. Reaching it awards XP once (to both Marcus
    /// and Cookie) and speaks a short description.</summary>
    public class ExplorationBeacon : LocatorObject
    {
        public int xp = 30; public int cookieXp = 15; public string description = "Area discovered";
        protected override AudioClip BeepClip => SideScrollTones.ExplorePing;
        protected override void OnReached()
        {
            Consumed = true;
            Src.PlayOneShot(SideScrollTones.ExplorePing, 1f);
            AccessibilityManager.Instance?.Announce(description + ". Experience gained.");
            SideScrollProgression.Instance?.AwardShared(xp, cookieXp);
            Destroy(gameObject, 1f);
        }
    }
}
