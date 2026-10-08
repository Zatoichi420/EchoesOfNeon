using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using UnityEngine;
using UnityEngine.UI;

namespace EchoesOfNeon.UI
{
    /// <summary>
    /// One row in the keybinding menu: shows an action's current binding
    /// and lets the player press this button to rebind it. Reads the
    /// display string in Start() (after InputManager has already loaded any
    /// saved overrides in its own Awake()) so it always reflects the
    /// player's actual current binding, never the hardcoded default.
    /// Only covers single-binding button actions (Fire, Jump, Interact,
    /// ...) - Move/Look are composite/continuous actions and interactive
    /// rebinding of a whole 4-way composite needs its own per-part UI this
    /// pass doesn't build; deliberately left out rather than half-built.
    /// </summary>
    [RequireComponent(typeof(AccessibleButton))]
    public class KeybindRow : MonoBehaviour
    {
        [SerializeField] private string actionName;
        [SerializeField] private Text valueText;
        [SerializeField] private InputManager.DeviceKind deviceKind = InputManager.DeviceKind.KeyboardMouse;

        private AccessibleButton _accessibleButton;
        private Button _button;

        public string ActionName { get => actionName; set => actionName = value; }
        public Text ValueText { get => valueText; set => valueText = value; }
        public InputManager.DeviceKind DeviceKind { get => deviceKind; set => deviceKind = value; }

        private void Awake()
        {
            _accessibleButton = GetComponent<AccessibleButton>();
            _button = GetComponent<Button>();
        }

        private void Start()
        {
            _button.onClick.AddListener(BeginRebind);
            Refresh();
        }

        private void Refresh()
        {
            if (InputManager.Instance == null || valueText == null) return;
            valueText.text = InputManager.Instance.GetBindingDisplayString(actionName, deviceKind);
            // The binding text sits next to the button but is not itself
            // focusable, so put it in the button's spoken hint.
            _accessibleButton.SetHint("Currently " + valueText.text);
        }

        private void BeginRebind()
        {
            if (InputManager.Instance == null) return;
            if (valueText != null) valueText.text = "Press a key...";
            AccessibilityManager.Instance?.Announce($"Rebinding {actionName.Replace(':', ' ')}. Press a key or button.");
            InputManager.Instance.StartRebind(actionName, () =>
            {
                InputManager.Instance.SaveBindingOverrides();
                Refresh();
                AccessibilityManager.Instance?.Announce($"{actionName.Replace(':', ' ')} bound to {(valueText != null ? valueText.text : "unknown")}.");
            });
        }
    }
}
