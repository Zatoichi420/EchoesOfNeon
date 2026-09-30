using UnityEngine;

namespace EchoesOfNeon.Player
{
    /// <summary>
    /// Test/scripted locomotion: walks a character around a loop of waypoints, pausing at each one, and drives
    /// the Animator's "Speed" float (Idle below 0.1, Walk above, Run above 3.0). Movement lives in Tick(dt)
    /// so tests can step it deterministically without Play Mode; Update just forwards Time.deltaTime.
    /// Not the player controller - the real one (TacticalPlayerController) is separate.
    /// </summary>
    public class WaypointWalker : MonoBehaviour
    {
        public Transform[] waypoints;
        public Animator animator;
        public float walkSpeed = 1.4f;
        public float runSpeed = 3.6f;
        public float pauseSeconds = 2f;
        public bool[] runToWaypoint;          // same length as waypoints; true = run the leg that ends at it
        public string speedParam = "Speed";
        public float turnDegreesPerSecond = 360f;

        public float CurrentSpeed { get; private set; }
        public int TargetIndex { get; private set; }
        private float _pause;

        private void Update() => Tick(Time.deltaTime);

        public void Tick(float dt)
        {
            if (waypoints == null || waypoints.Length == 0) return;
            float speed = 0f;
            if (_pause > 0f) _pause -= dt;
            else
            {
                var target = waypoints[TargetIndex].position; target.y = transform.position.y;
                var to = target - transform.position;
                float dist = to.magnitude;
                bool run = runToWaypoint != null && TargetIndex < runToWaypoint.Length && runToWaypoint[TargetIndex];
                speed = run ? runSpeed : walkSpeed;
                float step = speed * dt;
                if (dist <= step) { transform.position = target; TargetIndex = (TargetIndex + 1) % waypoints.Length; _pause = pauseSeconds; speed = 0f; }
                else
                {
                    transform.position += to / dist * step;
                    var want = Quaternion.LookRotation(to / dist, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, want, turnDegreesPerSecond * dt);
                }
            }
            CurrentSpeed = speed;
            if (animator != null) animator.SetFloat(speedParam, speed);
        }
    }
}
