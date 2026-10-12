using EchoesOfNeon.Accessibility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoesOfNeon.SideScroll
{
    public enum RewardKind { None, MarcusPack, CookieTreat }

    /// <summary>Full-health pack for Marcus. Found by ear like every locator; refuses (and says so) at full health so it is never wasted.</summary>
    public class FullHealthPack : LocatorObject
    {
        protected override float BeepPitch => 1.3f;
        protected override void OnReached()
        {
            if (player.Health >= player.MaxHealth - 0.01f)
            { if (!_told) { _told = true; AccessibilityManager.Instance?.Announce("Health is full. Pack kept."); } return; }
            player.Heal(player.MaxHealth - player.Health, "Health pack");
            AccessibilityManager.Instance?.Announce("Full health");
            Consumed = true; Destroy(gameObject, 0.05f);
        }
        private bool _told;
    }

    /// <summary>Treat for Cookie. Marcus picks it up and feeds her: full health, and it lifts her if she is down.</summary>
    public class CookieTreat : LocatorObject
    {
        protected override float BeepPitch => 1.8f;
        protected override void OnReached()
        {
            var ck = player.cookie;
            if (ck == null || (!ck.IsDown && ck.Health >= ck.maxHealth - 0.01f))
            { if (!_told) { _told = true; AccessibilityManager.Instance?.Announce("Cookie is fine. Treat kept."); } return; }
            ck.HealFull();
            AccessibilityManager.Instance?.Announce("Cookie is back to full health");
            Consumed = true; Destroy(gameObject, 0.05f);
        }
        private bool _told;
    }

    /// <summary>
    /// Random health drops after kills, for both Marcus and Cookie. Odds rise when either is hurt, and a drop is
    /// guaranteed after a dry streak so a bad run of luck can never leave the pair stranded. The decision is a pure
    /// function (Decide) so it can be tested without a scene. Auto-creates itself in any scene with a SideScrollPlayer.
    /// </summary>
    public class CombatRewardSpawner : MonoBehaviour
    {
        public float baseChance = 0.30f, hurtBonus = 0.30f, hurtBelow = 0.5f;
        public int guaranteeAfterKills = 4;

        private SideScrollPlayer _player;
        private int _killsSinceDrop;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot() { SceneManager.sceneLoaded += (s, m) => Ensure(); Ensure(); }

        private static void Ensure()
        {
            if (FindAnyObjectByType<SideScrollPlayer>() == null || FindAnyObjectByType<CombatRewardSpawner>() != null) return;
            new GameObject("CombatRewardSpawner").AddComponent<CombatRewardSpawner>();
        }

        private void OnEnable() { SideScrollEnemy.Killed += OnKilled; }
        private void OnDisable() { SideScrollEnemy.Killed -= OnKilled; }

        /// <summary>roll and pick are uniform 0..1. Picks the pack for whoever needs it more (ties split on pick).</summary>
        public static RewardKind Decide(float roll, float pick, float marcusFrac, float cookieFrac, int killsSinceDrop,
                                        float baseChance = 0.30f, float hurtBonus = 0.30f, float hurtBelow = 0.5f, int guarantee = 4)
        {
            bool hurt = marcusFrac < hurtBelow || cookieFrac < hurtBelow;
            float chance = baseChance + (hurt ? hurtBonus : 0f);
            if (killsSinceDrop + 1 < guarantee && roll >= chance) return RewardKind.None;
            if (marcusFrac < cookieFrac - 0.1f) return RewardKind.MarcusPack;
            if (cookieFrac < marcusFrac - 0.1f) return RewardKind.CookieTreat;
            return pick < 0.5f ? RewardKind.MarcusPack : RewardKind.CookieTreat;
        }

        private void OnKilled(SideScrollEnemy e)
        {
            if (e == null) return;
            if (_player == null) _player = FindAnyObjectByType<SideScrollPlayer>();
            if (_player == null) return;
            var ck = _player.cookie;
            float mf = _player.MaxHealth > 0f ? _player.Health / _player.MaxHealth : 1f;
            float cf = ck == null ? 1f : (ck.IsDown ? 0f : ck.Health / ck.maxHealth);
            var kind = Decide(Random.value, Random.value, mf, cf, _killsSinceDrop, baseChance, hurtBonus, hurtBelow, guaranteeAfterKills);
            if (kind == RewardKind.None || (kind == RewardKind.CookieTreat && ck == null)) { _killsSinceDrop++; return; }
            _killsSinceDrop = 0;

            var pos = e.transform.position;
            if (Physics.Raycast(pos + Vector3.up, Vector3.down, out var hit, 6f)) pos.y = hit.point.y + 0.4f;
            pos.z = 0f;
            var go = new GameObject(kind == RewardKind.MarcusPack ? "HealthPack" : "CookieTreat") { transform = { position = pos } };
            LocatorObject pickup = kind == RewardKind.MarcusPack ? go.AddComponent<FullHealthPack>() : (LocatorObject)go.AddComponent<CookieTreat>();
            pickup.player = _player;

            string side = pos.x >= _player.transform.position.x ? "to your right" : "to your left";
            AccessibilityManager.Instance?.Announce($"{(kind == RewardKind.MarcusPack ? "Health pack" : "Treat for Cookie")} dropped {side}.", SpeechPriority.Low);
        }
    }
}
