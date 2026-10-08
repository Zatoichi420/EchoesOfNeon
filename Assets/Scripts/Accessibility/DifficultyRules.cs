using UnityEngine;

namespace EchoesOfNeon.Accessibility
{
    /// <summary>Difficulty and timing-assist tables. Two independent dials so
    /// a player can make combat gentler on damage, on reaction time, or both:
    /// difficulty scales damage taken; timing assist lengthens enemy
    /// wind-ups (the audio telegraph before a hit) and the player's fend
    /// window.</summary>
    public static class DifficultyRules
    {
        public static readonly string[] DifficultyNames = { "Story", "Easy", "Normal", "Hard" };
        public static readonly float[] DamageTaken = { 0.4f, 0.7f, 1f, 1.3f };

        public static readonly string[] TimingNames = { "Standard", "Relaxed", "Generous" };
        public static readonly float[] EnemyTelegraph = { 1f, 1.35f, 1.75f };
        public static readonly float[] FendWindow = { 1f, 1.4f, 1.8f };

        public static float DamageTakenMultiplier(AccessibilitySettings s) => At(DamageTaken, s != null ? s.difficulty : 2);
        public static float EnemyTelegraphMultiplier(AccessibilitySettings s) => At(EnemyTelegraph, s != null ? s.timingAssist : 0);
        public static float FendWindowMultiplier(AccessibilitySettings s) => At(FendWindow, s != null ? s.timingAssist : 0);

        // Live values from the manager (1 when none exists, e.g. a bare test scene).
        public static float CurrentDamageTaken => DamageTakenMultiplier(Settings);
        public static float CurrentEnemyTelegraph => EnemyTelegraphMultiplier(Settings);
        public static float CurrentFendWindow => FendWindowMultiplier(Settings);

        private static AccessibilitySettings Settings =>
            AccessibilityManager.Instance != null ? AccessibilityManager.Instance.Settings : null;

        private static float At(float[] table, int index) => table[Mathf.Clamp(index, 0, table.Length - 1)];
    }
}
