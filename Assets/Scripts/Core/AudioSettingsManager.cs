using System;
using UnityEngine;

namespace EchoesOfNeon.Core
{
    [Serializable]
    public class AudioSettingsData
    {
        public float masterVolume = 1f;
        public float musicVolume = 0.35f; // menu music defaults to "not too loud", per Orlando's explicit request
        public float sfxVolume = 1f;
        public float dialogueVolume = 1f;
    }

    /// <summary>No AudioMixer exists in the project yet, and building one
    /// isn't needed just to satisfy four volume sliders - this is a plain
    /// settings blob (same convention as AccessibilityManager/PlayerCurrency:
    /// singleton + [Serializable] POCO + PlayerPrefs + JsonUtility + a
    /// changed event) that anything playing audio reads from directly
    /// (see MenuMusicPlayer). If per-category audio ever needs real mixing
    /// (ducking, snapshots), that's the point to introduce an AudioMixer -
    /// not before there's a second consumer that actually needs it.</summary>
    public class AudioSettingsManager : MonoBehaviour
    {
        public static AudioSettingsManager Instance { get; private set; }

        private const string PrefsKey = "EchoesOfNeon.AudioSettings";

        public AudioSettingsData Data { get; private set; } = new AudioSettingsData();

        public event Action<AudioSettingsData> OnSettingsChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }

        public void LoadSettings()
        {
            if (!PlayerPrefs.HasKey(PrefsKey)) return;
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(PrefsKey), Data);
        }

        public void ApplySettings(AudioSettingsData newSettings)
        {
            Data = newSettings ?? new AudioSettingsData();
            PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(Data));
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke(Data);
        }

        public float EffectiveMusicVolume => Data.masterVolume * Data.musicVolume;
        public float EffectiveSfxVolume => Data.masterVolume * Data.sfxVolume;
        public float EffectiveDialogueVolume => Data.masterVolume * Data.dialogueVolume;
    }
}
