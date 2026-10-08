using EchoesOfNeon.Accessibility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoesOfNeon.UI
{
    /// <summary>Speaks a short welcome the first time the player reaches the
    /// main menu, and the first time they reach a gameplay scene: where the
    /// accessibility options are, and which keys give help. Each message is
    /// said once ever (PlayerPrefs); F1 re-reads the controls any time.</summary>
    public class FirstRunPrompt : MonoBehaviour
    {
        public const string MenuKey = "EchoesOfNeon.FirstRun.Menu";
        public const string GameplayKey = "EchoesOfNeon.FirstRun.Gameplay";

        public const string MenuMessage =
            "Welcome to Echoes of Neon. Use the arrow keys and Enter to move through the menu. " +
            "Open Options, then Accessibility, to set captions, mono audio, size and difficulty.";
        public const string GameplayMessage =
            "Press F1 at any time to hear your controls. V repeats the last message, H reads your status, and Escape pauses.";

        private const float DelaySeconds = 1.5f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Spawn(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Spawn(scene);

        private static void Spawn(Scene scene)
        {
            bool menu = scene.name == PauseMenu.MainMenuSceneName;
            if (Seen(menu ? MenuKey : GameplayKey)) return;
            if (FindFirstObjectByType<FirstRunPrompt>() != null) return;
            var p = new GameObject("FirstRunPrompt").AddComponent<FirstRunPrompt>();
            p._key = menu ? MenuKey : GameplayKey;
            p._message = menu ? MenuMessage : GameplayMessage;
        }

        private string _key, _message;
        private float _elapsed;

        private void Update()
        {
            _elapsed += Time.unscaledDeltaTime;
            if (_elapsed < DelaySeconds || AccessibilityManager.Instance == null) return;
            AccessibilityManager.Instance.Announce(_message);
            MarkSeen(_key);
            Destroy(gameObject);
        }

        public static bool Seen(string key)
        {
            try { return PlayerPrefs.GetInt(key, 0) == 1; } catch { return true; }
        }

        public static void MarkSeen(string key)
        {
            try { PlayerPrefs.SetInt(key, 1); PlayerPrefs.Save(); } catch { /* prefs unavailable: just don't remember */ }
        }
    }
}
