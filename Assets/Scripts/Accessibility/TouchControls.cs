using EchoesOfNeon.Core;
using UnityEngine;
using UnityEngine.Accessibility;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace EchoesOfNeon.Accessibility
{
    /// <summary>
    /// Phone controls for the side-scroller. Gestures go through TouchGestureRecognizer and come
    /// out as the same InputManager events the keyboard and gamepad raise.
    ///
    /// With VoiceOver on, the screen is two accessibility nodes: a Pause button along the top,
    /// and a "direct touch" game area below it, so VoiceOver hands swipes and taps to the game
    /// instead of using them to move its own focus. Without VoiceOver, tapping the top strip pauses.
    /// A Bluetooth keyboard or game controller keeps working through InputManager as before.
    /// Created automatically on phones and tablets; nothing to add to a scene.
    /// </summary>
    public class TouchControls : MonoBehaviour
    {
        public const float PauseStripFraction = 0.12f;   // top of the screen

        public const string Guide =
            "Touch controls. " +
            "One finger: touch and hold the left or right half of the screen to walk that way. Tap to strike. " +
            "Swipe up to jump, swipe down to fend off an attack, swipe left or right to Echo Step that way. " +
            "Two fingers: tap for a sonar pulse, double tap to interact, swipe up to whistle for Cookie, " +
            "swipe down for Cookie's howl, swipe left or right for the next special. " +
            "Three fingers: tap to hear your status, swipe up to use your special, swipe down to repeat the last message, " +
            "swipe left or right to replay the latest clue, double tap to hear these controls again. " +
            "Pause is at the top of the screen.";

        private readonly TouchGestureRecognizer _gestures = new TouchGestureRecognizer();
        private InputManager _input;
        private AccessibilityNode _gameNode, _pauseNode;
        private bool _nodesShown;
        private int _pauseFinger = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateOnPhones()
        {
            if (!Application.isMobilePlatform || FindFirstObjectByType<TouchControls>() != null) return;
            var go = new GameObject("TouchControls");
            DontDestroyOnLoad(go);
            go.AddComponent<TouchControls>();
        }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            _gestures.OnGesture += Handle;
        }

        private void OnDisable()
        {
            _gestures.OnGesture -= Handle;
            _gestures.Cancel();
            ShowNodes(false);
        }

        private void OnApplicationPause(bool paused) { if (paused) _gestures.Cancel(); }

        private void Update()
        {
            _input = InputManager.Instance;
            if (_input == null) return;
            ShowNodes(_input.GameplayEnabled);
            _gestures.SetScreen(Screen.width, Screen.height);

            foreach (var t in ETouch.activeTouches)
            {
                int id = t.touchId;
                Vector2 pos = t.screenPosition;
                float time = (float)t.time;
                switch (t.phase)
                {
                    case UnityEngine.InputSystem.TouchPhase.Began:
                        // A touch that starts in the top strip is the Pause button, not a gesture.
                        if (InPauseStrip(pos)) { _pauseFinger = id; break; }
                        _gestures.Down(id, pos, time);
                        break;
                    case UnityEngine.InputSystem.TouchPhase.Moved:
                    case UnityEngine.InputSystem.TouchPhase.Stationary:
                        if (id != _pauseFinger) _gestures.Move(id, pos, time);
                        break;
                    case UnityEngine.InputSystem.TouchPhase.Ended:
                        if (id == _pauseFinger) { _pauseFinger = -1; if (InPauseStrip(pos)) _input.RaiseTouch(InputManager.TouchCommand.Pause); break; }
                        _gestures.Up(id, pos, time);
                        break;
                    case UnityEngine.InputSystem.TouchPhase.Canceled:
                        if (id == _pauseFinger) { _pauseFinger = -1; break; }
                        _gestures.Up(id, pos, time);
                        break;
                }
            }
            // Touch times use the Input System clock (realtime since startup), so tick on the same one.
            _gestures.Tick((float)Time.realtimeSinceStartupAsDouble);
        }

        private static bool InPauseStrip(Vector2 pos) => pos.y > Screen.height * (1f - PauseStripFraction);

        private void Handle(TouchGestureRecognizer.Gesture g)
        {
            if (_input == null) return;
            if (g.kind == TouchGestureRecognizer.Kind.HoldStart || g.kind == TouchGestureRecognizer.Kind.HoldMove)
            {
                _input.TouchMove(g.dir == TouchGestureRecognizer.Dir.Left ? -1f : 1f);
                return;
            }
            if (g.kind == TouchGestureRecognizer.Kind.HoldEnd) { _input.TouchMove(0f); return; }
            if (Map(g) is InputManager.TouchCommand c) _input.RaiseTouch(c);
        }

        /// <summary>Which game command a gesture means (null = none). Public for tests.</summary>
        public static InputManager.TouchCommand? Map(TouchGestureRecognizer.Gesture g)
        {
            var D = g.dir;
            bool sideways = D == TouchGestureRecognizer.Dir.Left || D == TouchGestureRecognizer.Dir.Right;
            switch (g.kind, g.fingers)
            {
                case (TouchGestureRecognizer.Kind.Tap, 1): return InputManager.TouchCommand.Strike;
                case (TouchGestureRecognizer.Kind.Tap, 2): return InputManager.TouchCommand.Sonar;
                case (TouchGestureRecognizer.Kind.Tap, 3): return InputManager.TouchCommand.Status;
                case (TouchGestureRecognizer.Kind.DoubleTap, 2): return InputManager.TouchCommand.Interact;
                case (TouchGestureRecognizer.Kind.DoubleTap, 3): return InputManager.TouchCommand.Hint;
                case (TouchGestureRecognizer.Kind.Swipe, 1):
                    if (D == TouchGestureRecognizer.Dir.Up) return InputManager.TouchCommand.Jump;
                    if (D == TouchGestureRecognizer.Dir.Down) return InputManager.TouchCommand.Fend;
                    return D == TouchGestureRecognizer.Dir.Left ? InputManager.TouchCommand.EchoStepLeft : InputManager.TouchCommand.EchoStepRight;
                case (TouchGestureRecognizer.Kind.Swipe, 2):
                    if (D == TouchGestureRecognizer.Dir.Up) return InputManager.TouchCommand.Whistle;
                    if (D == TouchGestureRecognizer.Dir.Down) return InputManager.TouchCommand.Howl;
                    return sideways ? InputManager.TouchCommand.CycleSpecial : (InputManager.TouchCommand?)null;
                case (TouchGestureRecognizer.Kind.Swipe, 3):
                    if (D == TouchGestureRecognizer.Dir.Up) return InputManager.TouchCommand.Special;
                    if (D == TouchGestureRecognizer.Dir.Down) return InputManager.TouchCommand.RepeatLast;
                    return sideways ? InputManager.TouchCommand.Journal : (InputManager.TouchCommand?)null;
                default: return null;
            }
        }

        // The game area and Pause button exist only during play; menus bring their own nodes.
        private void ShowNodes(bool show)
        {
            if (show == _nodesShown) return;
            var am = AccessibilityManager.Instance;
            if (am == null) return;
            _nodesShown = show;
            if (show)
            {
                _pauseNode = am.CreateScreenNode("Pause", AccessibilityRole.Button,
                    () => new Rect(0, 0, Screen.width, Screen.height * PauseStripFraction));
                if (_pauseNode != null)
                    _pauseNode.invoked += () => { _input?.RaiseTouch(InputManager.TouchCommand.Pause); return true; };
                _gameNode = am.CreateScreenNode("Game area. Direct touch. Three-finger double tap for the controls.", AccessibilityRole.None,
                    () => new Rect(0, Screen.height * PauseStripFraction, Screen.width, Screen.height * (1f - PauseStripFraction)),
                    directTouch: true);
                am.NotifyLayoutChanged(_gameNode);
            }
            else
            {
                _gestures.Cancel();
                am.RemoveMenuNode(_gameNode);
                am.RemoveMenuNode(_pauseNode);
                _gameNode = _pauseNode = null;
                am.NotifyLayoutChanged();
            }
        }
    }
}
