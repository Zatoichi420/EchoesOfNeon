using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfNeon.Core
{
    /// <summary>
    /// Phase 1: the single source of truth for input. Keyboard/mouse and
    /// gamepad (Xbox/PlayStation/generic) are both bound to every action and
    /// hot-swappable at runtime - whichever device the player last touched
    /// becomes CurrentDevice, broadcast via OnDeviceChanged so UI can update
    /// button glyphs accordingly (per the project's design doc).
    ///
    /// Everything else (player controller, accessibility layer, UI) listens
    /// to this class's events rather than touching Input System actions
    /// directly - decoupled event-driven architecture, per MEMORY.md's
    /// architectural rules. Toggle-vs-hold translation for Sprint/Crouch/Aim
    /// (an accessibility option) is deliberately NOT done here - this class
    /// only reports raw press/release; AccessibilityManager (Phase 2) is
    /// where that preference gets applied.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        public enum DeviceKind { KeyboardMouse, Gamepad }

        public DeviceKind CurrentDevice { get; private set; } = DeviceKind.KeyboardMouse;

        public event Action<DeviceKind> OnDeviceChanged;

        public event Action<Vector2> OnMove;
        public event Action<Vector2> OnLook;
        public event Action OnFirePressed;
        public event Action OnFireReleased;
        public event Action OnAimPressed;
        public event Action OnAimReleased;
        public event Action OnJump;
        public event Action OnInteract;
        public event Action OnSprintPressed;
        public event Action OnSprintReleased;
        public event Action OnCrouchPressed;
        public event Action OnCrouchReleased;
        public event Action OnReload;
        public event Action OnPause;
        public event Action OnSonarPulse;          // Oculus Sensory Suite: echolocation ping
        public event Action OnNeuralDilatePressed; // Oculus Sensory Suite: focus/slow-mo, held
        public event Action OnNeuralDilateReleased;
        public event Action OnFendPressed;         // Side-scroller: cane fend-off (parry)
        public event Action OnWhistlePressed;      // Side-scroller: whistle sends Cookie after an enemy
        public event Action OnEchoStepPressed;     // Side-scroller: Echo Step dash (Lv2)
        public event Action OnSpecialPressed;      // Side-scroller: fire the selected special
        public event Action OnCycleSpecialPressed; // Side-scroller: choose the next unlocked special
        public event Action OnHowlPressed;         // Side-scroller: Cookie's Shock Howl (Cookie Lv9)
        public event Action OnJournalPressed;      // Side-scroller: replay the latest lore clue
        public event Action OnHintPressed;         // Read out the current controls
        public event Action OnRepeatPressed;       // Say the last announcement again
        public event Action OnStatusPressed;       // Side-scroller: speak health / level / special / strain

        private InputActionMap _gameplayMap;
        private InputAction _move, _look, _fire, _aim, _jump, _interact, _sprint, _crouch, _reload, _pause, _sonar, _dilate, _fend, _whistle, _echoStep, _special, _cycleSpecial, _howl, _journal, _status, _repeat, _hint;

        private const string RebindsPrefsKey = "EchoesOfNeon.InputRebinds";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildActions();
            LoadBindingOverrides();
        }

        private void BuildActions()
        {
            _gameplayMap = new InputActionMap("Gameplay");

            _move = _gameplayMap.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            _move.AddBinding("<Gamepad>/leftStick");

            _look = _gameplayMap.AddAction("Look", InputActionType.Value, expectedControlLayout: "Vector2");
            _look.AddBinding("<Mouse>/delta");
            _look.AddBinding("<Gamepad>/rightStick");

            _fire = _gameplayMap.AddAction("Fire", InputActionType.Button);
            _fire.AddBinding("<Mouse>/leftButton");
            _fire.AddBinding("<Gamepad>/rightTrigger");

            _aim = _gameplayMap.AddAction("Aim", InputActionType.Button);
            _aim.AddBinding("<Mouse>/rightButton");
            _aim.AddBinding("<Gamepad>/leftTrigger");

            _jump = _gameplayMap.AddAction("Jump", InputActionType.Button);
            _jump.AddBinding("<Keyboard>/space");
            _jump.AddBinding("<Gamepad>/buttonSouth");

            _interact = _gameplayMap.AddAction("Interact", InputActionType.Button);
            _interact.AddBinding("<Keyboard>/e");
            _interact.AddBinding("<Gamepad>/buttonWest");

            _sprint = _gameplayMap.AddAction("Sprint", InputActionType.Button);
            _sprint.AddBinding("<Keyboard>/leftShift");
            _sprint.AddBinding("<Gamepad>/leftStickPress");

            _crouch = _gameplayMap.AddAction("Crouch", InputActionType.Button);
            _crouch.AddBinding("<Keyboard>/leftCtrl");
            _crouch.AddBinding("<Gamepad>/buttonEast");

            _reload = _gameplayMap.AddAction("Reload", InputActionType.Button);
            _reload.AddBinding("<Keyboard>/r");
            _reload.AddBinding("<Gamepad>/buttonNorth");

            _pause = _gameplayMap.AddAction("Pause", InputActionType.Button);
            _pause.AddBinding("<Keyboard>/escape");
            _pause.AddBinding("<Gamepad>/start");

            _sonar = _gameplayMap.AddAction("SonarPulse", InputActionType.Button);
            _sonar.AddBinding("<Keyboard>/q");
            _sonar.AddBinding("<Gamepad>/rightShoulder");

            _dilate = _gameplayMap.AddAction("NeuralDilate", InputActionType.Button);
            _dilate.AddBinding("<Keyboard>/leftAlt");
            _dilate.AddBinding("<Gamepad>/leftShoulder");

            _fend = _gameplayMap.AddAction("Fend", InputActionType.Button);
            _fend.AddBinding("<Keyboard>/f");
            _fend.AddBinding("<Gamepad>/leftTrigger");

            _whistle = _gameplayMap.AddAction("Whistle", InputActionType.Button);
            _whistle.AddBinding("<Keyboard>/v");
            _whistle.AddBinding("<Gamepad>/dpad/up");

            _echoStep = _gameplayMap.AddAction("EchoStep", InputActionType.Button);
            _echoStep.AddBinding("<Keyboard>/z"); _echoStep.AddBinding("<Gamepad>/dpad/left");
            _special = _gameplayMap.AddAction("Special", InputActionType.Button);
            _special.AddBinding("<Keyboard>/c"); _special.AddBinding("<Gamepad>/buttonNorth");
            _cycleSpecial = _gameplayMap.AddAction("CycleSpecial", InputActionType.Button);
            _cycleSpecial.AddBinding("<Keyboard>/tab"); _cycleSpecial.AddBinding("<Gamepad>/dpad/right");
            _howl = _gameplayMap.AddAction("Howl", InputActionType.Button);
            _howl.AddBinding("<Keyboard>/b"); _howl.AddBinding("<Gamepad>/dpad/down");
            _journal = _gameplayMap.AddAction("Journal", InputActionType.Button);
            _journal.AddBinding("<Keyboard>/y"); _journal.AddBinding("<Gamepad>/select");
            _status = _gameplayMap.AddAction("Status", InputActionType.Button);
            _status.AddBinding("<Keyboard>/h"); _status.AddBinding("<Gamepad>/rightStickPress");
            _repeat = _gameplayMap.AddAction("RepeatLast", InputActionType.Button);
            _repeat.AddBinding("<Keyboard>/v");
            _hint = _gameplayMap.AddAction("Hint", InputActionType.Button);
            _hint.AddBinding("<Keyboard>/f1");

            _move.performed += ctx => { NoteDevice(ctx); OnMove?.Invoke(ctx.ReadValue<Vector2>()); };
            _move.canceled += _ => OnMove?.Invoke(Vector2.zero);

            _look.performed += ctx => { NoteDevice(ctx); OnLook?.Invoke(ctx.ReadValue<Vector2>()); };
            _look.canceled += _ => OnLook?.Invoke(Vector2.zero);

            _fire.started += ctx => { NoteDevice(ctx); OnFirePressed?.Invoke(); };
            _fire.canceled += _ => OnFireReleased?.Invoke();

            _aim.started += ctx => { NoteDevice(ctx); OnAimPressed?.Invoke(); };
            _aim.canceled += _ => OnAimReleased?.Invoke();

            _jump.started += ctx => { NoteDevice(ctx); OnJump?.Invoke(); };
            _interact.started += ctx => { NoteDevice(ctx); OnInteract?.Invoke(); };

            _sprint.started += ctx => { NoteDevice(ctx); OnSprintPressed?.Invoke(); };
            _sprint.canceled += _ => OnSprintReleased?.Invoke();

            _crouch.started += ctx => { NoteDevice(ctx); OnCrouchPressed?.Invoke(); };
            _crouch.canceled += _ => OnCrouchReleased?.Invoke();

            _reload.started += ctx => { NoteDevice(ctx); OnReload?.Invoke(); };
            _pause.started += ctx => { NoteDevice(ctx); OnPause?.Invoke(); };

            _sonar.started += ctx => { NoteDevice(ctx); OnSonarPulse?.Invoke(); };

            _fend.started += ctx => { NoteDevice(ctx); OnFendPressed?.Invoke(); };
            _whistle.started += ctx => { NoteDevice(ctx); OnWhistlePressed?.Invoke(); };

            _echoStep.started += ctx => { NoteDevice(ctx); OnEchoStepPressed?.Invoke(); };
            _special.started += ctx => { NoteDevice(ctx); OnSpecialPressed?.Invoke(); };
            _cycleSpecial.started += ctx => { NoteDevice(ctx); OnCycleSpecialPressed?.Invoke(); };
            _howl.started += ctx => { NoteDevice(ctx); OnHowlPressed?.Invoke(); };
            _journal.started += ctx => { NoteDevice(ctx); OnJournalPressed?.Invoke(); };
            _status.started += ctx => { NoteDevice(ctx); OnStatusPressed?.Invoke(); };
            _repeat.started += ctx => { NoteDevice(ctx); OnRepeatPressed?.Invoke(); };
            _hint.started += ctx => { NoteDevice(ctx); OnHintPressed?.Invoke(); };

            _dilate.started += ctx => { NoteDevice(ctx); OnNeuralDilatePressed?.Invoke(); };
            _dilate.canceled += _ => OnNeuralDilateReleased?.Invoke();
        }

        private void NoteDevice(InputAction.CallbackContext ctx)
        {
            var device = ctx.control?.device;
            var kind = device is Gamepad ? DeviceKind.Gamepad : DeviceKind.KeyboardMouse;
            if (kind == CurrentDevice) return;
            CurrentDevice = kind;
            OnDeviceChanged?.Invoke(CurrentDevice);
        }

        /// <summary>Turns every gameplay action off (or back on) except Pause,
        /// so a pause menu can freeze movement, look, fire and abilities
        /// while Escape/Start can still reach it to resume. Disabling fires
        /// each action's canceled callback, which releases held inputs.</summary>
        public void SetGameplayInputEnabled(bool enabled)
        {
            if (_gameplayMap == null) return;
            foreach (var action in _gameplayMap.actions)
            {
                if (action == _pause) continue;
                if (enabled) action.Enable(); else action.Disable();
            }
        }

        private void OnEnable() => _gameplayMap?.Enable();
        private void OnDisable() => _gameplayMap?.Disable();
        private void OnDestroy() => _gameplayMap?.Dispose();

        /// <summary>Persisted as JSON via PlayerPrefs, per MEMORY.md's "Full
        /// runtime remapping with persistent JSON serialization."</summary>
        public void SaveBindingOverrides()
        {
            string json = _gameplayMap.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(RebindsPrefsKey, json);
            PlayerPrefs.Save();
        }

        public void LoadBindingOverrides()
        {
            if (!PlayerPrefs.HasKey(RebindsPrefsKey)) return;
            _gameplayMap.LoadBindingOverridesFromJson(PlayerPrefs.GetString(RebindsPrefsKey));
        }

        /// <summary>Every rebindable action name, in the fixed order they
        /// were added in BuildActions() - the keybinding menu lists these
        /// rather than hardcoding its own copy of the action list, so a new
        /// action added here shows up in the UI automatically.</summary>
        public string[] GetActionNames()
        {
            var names = new string[_gameplayMap.actions.Count];
            for (int i = 0; i < names.Length; i++) names[i] = _gameplayMap.actions[i].name;
            return names;
        }

        /// <summary>Human-readable current binding for one action (e.g.
        /// "W/A/S/D" or "Space") for the given device kind, for the
        /// keybinding menu to display next to each "press to rebind"
        /// button. Returns "-" if the action isn't found rather than
        /// throwing, since a menu row shouldn't crash the whole screen over
        /// one bad lookup.</summary>
        public string GetBindingDisplayString(string actionName, DeviceKind deviceKind)
        {
            SplitActionName(actionName, out var actionOnly, out var part);
            var action = _gameplayMap.FindAction(actionOnly);
            if (action == null) return "-";

            for (int i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];
                if (binding.isComposite) continue;
                if (part != null) { if (!binding.isPartOfComposite || !string.Equals(binding.name, part, StringComparison.OrdinalIgnoreCase)) continue; }
                else if (binding.isPartOfComposite) continue;
                var path = binding.effectivePath;
                bool matches = deviceKind == DeviceKind.Gamepad
                    ? path.Contains("<Gamepad>")
                    : (path.Contains("<Keyboard>") || path.Contains("<Mouse>"));
                if (matches) return InputControlPath.ToHumanReadableString(path, InputControlPath.HumanReadableStringOptions.OmitDevice);
            }
            return "-";
        }

        /// <summary>"Move:Up" -> action "Move", part "Up". A plain name has no part.</summary>
        public static void SplitActionName(string name, out string action, out string part)
        {
            int colon = name.IndexOf(':');
            if (colon < 0) { action = name; part = null; return; }
            action = name.Substring(0, colon);
            part = name.Substring(colon + 1);
        }

        /// <summary>Starts interactive rebinding for the named action (e.g.
        /// "Jump", "SonarPulse"). Caller is responsible for prompting the
        /// player and calling SaveBindingOverrides() in onComplete.</summary>
        public InputActionRebindingExtensions.RebindingOperation StartRebind(string actionName, Action onComplete, Action onCancel = null, Action<string> onConflict = null)
        {
            SplitActionName(actionName, out var actionOnly, out var part);
            var action = _gameplayMap.FindAction(actionOnly);
            if (action == null)
            {
                Debug.LogWarning($"[InputManager] No action named '{actionName}' to rebind.");
                return null;
            }
            int partIndex = -1;
            if (part != null)
            {
                for (int i = 0; i < action.bindings.Count; i++)
                    if (action.bindings[i].isPartOfComposite && string.Equals(action.bindings[i].name, part, StringComparison.OrdinalIgnoreCase)
                        && action.bindings[i].path.Contains("<Keyboard>")) { partIndex = i; break; }
                if (partIndex < 0)
                {
                    Debug.LogWarning($"[InputManager] No keyboard part '{part}' on '{actionOnly}'.");
                    return null;
                }
            }
            // Input System refuses to rebind an enabled action.
            action.Disable();
            var rebind = partIndex >= 0 ? action.PerformInteractiveRebinding(partIndex) : action.PerformInteractiveRebinding();
            return rebind
                .WithControlsExcluding("Mouse/position")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnMatchWaitForAnother(0.1f)
                .OnCancel(op =>
                {
                    op.Dispose();
                    action.Enable();
                    onCancel?.Invoke();
                })
                .OnComplete(op =>
                {
                    int idx = partIndex;
                    if (idx < 0)
                        for (int i = 0; i < action.bindings.Count; i++)
                            if (!action.bindings[i].isComposite && !action.bindings[i].isPartOfComposite) { idx = i; break; }
                    string conflict = idx >= 0 ? FindConflict(action, action.bindings[idx].effectivePath) : null;
                    op.Dispose();
                    action.Enable();
                    if (conflict != null) onConflict?.Invoke(conflict);
                    onComplete?.Invoke();
                })
                .Start();
        }

        /// <summary>Name of another gameplay action already using this path, or null.
        /// Lets the UI say so out loud instead of silently double-binding a key.</summary>
        private string FindConflict(InputAction rebound, string path)
        {
            foreach (var other in _gameplayMap.actions)
            {
                if (other == rebound) continue;
                foreach (var b in other.bindings)
                    if (!b.isComposite && b.effectivePath == path) return other.name;
            }
            return null;
        }
    }
}
