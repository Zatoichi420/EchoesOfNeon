using System;
using UnityEngine;

namespace EchoesOfNeon.Core
{
    [Serializable]
    public class SaveGameData
    {
        public string sceneName;
        public string checkpointId;
        public string savedAtUtc;
    }

    /// <summary>Checkpoint/progress record for the Main Menu's "Continue"
    /// button. Currency (PlayerCurrency) and upgrades (UpgradeManager)
    /// already persist across sessions on their own - what was missing was
    /// a record of WHERE the player was, so "Continue" means something.
    /// Same persistence convention as those two (singleton + [Serializable]
    /// POCO + one PlayerPrefs string key + JsonUtility + a changed event),
    /// kept as its own manager rather than folded into either of them since
    /// "where you are" is a different concern from "what you own."</summary>
    public class GameSaveManager : MonoBehaviour
    {
        public static GameSaveManager Instance { get; private set; }

        private const string PrefsKey = "EchoesOfNeon.GameSave";

        public bool HasSaveData { get; private set; }
        public SaveGameData Data { get; private set; }

        public event Action OnSaveDataChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadState();
        }

        private void LoadState()
        {
            HasSaveData = PlayerPrefs.HasKey(PrefsKey);
            if (!HasSaveData) return;
            Data = new SaveGameData();
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(PrefsKey), Data);
        }

        /// <summary>Records the player's current scene/checkpoint as the
        /// resume point. Call this at checkpoints, not every frame - a
        /// PlayerPrefs.Save() write on every frame would be wasteful and
        /// isn't needed for a checkpoint-granularity save.</summary>
        public void SaveCheckpoint(string sceneName, string checkpointId)
        {
            Data = new SaveGameData
            {
                sceneName = sceneName,
                checkpointId = checkpointId,
                savedAtUtc = DateTime.UtcNow.ToString("o"),
            };
            PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(Data));
            PlayerPrefs.Save();
            HasSaveData = true;
            OnSaveDataChanged?.Invoke();
        }

        /// <summary>For "New Game" - clears any existing checkpoint so a
        /// fresh run doesn't accidentally offer to "Continue" into the
        /// previous one. Does not touch currency/upgrades - those are a
        /// separate, deliberately-not-reset-here decision (not yet made
        /// whether New Game should reset progression too).</summary>
        public void ClearSave()
        {
            PlayerPrefs.DeleteKey(PrefsKey);
            PlayerPrefs.Save();
            Data = null;
            HasSaveData = false;
            OnSaveDataChanged?.Invoke();
        }
    }
}
