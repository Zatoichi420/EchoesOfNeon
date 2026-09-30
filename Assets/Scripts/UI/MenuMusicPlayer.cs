using EchoesOfNeon.Core;
using UnityEngine;

namespace EchoesOfNeon.UI
{
    /// <summary>
    /// Loops one ambient bed as menu music while any menu screen is open.
    /// PLACEHOLDER: the project's real theme (built in Ableton, see
    /// echoes-of-neon-ableton-soundtrack notes) isn't exported into Unity
    /// yet - blocked on a Live edition/license export issue. This plays
    /// new_carthage_night_rain.wav instead so the menu isn't silent and the
    /// volume-control wiring is real and testable today. Swap the clip via
    /// the inspector (or update the default below) once the real theme
    /// lands - nothing else about this component needs to change.
    /// </summary>
    public class MenuMusicPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip musicClip;
        [SerializeField] private bool fadeOnStop = true;
        [SerializeField] private float fadeSeconds = 1f;

        private AudioSource _source;
        private float _fadeTimer;
        private bool _fadingOut;

        private void Awake()
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.loop = true;
            _source.playOnAwake = false;
            _source.spatialBlend = 0f; // 2D - menu music isn't positional
        }

        private void OnEnable()
        {
            if (AudioSettingsManager.Instance != null)
                AudioSettingsManager.Instance.OnSettingsChanged += ApplyVolume;
            ApplyVolume(AudioSettingsManager.Instance != null ? AudioSettingsManager.Instance.Data : null);

            if (musicClip == null) return;
            _source.clip = musicClip;
            _source.Play();
        }

        private void OnDisable()
        {
            if (AudioSettingsManager.Instance != null)
                AudioSettingsManager.Instance.OnSettingsChanged -= ApplyVolume;
        }

        private void Update()
        {
            if (!_fadingOut) return;
            _fadeTimer += Time.unscaledDeltaTime;
            var t = fadeSeconds <= 0f ? 1f : Mathf.Clamp01(_fadeTimer / fadeSeconds);
            _source.volume = Mathf.Lerp(_startFadeVolume, 0f, t);
            if (t >= 1f)
            {
                _source.Stop();
                _fadingOut = false;
            }
        }

        private float _startFadeVolume;

        /// <summary>Call when leaving to gameplay so the music doesn't just
        /// cut - a hard stop reads as a bug, a fade reads as intentional.</summary>
        public void StopWithFade()
        {
            if (!fadeOnStop || !_source.isPlaying)
            {
                _source.Stop();
                return;
            }
            _startFadeVolume = _source.volume;
            _fadeTimer = 0f;
            _fadingOut = true;
        }

        private void ApplyVolume(AudioSettingsData data)
        {
            var musicVolume = AudioSettingsManager.Instance != null
                ? AudioSettingsManager.Instance.EffectiveMusicVolume
                : 0.35f;
            _source.volume = musicVolume;
        }
    }
}
