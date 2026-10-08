using EchoesOfNeon.Accessibility;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.UI;

namespace EchoesOfNeon.UI
{
    /// <summary>
    /// Drop next to a standard UGUI Toggle. On/off state is announced via
    /// AccessibilityState.Selected + a spoken value ("on"/"off"), never
    /// color alone - this is what actually satisfies the "invisible
    /// selection state" failure mode (~/ACCESSIBILITY_WORKFLOW.md) for
    /// toggles, tabs, and any option chip built on top of Toggle.
    /// </summary>
    [RequireComponent(typeof(Toggle))]
    public class AccessibleToggle : MonoBehaviour
    {
        [Tooltip("What the screen reader announces, e.g. \"Reduce Motion\". Falls back to this object's child Text if left blank.")]
        [SerializeField] private string label;

        [Tooltip("Optional: another accessible element this toggle is nested under (e.g. a settings panel).")]
        [SerializeField] private AccessibleButton parentElement;

        private Toggle _toggle;
        [Tooltip("Optional extra spoken after the control, e.g. \"Press Enter to rebind\".")]
        [SerializeField] private string hint;

        private AccessibilityNode _node;

        public AccessibilityNode Node => _node;

        private void Awake()
        {
            _toggle = GetComponent<Toggle>();
        }

        private void OnEnable()
        {
            if (AccessibilityManager.Instance == null) return;

            var resolvedLabel = string.IsNullOrEmpty(label) ? ResolveTextLabel() : label;
            _node = AccessibilityManager.Instance.CreateMenuNode(
                resolvedLabel, AccessibilityRole.Toggle,
                (RectTransform)transform, parentElement != null ? parentElement.Node : null);
            if (_node == null) return;
            _node.hint = hint;

            _node.invoked += HandleInvoked;
            ApplyState(_toggle.isOn);
            _toggle.onValueChanged.AddListener(ApplyState);

            AccessibilityManager.Instance.NotifyLayoutChanged(_node);
        }

        private void OnDisable()
        {
            _toggle.onValueChanged.RemoveListener(ApplyState);
            if (_node == null || AccessibilityManager.Instance == null) return;
            _node.invoked -= HandleInvoked;
            AccessibilityManager.Instance.RemoveMenuNode(_node);
            _node = null;
        }

        // Screen-reader activation flips the toggle itself, which fires
        // onValueChanged -> ApplyState -> the node's own state/value update.
        // Never set _node.state directly from here - that would announce a
        // change the underlying Toggle hasn't actually made yet.
        // invoked is a Func<bool> (success/failure), not an Action.
        private bool HandleInvoked()
        {
            _toggle.isOn = !_toggle.isOn;
            return true;
        }

        private void ApplyState(bool isOn)
        {
            if (_node == null) return;
            _node.state = isOn ? AccessibilityState.Selected : AccessibilityState.None;
            _node.value = isOn ? "on" : "off";
        }

        private string ResolveTextLabel() => EchoesOfNeon.UI.AccessibleLabel.Resolve(this);
    }
}
