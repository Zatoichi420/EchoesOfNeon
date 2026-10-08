using System;
using EchoesOfNeon.Accessibility;
using UnityEngine;

namespace EchoesOfNeon.Player
{
    public enum GuideState { Leading, Waiting, Returning, Arrived }

    /// <summary>
    /// Cookie as a guide dog. She leads the player toward an objective by staying a few metres ahead, and never
    /// forces the player: if Marcus falls behind she stops and waits (looking back at him), resumes when he
    /// closes up, and comes back to him if he wanders far off. Movement lives in Tick(dt) so tests can step it.
    /// States: Leading -> Waiting (player too far behind) -> Leading (player caught up) -> Arrived (at the
    /// objective); Waiting/Arrived -> Returning if the player is very far away, then back to Leading.
    /// Drives the same animator "Speed" float as the other companion scripts. Guiding by ear is CompanionAudio.
    /// Straight-line movement only for now: no pathfinding/obstacle avoidance yet.
    /// </summary>
    public class CompanionGuide : MonoBehaviour
    {
        public Transform player;
        public Transform objective;
        public Animator animator;
        public float guideSpeed = 3.2f;          // slower than the player's walk (4 m/s) so he can catch up while she waits
        public float returnSpeed = 4.5f;
        public float waitDistance = 6f;          // stop and wait when the player is this far behind
        public float resumeDistance = 3.5f;      // resume leading when the player is this close again
        public float returnDistance = 12f;       // come back for the player when he is this far away
        public float arriveRadius = 1.2f;        // count as arrived this close to the objective
        public float turnDegreesPerSecond = 540f;
        public string speedParam = "Speed";

        public GuideState State { get; private set; } = GuideState.Leading;

        private void OnEnable()
        {
            StatusRegistry.Register("cookie", () =>
            {
                if (player == null) return "";
                float d = Vector3.Distance(player.position, transform.position);
                string what = State == GuideState.Leading ? "leading" : State == GuideState.Waiting ? "waiting for you"
                    : State == GuideState.Returning ? "coming back to you" : "at the destination";
                return $"Cookie is {what}, {d:F0} meters away.";
            });
        }

        private void OnDisable() => StatusRegistry.Unregister("cookie");
        public float CurrentSpeed { get; private set; }
        public event Action<GuideState, GuideState> OnStateChanged;   // (from, to)

        private void Update() => Tick(Time.deltaTime);

        public void Tick(float dt)
        {
            if (player == null) return;
            float dPlayer = Flat(player.position - transform.position).magnitude;
            float dObjective = objective != null ? Flat(objective.position - transform.position).magnitude : 0f;
            float speed = 0f; Vector3 face = Vector3.zero;

            switch (State)
            {
                case GuideState.Leading:
                    if (objective == null || dObjective <= arriveRadius) SetState(GuideState.Arrived);
                    else if (dPlayer >= waitDistance) SetState(GuideState.Waiting);
                    else { speed = guideSpeed; face = Flat(objective.position - transform.position); }
                    break;
                case GuideState.Waiting:
                    if (dPlayer >= returnDistance) SetState(GuideState.Returning);
                    else if (dPlayer <= resumeDistance) SetState(GuideState.Leading);
                    else face = Flat(player.position - transform.position);
                    break;
                case GuideState.Returning:
                    if (dPlayer <= resumeDistance) SetState(GuideState.Leading);
                    else { speed = returnSpeed; face = Flat(player.position - transform.position); }
                    break;
                case GuideState.Arrived:
                    if (dPlayer >= returnDistance) SetState(GuideState.Returning);
                    else face = Flat(player.position - transform.position);
                    break;
            }

            if (speed > 0f && face.sqrMagnitude > 1e-6f)
                transform.position += face.normalized * Mathf.Min(speed * dt, face.magnitude);
            if (face.sqrMagnitude > 1e-6f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(face.normalized, Vector3.up), turnDegreesPerSecond * dt);

            CurrentSpeed = speed;
            if (animator != null) animator.SetFloat(speedParam, speed);
        }

        private void SetState(GuideState next)
        {
            if (next == State) return;
            var prev = State; State = next; OnStateChanged?.Invoke(prev, next);
        }

        private static Vector3 Flat(Vector3 v) { v.y = 0f; return v; }
    }
}
