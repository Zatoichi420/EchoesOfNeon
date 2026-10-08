using EchoesOfNeon.Accessibility;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EchoesOfNeon.UI
{
    /// <summary>Speaks the newly focused control whenever keyboard or gamepad
    /// navigation moves the EventSystem selection. uGUI selection is separate
    /// from the OS screen reader's cursor, so without this arrow-key
    /// navigation is silent. Added automatically by AccessibilityManager.</summary>
    public class MenuFocusAnnouncer : MonoBehaviour
    {
        private GameObject _last;

        private void Update()
        {
            var es = EventSystem.current;
            var current = es != null ? es.currentSelectedGameObject : null;
            if (current == _last) return;
            _last = current;
            if (current == null) return;

            var description = AccessibleDescription.Describe(current);
            if (string.IsNullOrEmpty(description)) return;

            var a11y = AccessibilityManager.Instance;
            if (a11y == null) return;
            var prefix = a11y.ConsumeFocusPrefix();
            a11y.Announce(string.IsNullOrEmpty(prefix) ? description : prefix + " " + description);
        }
    }
}
