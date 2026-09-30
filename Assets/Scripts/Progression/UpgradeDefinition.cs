using UnityEngine;

namespace EchoesOfNeon.Progression
{
    /// <summary>Which part of Marcus's (or Cookie's) kit an upgrade affects.
    /// Cane and Cookie entries exist so upgrade data can be authored ahead of
    /// those systems being built (no mobility-cane weapon or Cookie
    /// controller exists in code yet as of 2026-09-16) - UpgradeManager logs
    /// a warning and skips applying them rather than crashing until the
    /// corresponding system exists to receive the modifier.</summary>
    public enum UpgradeCategory
    {
        Augment,
        Cane,
        Cookie
    }

    /// <summary>The specific stat an upgrade modifies. Deliberately does NOT
    /// include Neural Dilate's timescale - that value lives in
    /// AccessibilityManager.Settings as a persisted accessibility preference,
    /// a different concern from purchased progression, and mixing the two
    /// persistence systems was judged not worth the coupling for this pass.</summary>
    public enum TargetStat
    {
        MoveSpeed,
        SonarRadius,
        CaneReserved,
        CookieReserved
    }

    public enum ModifierType
    {
        Flat,
        Percent
    }

    /// <summary>Data-only description of one purchasable upgrade at Max's
    /// workshop. Create via Assets > Create > Echoes of Neon > Upgrade
    /// Definition. Tiering is a simple linked list via prerequisiteUpgradeId
    /// (empty = no prerequisite, available from the start) rather than a
    /// numeric tier field, so tiers don't have to be contiguous or exclusive
    /// to one stat.</summary>
    [CreateAssetMenu(fileName = "NewUpgrade", menuName = "Echoes of Neon/Upgrade Definition")]
    public class UpgradeDefinition : ScriptableObject
    {
        [Tooltip("Stable identifier used for save data - do not change once players may have purchased this upgrade.")]
        public string upgradeId;

        public string displayName;

        [TextArea]
        public string description;

        public UpgradeCategory category;

        [Min(0)]
        public int cost;

        [Tooltip("upgradeId of a prerequisite upgrade, or empty for none.")]
        public string prerequisiteUpgradeId;

        public TargetStat targetStat;
        public ModifierType modifierType;

        [Tooltip("Flat: added to the base value. Percent: 0.1 = +10%.")]
        public float modifierAmount;
    }
}
