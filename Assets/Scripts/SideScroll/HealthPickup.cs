using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using UnityEngine;

namespace EchoesOfNeon.SideScroll
{
    /// <summary>Med-cell: restores a chunk of health (default 25) when Marcus walks into it. A different pitch than
    /// other locators; not collected at full health (it says so and keeps beeping) so it is never wasted.</summary>
    public class HealthPickup : LocatorObject
    {
        public float healAmount = 25f;
        protected override float BeepPitch => 1.5f;
        protected override void OnReached()
        {
            if (player.Health >= player.MaxHealth - 0.01f)
            { if (!_told) { _told = true; AccessibilityManager.Instance?.Announce("Health is full"); } return; }
            player.Heal(healAmount, "Med-cell");
            Consumed = true; Destroy(gameObject, 0.05f);
        }
        private bool _told;
    }
}
