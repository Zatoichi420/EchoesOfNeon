using System;
using EchoesOfNeon.Accessibility;
using UnityEngine;
using UnityEngine.Accessibility;

namespace EchoesOfNeon.UI
{
    /// <summary>Marks a menu panel as a titled container for the screen
    /// reader and wires VoiceOver's dismiss gesture (two-finger scrub) to a
    /// Back action. Use AnnounceTitle() when the player opens the panel.</summary>
    public class AccessiblePanel : MonoBehaviour
    {
        public string Title = "";
        public Action OnDismiss;

        public AccessibilityNode Node { get; private set; }

        private void OnEnable()
        {
            var a11y = AccessibilityManager.Instance;
            if (a11y == null) return;
            Node = a11y.CreateMenuNode(Title, AccessibilityRole.Container, transform as RectTransform);
            if (Node == null) return;
            Node.dismissed += Dismiss;
        }

        private void OnDisable()
        {
            if (Node == null || AccessibilityManager.Instance == null) { Node = null; return; }
            Node.dismissed -= Dismiss;
            AccessibilityManager.Instance.RemoveMenuNode(Node);
            Node = null;
        }

        public void AnnounceTitle() => AccessibilityManager.Instance?.SetFocusPrefix(Title + ".");

        public bool Dismiss()
        {
            if (OnDismiss == null) return false;
            OnDismiss();
            return true;
        }
    }

    /// <summary>Escape / gamepad B (the UI Cancel action) reaches the selected
    /// control and bubbles up to the nearest parent with this component.</summary>
    public class CancelRelay : MonoBehaviour, UnityEngine.EventSystems.ICancelHandler
    {
        public Action OnCancelRequested;
        public void OnCancel(UnityEngine.EventSystems.BaseEventData eventData) => OnCancelRequested?.Invoke();
    }
}
