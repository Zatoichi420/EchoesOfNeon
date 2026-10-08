using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using UnityEngine;

namespace EchoesOfNeon.Tests
{
    /// <summary>Scenes loaded by tests (MainMenu) create DontDestroyOnLoad
    /// managers that would otherwise leak into every later test.</summary>
    public static class TestSingletons
    {
        public static void DestroyAll()
        {
            foreach (var m in Object.FindObjectsByType<AccessibilityManager>(FindObjectsSortMode.None)) Object.DestroyImmediate(m.gameObject);
            foreach (var m in Object.FindObjectsByType<AudioSettingsManager>(FindObjectsSortMode.None)) Object.DestroyImmediate(m.gameObject);
            foreach (var m in Object.FindObjectsByType<InputManager>(FindObjectsSortMode.None)) Object.DestroyImmediate(m.gameObject);
            foreach (var m in Object.FindObjectsByType<GameSaveManager>(FindObjectsSortMode.None)) Object.DestroyImmediate(m.gameObject);
        }
    }
}
