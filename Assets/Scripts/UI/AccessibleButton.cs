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
        [Tooltip("Optional extra spoken after the control, e.g. \"Press Enter to rebind\".")]
        [SerializeField] private string hint;

        private AccessibilityNode _node;

        public AccessibilityNode Node => _node;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (AccessibilityManager.Instance == null)
            {
                // Manager not created yet: register when it announces itself.
                AccessibilityManager.Ready += Register;
                return;
            }
            Register();
        }

        private void Register()
        {
            AccessibilityManager.Ready -= Register;
            if (_node != null || AccessibilityManager.Instance == null || !isActiveAndEnabled) return;

            var resolvedLabel = string.IsNullOrEmpty(label) ? ResolveTextLabel() : label;
            _node = AccessibilityManager.Instance.CreateMenuNode(
                resolvedLabel, AccessibilityRole.Button,
                (RectTransform)transform, parentElement != null ? parentElement.Node : null);
            if (_node == null) return;
            _node.hint = hint;

            _node.invoked += HandleInvoked;
            if (_roleOverride.HasValue) _node.role = _roleOverride.Value;
            ApplyState();

            // A freshly-opened panel needs the OS told something changed -
            // otherwise the screen reader keeps announcing whatever was
            // focused before this button existed.
            AccessibilityManager.Instance.NotifyLayoutChanged(_node);
        }

        private void OnDisable()
        {
            AccessibilityManager.Ready -= Register;
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
        /// <summary>Marks this button as the active one in a group (e.g. the
        /// current options tab) so the screen reader says "selected" -
        /// otherwise the active tab is only visible, never audible.</summary>
        public void SetSelected(bool selected)
        {
            _selected = selected;
            ApplyState();
        }

        /// <summary>Changes the spoken hint at runtime (e.g. a keybind row
        /// saying which key it is currently bound to).</summary>
        public void SetHint(string newHint)
        {
            hint = newHint;
            if (_node != null) _node.hint = newHint;
        }

        /// <summary>Overrides the announced role (e.g. TabButton).</summary>
        public void SetRole(AccessibilityRole role)
        {
            _roleOverride = role;
            if (_node != null) _node.role = role;
        }

        private bool _selected;
        private AccessibilityRole? _roleOverride;

        private void ApplyState()
        {
            if (_node == null) return;
            var state = AccessibilityState.None;
            if (!_button.interactable) state |= AccessibilityState.Disabled;
            if (_selected) state |= AccessibilityState.Selected;
            _node.state = state;
        }

        public void SetInteractable(bool interactable)
        {
            _button.interactable = interactable;
            ApplyState();
        }

        private bool HandleInvoked()
        {
            _button.onClick.Invoke();
            return true;
        }

        private string ResolveTextLabel() => EchoesOfNeon.UI.AccessibleLabel.Resolve(this);
    }
}
