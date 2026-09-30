using EchoesOfNeon.Accessibility;
using UnityEngine;

namespace EchoesOfNeon.Player
{
    /// <summary>
    /// Turns the real player's movement (CharacterController velocity + AccessibilityManager sprint state) into
    /// the Animator "Speed" float on Marcus's body model: 0 = Idle, ~1.4 = Walk, ~3.6 = Run (the animator
    /// controller's thresholds are 0.1 and 3.0). Uses unscaled time like TacticalPlayerController, so Neural
    /// Dilate (which slows Time.timeScale) does not slow Marcus's own body animation.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerAnimationBridge : MonoBehaviour
    {
        public Animator animator;
        public float walkAnimSpeed = 1.4f;
        public float runAnimSpeed = 3.6f;
        public float smoothing = 12f;              // Speed-parameter units per second
        public string speedParam = "Speed";

        public float SpeedParameter { get; private set; }
        private CharacterController _controller;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (animator != null)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                animator.applyRootMotion = false;
            }
        }

        private void Update()
        {
            var v = _controller.velocity; v.y = 0f;
            bool moving = _controller.isGrounded && v.magnitude > 0.1f;
            var acc = AccessibilityManager.Instance;
            Tick(Time.unscaledDeltaTime, moving, acc != null && acc.IsSprinting);
        }

        public void Tick(float dt, bool moving, bool sprinting)
        {
            float target = !moving ? 0f : (sprinting ? runAnimSpeed : walkAnimSpeed);
            SpeedParameter = Mathf.MoveTowards(SpeedParameter, target, smoothing * dt);
            if (animator != null) animator.SetFloat(speedParam, SpeedParameter);
        }
    }
}
