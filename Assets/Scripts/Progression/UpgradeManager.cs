using System;
using System.Collections.Generic;
using EchoesOfNeon.Optics;
using EchoesOfNeon.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoesOfNeon.Progression
{
    [Serializable]
    public class PurchasedUpgradesData
    {
        public List<string> purchasedIds = new List<string>();
    }

    /// <summary>
    /// Central owner of Max's workshop upgrade hub (confirmed 2026-09-16:
    /// a recurring hub, not a one-off - cane, augment, and Cookie upgrades).
    /// Holds the catalog of UpgradeDefinitions, tracks which are purchased,
    /// and applies their modifiers to the live stats on TacticalPlayerController
    /// / OculusSensorySuite. Cane and Cookie targets are accepted as valid
    /// data today but not yet appliable - see TargetStat's own doc comment -
    /// so authoring upgrade content for those categories ahead of the
    /// underlying systems existing doesn't block on this system.
    ///
    /// Persistence follows AccessibilityManager's convention: singleton +
    /// [Serializable] POCO + one PlayerPrefs string key + JsonUtility + a
    /// changed event.
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        private const string PrefsKey = "EchoesOfNeon.PurchasedUpgrades";

        [Tooltip("Every upgrade purchasable at Max's workshop. Order doesn't matter - prerequisites are resolved by upgradeId, not list position.")]
        [SerializeField] private UpgradeDefinition[] catalog;

        [Tooltip("Optional - if left empty, resolved at Start() via FindFirstObjectByType.")]
        [SerializeField] private TacticalPlayerController playerController;

        public PurchasedUpgradesData Data { get; private set; } = new PurchasedUpgradesData();

        /// <summary>Fired after an upgrade is purchased and applied, or after
        /// LoadAndApplyAll() replays saved purchases - either way, the moment
        /// UI/announcements should refresh from.</summary>
        public event Action<UpgradeDefinition> OnUpgradeApplied;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                Instance = null;
            }
        }

        /// <summary>This manager outlives scenes but the player it modifies does
        /// not, so re-resolve the player and replay purchases after every load.</summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            playerController = FindFirstObjectByType<TacticalPlayerController>();
            LoadAndApplyAll();
        }

        private void Start()
        {
            if (playerController == null)
            {
                playerController = FindFirstObjectByType<TacticalPlayerController>();
            }
            LoadAndApplyAll();
        }

        public bool IsPurchased(string upgradeId) => Data.purchasedIds.Contains(upgradeId);

        public bool IsAvailable(UpgradeDefinition def)
        {
            if (def == null || IsPurchased(def.upgradeId)) return false;
            if (string.IsNullOrEmpty(def.prerequisiteUpgradeId)) return true;
            return IsPurchased(def.prerequisiteUpgradeId);
        }

        /// <summary>Spends currency, records the purchase, and applies the
        /// modifier immediately. Returns false (no side effects) if the
        /// upgrade is already owned, its prerequisite isn't met, or the
        /// player can't afford it.</summary>
        public bool TryPurchase(UpgradeDefinition def)
        {
            if (!IsAvailable(def)) return false;
            if (PlayerCurrency.Instance == null || !PlayerCurrency.Instance.Spend(def.cost)) return false;

            Data.purchasedIds.Add(def.upgradeId);
            SaveData();
            Apply(def);
            return true;
        }

        /// <summary>Replays every saved purchase against fresh stats - called
        /// once at Start(), and safe to call again (e.g. after a scene
        /// reload replaces TacticalPlayerController/OculusSensorySuite)
        /// since it clears each stat's modifiers first rather than
        /// double-applying.</summary>
        public void LoadAndApplyAll()
        {
            if (!PlayerPrefs.HasKey(PrefsKey))
            {
                Data = new PurchasedUpgradesData();
            }
            else
            {
                try
                {
                    JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(PrefsKey), Data);
                }
                catch (ArgumentException e)
                {
                    // Corrupt save: keep playing with no upgrades rather than throwing.
                    Debug.LogWarning($"UpgradeManager: saved upgrades unreadable, starting empty. {e.Message}");
                    Data = new PurchasedUpgradesData();
                }
            }

            // Explicit null checks: '?.' skips Unity's destroyed-object check.
            if (playerController != null) playerController.ClearMoveSpeedModifiers();
            if (OculusSensorySuite.Instance != null) OculusSensorySuite.Instance.ClearSonarRadiusModifiers();

            foreach (var id in Data.purchasedIds)
            {
                var def = FindInCatalog(id);
                if (def != null) Apply(def);
            }
        }

        private void SaveData()
        {
            PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(Data));
            PlayerPrefs.Save();
        }

        private UpgradeDefinition FindInCatalog(string upgradeId)
        {
            if (catalog == null) return null;
            foreach (var def in catalog)
            {
                if (def != null && def.upgradeId == upgradeId) return def;
            }
            return null;
        }

        private void Apply(UpgradeDefinition def)
        {
            bool applied = def.targetStat switch
            {
                TargetStat.MoveSpeed when playerController != null =>
                    playerController.AddMoveSpeedModifier(def.modifierType, def.modifierAmount),
                TargetStat.SonarRadius when OculusSensorySuite.Instance != null =>
                    OculusSensorySuite.Instance.AddSonarRadiusModifier(def.modifierType, def.modifierAmount),
                _ => false
            };

            if (!applied)
            {
                Debug.LogWarning($"[UpgradeManager] '{def.upgradeId}' targets {def.targetStat}, which has no live stat yet (system not built) - recorded as purchased, no gameplay effect until it exists.");
            }

            OnUpgradeApplied?.Invoke(def);
        }
    }
}
