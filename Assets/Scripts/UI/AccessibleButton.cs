using EchoesOfNeon.Accessibility;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.UI;

namespace EchoesOfNeon.UI
{
    /// <summary>
    /// Drop next to a standard UGUI Button to expose it to the native
    /// screen-reader hookup (see AccessibilityManager.CreateMenuNode).
    /// Registers an AccessibilityNode on enable and removes it on disable,
    /// so opening/closing menu panels keeps the AccessibilityHierarchy in
    /// sync rather than leaking stale nodes - Unity does not do this for
    /// you. Screen-reader activation (double-tap/Enter) fires the button's
    /// own onClick, never a separate action path.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class AccessibleButton : MonoBehaviour
    {
        [Tooltip("What the screen reader announces, e.g. \"Start Game\". Falls back to this object's child Text if left blank.")]
        [SerializeField] private string label;

        [Tooltip("Optional: another accessible element this button is nested under (e.g. a tab or panel), so the hierarchy reflects real menu structure.")]
        [SerializeField] private AccessibleButton parentElement;

        private Button _button;
        private AccessibilityNode _node;

        public AccessibilityNode Node => _node;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (AccessibilityManager.Instance == null) return;

            var resolvedLabel = string.IsNullOrEmpty(label) ? ResolveTextLabel() : label;
            _node = AccessibilityManager.Instance.CreateMenuNode(
                resolvedLabel, AccessibilityRole.Button,
                (RectTransform)transform, parentElement != null ? parentElement.Node : null);
            if (_node == null) return;

            _node.invoked += HandleInvoked;
            _node.state = _button.interactable ? AccessibilityState.None : AccessibilityState.Disabled;

            // A freshly-opened panel needs the OS told something changed -
            // otherwise the screen reader keeps announcing whatever was
            // focused before this button existed.
            AccessibilityManager.Instance.NotifyLayoutChanged(_node);
        }

        private void OnDisable()
        {
            if (_node == null || AccessibilityManager.Instance == null) return;
            _node.invoked -= HandleInvoked;
            AccessibilityManager.Instance.RemoveMenuNode(_node);
            _node = null;
        }

        // invoked is a Func<bool> (success/failure), not an Action - a
        // button press essentially can't fail here, so this always reports
        // success back to the screen reader.
        /// <summary>Sets both the underlying Button's interactable state
        /// AND the screen-reader node's Disabled state together - setting
        /// Button.interactable alone would silently desync the two, which
        /// is exactly the kind of gap the project's "invisible selection
        /// state" failure mode exists to catch, applied to enabled/disabled
        /// rather than selected/unselected.</summary>
        public void SetInteractable(bool interactable)
        {
            _button.interactable = interactable;
            if (_node != null)
                _node.state = interactable ? AccessibilityState.None : AccessibilityState.Disabled;
        }

        private bool HandleInvoked()
        {
            _button.onClick.Invoke();
            return true;
        }

        private string ResolveTextLabel()
        {
            var text = GetComponentInChildren<Text>();
            return text != null ? text.text : gameObject.name;
        }
    }
}
