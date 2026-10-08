using UnityEngine;

namespace EchoesOfNeon.Core
{
    public enum AudioCategory { Sfx, Dialogue }

    /// <summary>Makes an AudioSource honor the player's SFX or Dialogue
    /// volume (times master) without an AudioMixer. PlayOneShot's volume
    /// argument multiplies AudioSource.volume, so scaling the source scales
    /// every clip played through it. Call Bind() right after creating a
    /// source; later changes the code makes to source.volume are respected
    /// (the new value becomes the base).</summary>
    public static class AudioRouting
    {
        public static AudioSource Bind(AudioSource source, AudioCategory category)
        {
            if (source == null) return null;
            var binder = source.GetComponent<CategoryVolume>();
            if (binder == null || binder.Source != source)
                binder = source.gameObject.AddComponent<CategoryVolume>();
            binder.Setup(source, category);
            return source;
        }

        /// <summary>Multiplier for a category right now (1 when no settings manager exists, e.g. a test scene started directly).</summary>
        public static float Multiplier(AudioCategory category)
        {
            var m = AudioSettingsManager.Instance;
            if (m == null) return 1f;
            return category == AudioCategory.Dialogue ? m.EffectiveDialogueVolume : m.EffectiveSfxVolume;
        }

        public class CategoryVolume : MonoBehaviour
        {
            public AudioSource Source { get; private set; }
            private AudioCategory _category;
            private float _base = 1f;
            private float _applied = -1f;

            public void Setup(AudioSource source, AudioCategory category)
            {
                Source = source;
                _category = category;
                _base = source.volume;
                _applied = -1f;
                Apply();
            }

            private void OnEnable()
            {
                if (AudioSettingsManager.Instance != null)
                    AudioSettingsManager.Instance.OnSettingsChanged += HandleChanged;
            }

            private void OnDisable()
            {
                if (AudioSettingsManager.Instance != null)
                    AudioSettingsManager.Instance.OnSettingsChanged -= HandleChanged;
            }

            private void HandleChanged(AudioSettingsData _) => Apply();

            public void Apply()
            {
                if (Source == null) return;
                // Someone set a new base volume since we last wrote one.
                if (_applied >= 0f && !Mathf.Approximately(Source.volume, _applied)) _base = Source.volume;
                _applied = _base * Multiplier(_category);
                Source.volume = _applied;
            }

            // Sources can be given a new base after Bind() (RestPoint does);
            // re-sync once the first frame has run.
            private void Start() => Apply();
        }
    }
}
