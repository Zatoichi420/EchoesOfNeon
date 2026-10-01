using System;
using EchoesOfNeon.Accessibility;
using UnityEngine;

namespace EchoesOfNeon.SideScroll
{
    /// <summary>Marcus abilities, in unlock order: ability i unlocks at Marcus level i + 2.</summary>
    public enum MarcusAbility { EchoStep, SonarLock, FlashBlind, SonicPulse, ShadowVeil, Resonance, Overclock, ExecutionChain, EchoStorm }

    /// <summary>Cookie's cybernetic augments, in unlock order: augment i unlocks at Cookie level i + 2.</summary>
    public enum CookieAbility { CollarPing, FastWhistle, CyberEye, TitanBite, GuardMode, Rally, PackLink, ShockHowl, TandemStrike }

    [Serializable]
    public class LevelTrack
    {
        public const int MaxLevel = 10;
        public int level = 1;
        public int xp;                                   // xp within the current level
        public int XpToNext => level >= MaxLevel ? 0 : 60 + 40 * (level - 1);
    }

    [Serializable]
    internal class ProgressionSave { public LevelTrack marcus = new LevelTrack(); public LevelTrack cookie = new LevelTrack(); }

    /// <summary>
    /// XP and levels for Marcus and Cookie, which level independently (cap 10). XP comes from defeating enemies,
    /// successful fends, Cookie's bites, and exploring (ExplorationBeacon). Every Marcus level-up unlocks one
    /// augment, grants stat gains, and plays the next lore CLUE (spoken via Announce; replay with Journal).
    /// Abilities are derived from level, so the unlock rules live in one place. Saved with PlayerPrefs JSON, the
    /// same convention as the other progression classes.
    /// </summary>
    public class SideScrollProgression : MonoBehaviour
    {
        public LevelTrack marcus = new LevelTrack();
        public LevelTrack cookie = new LevelTrack();
        public bool persist = true;                      // tests turn this off
        private const string Key = "EchoesOfNeon.SideScrollProgress";

        public event Action<int> OnMarcusLevelUp;
        public event Action<int> OnCookieLevelUp;

        public int MarcusLevel => marcus.level;
        public int CookieLevel => cookie.level;
        public string LastClue { get; private set; } = LoreClues.ForLevel(1);

        public bool Has(MarcusAbility a) => marcus.level >= (int)a + 2;
        public bool Has(CookieAbility a) => cookie.level >= (int)a + 2;

        // ---- stat gains derived from level (plan table); Lv gives +10 max health and +3% damage on top ----
        public float MaxHealthBonus => 10f * (marcus.level - 1) + (marcus.level >= 5 ? 20f : 0f);
        public float DamageMultiplier => 1f + 0.03f * (marcus.level - 1) + (marcus.level >= 6 ? 0.10f : 0f) + (marcus.level >= 9 ? 0.15f : 0f);
        public float MoveSpeedMultiplier => marcus.level >= 2 ? 1.10f : 1f;
        public float FendWindowBonus => marcus.level >= 3 ? 0.1f : 0f;
        public float SonarRadiusMultiplier => marcus.level >= 4 ? 1.15f : 1f;
        public float DamageTakenMultiplier => marcus.level >= 7 ? 0.8f : 1f;
        public float ComboWindowBonus => marcus.level >= 8 ? 0.1f : 0f;
        public float StrainCostMultiplier => marcus.level >= 10 ? 0.7f : 1f;
        public float CookieBiteMultiplier => 1f + 0.05f * (cookie.level - 1);
        public float CookieCooldownReduction => (Has(CookieAbility.FastWhistle) ? 1.5f : 0f);

        public static SideScrollProgression Instance { get; private set; }
        private void Awake() { Instance = this; if (persist) Load(); }
        private void OnDestroy() { if (Instance == this) Instance = null; }

        public bool AwardMarcusXp(int amount, string reason = null)
        {
            if (amount <= 0 || marcus.level >= LevelTrack.MaxLevel) return false;
            marcus.xp += amount; bool up = false;
            while (marcus.level < LevelTrack.MaxLevel && marcus.xp >= marcus.XpToNext)
            {
                marcus.xp -= marcus.XpToNext; marcus.level++; up = true; MarcusLevelUp();
            }
            if (marcus.level >= LevelTrack.MaxLevel) marcus.xp = 0;
            if (persist) Save();
            return up;
        }

        public bool AwardCookieXp(int amount)
        {
            if (amount <= 0 || cookie.level >= LevelTrack.MaxLevel) return false;
            cookie.xp += amount; bool up = false;
            while (cookie.level < LevelTrack.MaxLevel && cookie.xp >= cookie.XpToNext)
            {
                cookie.xp -= cookie.XpToNext; cookie.level++; up = true;
                Say($"Cookie reached level {cookie.level}. New augment: {CookieNames[cookie.level - 2]}.");
                OnCookieLevelUp?.Invoke(cookie.level);
            }
            if (cookie.level >= LevelTrack.MaxLevel) cookie.xp = 0;
            if (persist) Save();
            return up;
        }

        /// <summary>Exploration and kills reward both: Marcus full, Cookie a share.</summary>
        public void AwardShared(int marcusXp, int cookieXp) { AwardMarcusXp(marcusXp); AwardCookieXp(cookieXp); }

        private void MarcusLevelUp()
        {
            LastClue = LoreClues.ForLevel(marcus.level);
            Say($"Level {marcus.level}. {MarcusNames[marcus.level - 2]} unlocked. Clue available: press Journal.");
            OnMarcusLevelUp?.Invoke(marcus.level);
        }

        public static readonly string[] MarcusNames =
        { "Echo Step", "Sonar Lock", "Flash Blind", "Sonic Pulse", "Shadow Veil", "Resonance", "Overclock", "Execution Chain", "Echo Storm" };
        public static readonly string[] CookieNames =
        { "Collar Ping", "Faster Whistle", "Cyber Eye", "Titan Bite", "Guard Mode", "Rally", "Pack Link", "Shock Howl", "Tandem Strike" };

        public string StatusLine() =>
            $"Marcus level {marcus.level}, {(marcus.level >= LevelTrack.MaxLevel ? "maxed" : marcus.xp + " of " + marcus.XpToNext + " experience")}. " +
            $"Cookie level {cookie.level}.";

        public string ReadJournal() { Say(LastClue); return LastClue; }

        private static void Say(string s) { AccessibilityManager.Instance?.Announce(s); Debug.Log("[Progress] " + s); }

        public void Save() { try { PlayerPrefs.SetString(Key, JsonUtility.ToJson(new ProgressionSave { marcus = marcus, cookie = cookie })); PlayerPrefs.Save(); } catch (Exception) { } }
        public void Load()
        {
            if (!PlayerPrefs.HasKey(Key)) return;
            try { var s = JsonUtility.FromJson<ProgressionSave>(PlayerPrefs.GetString(Key)); if (s != null) { marcus = s.marcus; cookie = s.cookie; LastClue = LoreClues.ForLevel(marcus.level); } } catch (Exception) { }
        }
        public void ResetAll() { marcus = new LevelTrack(); cookie = new LevelTrack(); LastClue = LoreClues.ForLevel(1); PlayerPrefs.DeleteKey(Key); }
    }

    /// <summary>
    /// DRAFT lore clues, one per Marcus level (spoken as short narration). Orlando loosened the old "no hints" rule on
    /// 2026-09-30: clues may NAME the vampire bloodline from early on. The Madame being his TWIN is still the ending's
    /// reveal and is deliberately NOT mentioned here. Built only from facts in Docs/story-bible.md (father's death tied to
    /// the bloodline, Max has always known, Aegis-Corvus studied the bloodline, literal resurrection, augment strain =
    /// bloodline waking). Orlando to rewrite in his own voice.
    /// </summary>
    public static class LoreClues
    {
        public static readonly string[] Clues =
        {
            "They call me Echo. I learned the dark by ear. I never asked why the dark never frightened me.",
            "Max stitched up my augments again and didn't meet my eyes. He has known something about me for years.",
            "My father's death was not an accident. Someone made sure of it, and the reason was something in our blood.",
            "The strain again. My hands hum for an hour afterward. The doctors never had a word for it. There are old families who do.",
            "Aegis-Corvus kept files on my family. Not on me. On the bloodline. They were studying us like a weapon.",
            "I do my best work at night. I heal faster than I should. People say it's the augments. It isn't the augments.",
            "Desi was frightened of what she found in those files. A bloodline older than the city. The old word for it is vampire.",
            "I died, once. Truly died, and came back. Max will not say how. I am starting to think the Cross blood will not let its own stay dead.",
            "G watches me the way a man watches something he has been waiting for. If the legends are true, he knows what I am becoming.",
            "The strain is not killing me. It is waking me. I know what I am, and I know whose hands built the cage around it."
        };

        public static string ForLevel(int level) => Clues[Mathf.Clamp(level - 1, 0, Clues.Length - 1)];
    }
}
