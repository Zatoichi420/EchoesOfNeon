using System.Collections;
using EchoesOfNeon.Accessibility;
using UnityEngine;

namespace EchoesOfNeon.Core
{
    /// <summary>
    /// Temporary verification aid for the Phase 1-3 test scene - confirms
    /// the native screen-reader hookup actually reaches NVDA independent of
    /// whether input is working. Safe to delete once real menu UI exists
    /// and exercises Announce() for real reasons.
    /// </summary>
    public class TestAnnouncer : MonoBehaviour
    {
        private IEnumerator Start()
        {
            // One-frame delay so AccessibilityManager.Awake() (which sets
            // Instance and registers the AccessibilityHierarchy) has
            // definitely run first, regardless of GameObject order.
            yield return null;
            AccessibilityManager.Instance?.Announce(
                "Echoes of Neon test scene loaded. Use W A S D or the left stick to move, mouse or right stick to look.");
        }
    }
}
