using EchoesOfNeon.Progression;
using UnityEditor;
using UnityEngine;

namespace EchoesOfNeon.EditorTools
{
    /// <summary>Batch-mode-runnable seed data for the upgrade hub, so
    /// UpgradeManager has real assets to test against before Max's workshop
    /// UI exists. Run via Unity -batchmode -executeMethod
    /// EchoesOfNeon.EditorTools.UpgradeSeedData.CreateSeedUpgrades, or Assets
    /// menu. Safe to re-run - overwrites existing assets at the same path
    /// rather than duplicating.</summary>
    public static class UpgradeSeedData
    {
        private const string FolderPath = "Assets/Data/Upgrades";

        [MenuItem("Echoes of Neon/Create Seed Upgrade Data")]
        public static void CreateSeedUpgrades()
        {
            EnsureFolder();

            CreateOrUpdate("augment_speed_1", "Swift Steps I", "Recalibrated leg servos - a modest, permanent boost to Marcus's walking, sprinting, and crouching speed alike.",
                UpgradeCategory.Augment, 100, "", TargetStat.MoveSpeed, ModifierType.Flat, 0.5f);

            CreateOrUpdate("augment_speed_2", "Swift Steps II", "Further servo tuning. Requires Swift Steps I.",
                UpgradeCategory.Augment, 250, "augment_speed_1", TargetStat.MoveSpeed, ModifierType.Flat, 0.5f);

            CreateOrUpdate("augment_sonar_1", "Wide Sonar I", "Boosted emitter output - extends the sonar pulse's reach.",
                UpgradeCategory.Augment, 100, "", TargetStat.SonarRadius, ModifierType.Flat, 5f);

            CreateOrUpdate("augment_sonar_2", "Wide Sonar II", "A focusing lens for the sonar emitter - a further 20% range increase. Requires Wide Sonar I.",
                UpgradeCategory.Augment, 300, "augment_sonar_1", TargetStat.SonarRadius, ModifierType.Percent, 0.2f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[UpgradeSeedData] Seed upgrades created/updated in " + FolderPath);
        }

        private static void EnsureFolder()
        {
            if (AssetDatabase.IsValidFolder(FolderPath)) return;
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            AssetDatabase.CreateFolder("Assets/Data", "Upgrades");
        }

        private static void CreateOrUpdate(string id, string displayName, string description,
            UpgradeCategory category, int cost, string prerequisiteId,
            TargetStat targetStat, ModifierType modifierType, float amount)
        {
            string path = $"{FolderPath}/{id}.asset";
            var def = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(path);
            bool isNew = def == null;
            if (isNew) def = ScriptableObject.CreateInstance<UpgradeDefinition>();

            def.upgradeId = id;
            def.displayName = displayName;
            def.description = description;
            def.category = category;
            def.cost = cost;
            def.prerequisiteUpgradeId = prerequisiteId;
            def.targetStat = targetStat;
            def.modifierType = modifierType;
            def.modifierAmount = amount;

            if (isNew) AssetDatabase.CreateAsset(def, path);
            else EditorUtility.SetDirty(def);
        }
    }
}
