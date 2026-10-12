using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using UnityEngine;

namespace EchoesOfNeon.SideScroll
{
    /// <summary>Shared base: a world object the player finds by ear. While Marcus is within hearing range it plays a
    /// slow locator beep from its own position (3D, so pan = left/right) that speeds up as he nears (Super Liam's
    /// pickup-locator idea, our own sounds). Subclasses decide what happens on reaching it.</summary>
    public abstract class LocatorObject : MonoBehaviour
    {
        public SideScrollPlayer player;
        public float hearRange = 14f, reachX = 0.9f, reachY = 1.6f;
        public float slowInterval = 1.1f, fastInterval = 0.25f;
        protected AudioSource Src;
        private float _t;

        protected virtual AudioClip BeepClip => SideScrollTones.Locator;
        protected virtual float BeepPitch => 1f;
        protected bool Consumed;

        protected virtual void Awake()
        {
            Src = gameObject.AddComponent<AudioSource>(); Src.spatialBlend = 1f; Src.rolloffMode = AudioRolloffMode.Linear;
            Src.minDistance = 2f; Src.maxDistance = hearRange + 2f; Src.playOnAwake = false;
            EchoesOfNeon.Core.AudioRouting.Bind(Src, EchoesOfNeon.Core.AudioCategory.Sfx);
            EchoesOfNeon.Core.AudioOcclusion.Attach(Src);
        }

        protected bool Reached()
        {
            if (player == null || player.IsDead) return false;
            var d = player.transform.position - transform.position;
            return Mathf.Abs(d.x) <= reachX && Mathf.Abs(d.y) <= reachY;
        }

        protected virtual void Update() => Tick(Time.deltaTime);

        public virtual void Tick(float dt)
        {
            if (Consumed || player == null) return;
            float dx = Mathf.Abs(player.transform.position.x - transform.position.x);
            if (dx <= hearRange)
            {
                _t += dt;
                if (_t >= Mathf.Lerp(fastInterval, slowInterval, Mathf.Clamp01(dx / hearRange))) { _t = 0f; Src.pitch = BeepPitch; Src.PlayOneShot(BeepClip, 0.8f); }
            }
            if (Reached()) OnReached();
        }

        protected abstract void OnReached();
    }
}
