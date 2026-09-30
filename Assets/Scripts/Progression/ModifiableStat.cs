using System;
using System.Collections.Generic;

namespace EchoesOfNeon.Progression
{
    /// <summary>
    /// A numeric value that can be permanently raised by purchased upgrades
    /// without the owning component needing to know upgrades exist. Pattern
    /// borrowed (idea only, no code) from evaluating delmarle/RPG-Core's
    /// stats system 2026-09-16: base value + stacked modifiers, recomputed
    /// on change, rather than upgrades mutating the base field directly.
    ///
    /// Deliberately NOT persisted itself - UpgradeManager rebuilds each
    /// stat's modifiers at runtime by replaying the player's purchased
    /// UpgradeDefinitions against the fresh BaseValue every time a scene
    /// loads, the same way AccessibilityManager treats its own settings as
    /// the single source of truth rather than caching a derived value.
    /// </summary>
    [Serializable]
    public class ModifiableStat
    {
        [UnityEngine.SerializeField] private float baseValue;

        private readonly List<float> _flatModifiers = new List<float>();
        private readonly List<float> _percentModifiers = new List<float>();

        public event Action OnValueChanged;

        public ModifiableStat(float baseValue)
        {
            this.baseValue = baseValue;
        }

        public float BaseValue => baseValue;

        /// <summary>(base + sum of flat modifiers) * (1 + sum of percent modifiers).</summary>
        public float Value
        {
            get
            {
                float flatSum = 0f;
                for (int i = 0; i < _flatModifiers.Count; i++) flatSum += _flatModifiers[i];

                float percentSum = 0f;
                for (int i = 0; i < _percentModifiers.Count; i++) percentSum += _percentModifiers[i];

                return (baseValue + flatSum) * (1f + percentSum);
            }
        }

        public void AddFlatModifier(float amount)
        {
            _flatModifiers.Add(amount);
            OnValueChanged?.Invoke();
        }

        public void AddPercentModifier(float amount)
        {
            _percentModifiers.Add(amount);
            OnValueChanged?.Invoke();
        }

        /// <summary>Clears every applied modifier so they can be replayed
        /// from scratch (UpgradeManager does this on load, rather than
        /// risk double-applying if it's ever re-initialized mid-session).</summary>
        public void ClearModifiers()
        {
            _flatModifiers.Clear();
            _percentModifiers.Clear();
            OnValueChanged?.Invoke();
        }
    }
}
