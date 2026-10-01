using UnityEngine;

namespace EchoesOfNeon.SideScroll
{
    /// <summary>
    /// Marcus's cane as an edge probe. Looks for missing ground ahead (in his facing direction) at three
    /// distances and ticks faster and higher-pitched the nearer the drop is: slow tick = gap far ahead, rapid
    /// = a step away. Silence means solid ground. Also exposes the nearest gap distance for tests/UI.
    /// A fast tick is deliberately pitched high so it cuts through enemy hum and footsteps.
    /// </summary>
    [RequireComponent(typeof(SideScrollPlayer))]
    public class SideScrollEdgeSensor : MonoBehaviour
    {
        public float[] probeDistances = { 0.8f, 2f, 4f };
        public float dropDepth = 2.2f;                 // ground further than this below = a gap
        public float slowInterval = 0.9f, fastInterval = 0.14f;

        public float NearestGapDistance { get; private set; } = -1f;   // -1 = solid ground ahead
        public int TicksPlayed { get; private set; }

        private SideScrollPlayer _player;
        private AudioSource _src;
        private float _timer;

        private void Awake()
        {
            _player = GetComponent<SideScrollPlayer>();
            _src = new GameObject("EdgeTicks").AddComponent<AudioSource>();
            _src.transform.SetParent(transform, false); _src.spatialBlend = 0f; _src.playOnAwake = false;
        }

        private void Update() => Tick(Time.deltaTime);

        public void Tick(float dt)
        {
            NearestGapDistance = -1f;
            if (_player.IsDead || !_player.IsGrounded) { _timer = 0f; return; }
            foreach (var d in probeDistances)
            {
                var origin = transform.position + Vector3.right * (_player.Facing * d) + Vector3.up * 0.5f;
                if (!Physics.Raycast(origin, Vector3.down, dropDepth + 0.5f)) { NearestGapDistance = d; break; }
            }
            if (NearestGapDistance < 0f) { _timer = 0f; return; }

            float closeness = 1f - Mathf.InverseLerp(probeDistances[0], probeDistances[probeDistances.Length - 1], NearestGapDistance);
            _timer += dt;
            if (_timer >= Mathf.Lerp(slowInterval, fastInterval, closeness))
            {
                _timer = 0f; TicksPlayed++;
                _src.pitch = Mathf.Lerp(0.8f, 1.6f, closeness);
                _src.PlayOneShot(SideScrollTones.LedgeTick, 0.6f);
            }
        }
    }
}
