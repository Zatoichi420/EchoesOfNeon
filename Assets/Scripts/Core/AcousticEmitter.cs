using UnityEngine;

namespace EchoesOfNeon.Core
{
    /// <summary>Attach to anything that makes noise - gunfire, footsteps,
    /// mechanical alerts. Call Emit() from whatever gameplay code triggers
    /// the sound (a weapon firing, a footstep timer, an AI bark) - this
    /// component only knows how loud/what kind, not when.</summary>
    public class AcousticEmitter : MonoBehaviour
    {
        [SerializeField] private AcousticEventType eventType = AcousticEventType.Mechanical;
        [SerializeField] private float loudness = 15f; // world-space radius this event should be perceptible within

        public void Emit()
        {
            if (AcousticEventSystem.Instance == null) return;
            AcousticEventSystem.Instance.Emit(transform.position, eventType, loudness, transform);
        }

        public void Emit(float loudnessOverride)
        {
            if (AcousticEventSystem.Instance == null) return;
            AcousticEventSystem.Instance.Emit(transform.position, eventType, loudnessOverride, transform);
        }
    }
}
