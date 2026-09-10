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

        private InputActionMap _gameplayMap;
        private InputAction _move, _look, _fire, _aim, _jump, _interact, _sprint, _crouch, _reload, _pause, _sonar, _dilate;

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

            _move.performed += ctx => { NoteDevice(ctx); OnMove?.Invoke(ctx.ReadValue<Vector2>()); };
            _move.canceled += _ => OnMove?.Invoke(Vector2.zero);

            _look.performed += ctx => { NoteDevice(ctx); OnLook?.Invoke(ctx.ReadValue<Vector2>()); };

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

        /// <summary>Starts interactive rebinding for the named action (e.g.
        /// "Jump", "SonarPulse"). Caller is responsible for prompting the
        /// player and calling SaveBindingOverrides() in onComplete.</summary>
        public InputActionRebindingExtensions.RebindingOperation StartRebind(string actionName, Action onComplete)
        {
            var action = _gameplayMap.FindAction(actionName);
            if (action == null)
            {
                Debug.LogWarning($"[InputManager] No action named '{actionName}' to rebind.");
                return null;
            }
            action.Disable();
            return action.PerformInteractiveRebinding()
                .WithControlsExcluding("Mouse/position")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(op =>
                {
                    op.Dispose();
                    action.Enable();
                    onComplete?.Invoke();
                })
                .Start();
        }
    }
}
