using UnityEngine;

namespace EchoesOfNeon.Player
{
    /// <summary>
    /// Keeps Marcus's mobility cane at a steady angle in the world instead of swinging rigidly with his
    /// hand. The cane is parented to the hand bone so it still follows the hand's position, but a real
    /// held cane is not a pendulum - only its position tracks the hand, its angle stays near-vertical,
    /// tip trailing slightly behind. Rotation-only: no effect on the cane's position or scale.
    /// </summary>
    public class HeldCaneStabilizer : MonoBehaviour
    {
        public Transform cane;
        public Transform facing;                 // the character root; faces +Z
        public float tiltBackDegrees = 20f;

        private void LateUpdate() => Apply();

        public void Apply()
        {
            if (cane == null || facing == null) return;
            var yaw = Quaternion.Euler(0f, facing.eulerAngles.y, 0f);
            cane.rotation = yaw * Quaternion.Euler(-tiltBackDegrees, 0f, 0f);
        }
    }
}
