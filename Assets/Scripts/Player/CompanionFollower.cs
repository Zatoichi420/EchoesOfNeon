using UnityEngine;

namespace EchoesOfNeon.Player
{
    /// <summary>
    /// Keeps a companion (Cookie) beside/behind a leader (Marcus): closes the gap when it is larger than
    /// followDistance, stops inside it, and speeds up to catch up when the leader runs. Drives the same
    /// "Speed" animator float. Movement is in Tick(dt) so tests can step it without Play Mode.
    /// </summary>
    public class CompanionFollower : MonoBehaviour
    {
        public Transform leader;
        public Animator animator;
        public float followDistance = 1.6f;
        public float walkSpeed = 1.5f;
        public float maxSpeed = 4.2f;
        public float catchUpGain = 2.5f;      // extra m/s per metre of gap beyond followDistance
        public string speedParam = "Speed";
        public float turnDegreesPerSecond = 540f;

        public float CurrentSpeed { get; private set; }

        private void Update() => Tick(Time.deltaTime);

        public void Tick(float dt)
        {
            if (leader == null) return;
            var to = leader.position - transform.position; to.y = 0f;
            float dist = to.magnitude;
            float speed = 0f;
            if (dist > followDistance + 0.15f)
                speed = Mathf.Clamp(walkSpeed + (dist - followDistance) * catchUpGain, walkSpeed, maxSpeed);
            if (speed > 0f)
            {
                float step = Mathf.Min(speed * dt, dist - followDistance);
                transform.position += to / dist * Mathf.Max(step, 0f);
                var want = Quaternion.LookRotation(to / dist, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, want, turnDegreesPerSecond * dt);
            }
            CurrentSpeed = speed;
            if (animator != null) animator.SetFloat(speedParam, speed);
        }
    }
}
