using System;
using UnityEngine;

namespace EchoesOfNeon.Progression
{
    [Serializable]
    public class CurrencyData
    {
        public int balance;
    }

    /// <summary>Marcus's spendable balance for Max's workshop upgrades.
    /// Persistence follows AccessibilityManager's own convention exactly
    /// (singleton + [Serializable] POCO + one PlayerPrefs string key +
    /// JsonUtility.FromJsonOverwrite/ToJson + PlayerPrefs.Save + a changed
    /// event) so the project has one consistent save pattern rather than
    /// two. What currency actually IS (mission rewards? scavenged loot?) and
    /// how it's earned is not yet decided - out of scope for this pass,
    /// which only builds the spend/hold mechanism.</summary>
    public class PlayerCurrency : MonoBehaviour
    {
        public static PlayerCurrency Instance { get; private set; }

        private const string PrefsKey = "EchoesOfNeon.PlayerCurrency";

        public CurrencyData Data { get; private set; } = new CurrencyData();

        public event Action<int> OnBalanceChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
        }

        public void LoadData()
        {
            if (!PlayerPrefs.HasKey(PrefsKey)) return;
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(PrefsKey), Data);
        }

        private void SaveData()
        {
            PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(Data));
            PlayerPrefs.Save();
        }

        public bool CanAfford(int cost) => Data.balance >= cost;

        public bool Spend(int amount)
        {
            if (amount < 0 || !CanAfford(amount)) return false;
            Data.balance -= amount;
            SaveData();
            OnBalanceChanged?.Invoke(Data.balance);
            return true;
        }

        public void Add(int amount)
        {
            if (amount < 0) return;
            Data.balance += amount;
            SaveData();
            OnBalanceChanged?.Invoke(Data.balance);
        }
    }
}
