using EchoesOfNeon.Core;
using UnityEngine;

namespace EchoesOfNeon.UI
{
    /// <summary>
    /// Loops one track as the main menu theme while any menu screen is open.
    /// The theme is "City lights at night" by Ondrosik (CC0, archive.org
    /// Ondrosik Free Music Catalog), locked by Orlando on 2026-10-09; it is
    /// assigned by MainMenuUISetup from Assets/Audio/Music/. The track fades
    /// in over its first seconds and out over its last, so the loop "breathes"
    /// by design. To swap the clip, assign a different AudioClip in the
    /// inspector or update MainMenuUISetup; nothing else needs to change.
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

        private float _baseVolume = 0.35f;

        private void Update()
        {
            if (!_fadingOut) { _source.volume = _baseVolume * AudioDucking.Multiplier(); return; }
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
            _baseVolume = musicVolume;
            _source.volume = musicVolume * AudioDucking.Multiplier();
        }
    }
}
