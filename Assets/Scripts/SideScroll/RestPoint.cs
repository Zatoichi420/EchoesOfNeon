using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using UnityEngine;

namespace EchoesOfNeon.SideScroll
{
    /// <summary>Rest point (Max's workshop, a safe nook): a steady hum marks it. Standing in it and pressing Interact
    /// restores full health, clears strain, and makes it the respawn point. A tone and "Rested" confirm it.</summary>
    public class RestPoint : LocatorObject
    {
        public string label = "Rest point";
        private InputManager _input;
        public int TimesRested { get; private set; }
        protected override float BeepPitch => 0.6f;
        protected override void Awake()
        {
            base.Awake(); slowInterval = 2.0f; fastInterval = 0.9f; reachX = 1.5f;
            Src.loop = true; Src.clip = SideScrollTones.RestHum; Src.volume = 0.25f; Src.Play();
        }
        private void OnEnable() { _input = InputManager.Instance; if (_input != null) _input.OnInteract += Interact; }
        private void OnDisable() { if (_input != null) _input.OnInteract -= Interact; }
        public override void Tick(float dt) { /* hum is continuous; no beeps, no auto-collect */ }
        public bool InRange => Reached();
        public void Interact()
        {
            if (!InRange) return;
            player.Rest(transform.position); TimesRested++;
            SideScrollProgression.Instance?.Save();
        }
        protected override void OnReached() { }
    }
}
