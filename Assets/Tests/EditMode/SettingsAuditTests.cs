using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using NUnit.Framework;
using UnityEngine;

namespace EchoesOfNeon.Tests
{
    /// <summary>Guards against settings that appear in the menu but that no
    /// system reads. Every field must be consumed by game code OR listed in
    /// the matching UnimplementedSettings array (which hides its control).</summary>
    public class SettingsAuditTests
    {
        private static string[] SourceFiles(params string[] excludeFileNames) =>
            Directory.GetFiles(Path.Combine(Application.dataPath, "Scripts"), "*.cs", SearchOption.AllDirectories)
                .Where(f => !excludeFileNames.Contains(Path.GetFileName(f)))
                .ToArray();

        // Counts member accesses like "settings.fieldName" outside the menu,
        // ignoring nameof(...) lines (the UnimplementedSettings lists).
        private static bool IsReadOutsideMenu(string pattern)
        {
            var rx = new Regex(pattern);
            return SourceFiles("MenuController.cs").Any(f =>
                File.ReadAllLines(f).Any(line => !line.Contains("nameof(") && rx.IsMatch(line)));
        }

        [Test]
        public void EveryAccessibilitySetting_IsConsumedOrListedUnimplemented()
        {
            var fields = typeof(AccessibilitySettings).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var f in fields)
            {
                bool consumed = IsReadOutsideMenu(@"\." + f.Name + @"\b");
                bool listed = AccessibilityManager.UnimplementedSettings.Contains(f.Name);
                Assert.IsFalse(consumed && listed, $"{f.Name} is now used by game code - remove it from AccessibilityManager.UnimplementedSettings and show its menu control.");
                Assert.IsTrue(consumed || listed, $"{f.Name} is shown in the menu but nothing reads it. Wire it up or add it to AccessibilityManager.UnimplementedSettings.");
            }
        }

        [Test]
        public void EveryAudioVolume_IsConsumedOrListedUnimplemented()
        {
            foreach (var name in new[] { "musicVolume", "sfxVolume", "dialogueVolume" })
            {
                var effective = "Effective" + char.ToUpper(name[0]) + name.Substring(1);
                bool consumed = SourceFiles("MenuController.cs", "AudioSettingsManager.cs").Any(f => File.ReadAllText(f).Contains(effective));
                bool listed = AudioSettingsManager.UnimplementedSettings.Contains(name);
                Assert.IsFalse(consumed && listed, $"{name} is now played through - remove it from AudioSettingsManager.UnimplementedSettings.");
                Assert.IsTrue(consumed || listed, $"{name} slider does nothing. Route audio through {effective} or list it as unimplemented.");
            }
        }

        [Test]
        public void UnimplementedLists_OnlyNameRealFields()
        {
            foreach (var n in AccessibilityManager.UnimplementedSettings)
                Assert.IsNotNull(typeof(AccessibilitySettings).GetField(n), n);
            foreach (var n in AudioSettingsManager.UnimplementedSettings)
                Assert.IsNotNull(typeof(AudioSettingsData).GetField(n), n);
        }
    }
}
