using UnityEngine;

namespace EchoesOfNeon.Player
{
    /// <summary>
    /// How the player follows Cookie by ear. While she moves: her 3D-positioned pawsteps at a cadence that speeds up
    /// with her speed, plus a collar jingle every ~second. While she waits or has arrived: no footsteps, but a
    /// quieter collar rustle every couple of seconds so her position stays findable when she is standing still
    /// (the player must be able to locate her without sight). Purely audio; movement is CompanionGuide. Placeholder
    /// clips: procedural pawsteps and collar (see Docs/Story/Audio/generation-scripts/).
    /// </summary>
    [RequireComponent(typeof(CompanionGuide))]
    public class CompanionAudio : MonoBehaviour
    {
        public AudioClip[] pawsteps;
        public AudioClip[] collar;
        public float stepVolume = 0.7f;
        public float collarVolume = 0.4f;
        public float restCollarVolume = 0.22f;
        public float collarInterval = 1.0f;
        public float restCollarInterval = 2.5f;
        public float minStepInterval = 0.26f, maxStepInterval = 0.5f;

        public int StepsPlayed { get; private set; }
        public int CollarPlays { get; private set; }
        public int RestRustles { get; private set; }

        private CompanionGuide _guide;
        private AudioSource _steps, _collar;
        private float _stepTimer, _collarTimer;

        private void Awake()
        {
            _guide = GetComponent<CompanionGuide>();
            _steps = MakeSource("PawstepSource"); _collar = MakeSource("CollarSource");
        }

        private AudioSource MakeSource(string name)
        {
            var go = new GameObject(name); go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            var s = go.AddComponent<AudioSource>();
            s.playOnAwake = false; s.spatialBlend = 1f;           // fully 3D: direction and distance are the whole point
            s.rolloffMode = AudioRolloffMode.Logarithmic; s.minDistance = 1.5f; s.maxDistance = 30f;
            EchoesOfNeon.Core.AudioRouting.Bind(s, EchoesOfNeon.Core.AudioCategory.Sfx);
            return s;
        }

        private void Update() => Tick(Time.deltaTime);

        public void Tick(float dt)
        {
            bool moving = _guide.CurrentSpeed > 0.1f;
            if (moving)
            {
                _stepTimer += dt; _collarTimer += dt;
                float interval = Mathf.Lerp(maxStepInterval, minStepInterval, Mathf.Clamp01(_guide.CurrentSpeed / 4.5f));
                if (_stepTimer >= interval) { _stepTimer = 0f; Play(_steps, pawsteps, stepVolume); StepsPlayed++; }
                if (_collarTimer >= collarInterval) { _collarTimer = 0f; Play(_collar, collar, collarVolume); CollarPlays++; }
            }
            else
            {
                _stepTimer = 0f; _collarTimer += dt;
                if (_collarTimer >= restCollarInterval) { _collarTimer = 0f; Play(_collar, collar, restCollarVolume); RestRustles++; }
            }
        }

        private static void Play(AudioSource src, AudioClip[] clips, float volume)
        {
            if (src == null || clips == null || clips.Length == 0) return;
            src.pitch = Random.Range(0.95f, 1.05f);
            src.PlayOneShot(clips[Random.Range(0, clips.Length)], volume);
        }
    }
}
