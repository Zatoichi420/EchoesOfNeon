using EchoesOfNeon.Accessibility;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.UI;

namespace EchoesOfNeon.UI
{
    /// <summary>
    /// Drop next to a standard UGUI Slider. Screen-reader increment/
    /// decrement (arrow keys/D-pad while focused) drives the same Slider
    /// value a pointer drag would, and the announced value updates
    /// immediately after every change - same "state must be spoken, not
    /// just shown" rule as AccessibleToggle, applied to a continuous or
    /// stepped control (e.g. difficulty, look sensitivity) instead of a
    /// binary one.
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class AccessibleSlider : MonoBehaviour
    {
        [Tooltip("What the screen reader announces, e.g. \"Difficulty\". Falls back to this object's child Text if left blank.")]
        [SerializeField] private string label;

        [Tooltip("Optional: another accessible element this slider is nested under (e.g. a settings panel).")]
        [SerializeField] private AccessibleButton parentElement;

        [Tooltip("Optional named steps, e.g. [\"Easy\", \"Normal\", \"Hard\"] for a 0-2 whole-number slider, so the screen reader speaks the name instead of a raw number. Leave empty to announce the raw value.")]
        [SerializeField] private string[] valueLabels;

        private Slider _slider;
        [Tooltip("Optional extra spoken after the control, e.g. \"Press Enter to rebind\".")]
        [SerializeField] private string hint;

        private AccessibilityNode _node;

        public AccessibilityNode Node => _node;

        private void Awake()
        {
            _slider = GetComponent<Slider>();
        }

        private void OnEnable()
        {
            if (AccessibilityManager.Instance == null) return;

            var resolvedLabel = string.IsNullOrEmpty(label) ? ResolveTextLabel() : label;
            _node = AccessibilityManager.Instance.CreateMenuNode(
                resolvedLabel, AccessibilityRole.Slider,
                (RectTransform)transform, parentElement != null ? parentElement.Node : null);
            if (_node == null) return;
            _node.hint = hint;

            _node.incremented += HandleIncremented;
            _node.decremented += HandleDecremented;
            ApplyValue(_slider.value);
            _slider.onValueChanged.AddListener(ApplyValue);

            AccessibilityManager.Instance.NotifyLayoutChanged(_node);
        }

        private void OnDisable()
        {
            _slider.onValueChanged.RemoveListener(ApplyValue);
            if (_node == null || AccessibilityManager.Instance == null) return;
            _node.incremented -= HandleIncremented;
            _node.decremented -= HandleDecremented;
            AccessibilityManager.Instance.RemoveMenuNode(_node);
            _node = null;
        }

        private void HandleIncremented() => Step(1);
        private void HandleDecremented() => Step(-1);

        private void Step(int direction)
        {
            // Whole-number sliders (e.g. a 3-step difficulty picker) move
            // one step at a time; continuous sliders move in 5% increments
            // of their range - matches the granularity a sighted user gets
            // from a single arrow-key nudge on the same control.
            var step = _slider.wholeNumbers ? 1f : (_slider.maxValue - _slider.minValue) / 20f;
            _slider.value = Mathf.Clamp(_slider.value + direction * step, _slider.minValue, _slider.maxValue);
        }

        private void ApplyValue(float value)
        {
            if (_node == null) return;
            var index = Mathf.RoundToInt(value);
            _node.value = (valueLabels != null && index >= 0 && index < valueLabels.Length)
                ? valueLabels[index]
                : value.ToString("0.##");
        }

        private string ResolveTextLabel() => EchoesOfNeon.UI.AccessibleLabel.Resolve(this);
    }
}
