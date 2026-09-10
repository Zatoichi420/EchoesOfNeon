using EchoesOfNeon.Accessibility;
using EchoesOfNeon.Core;
using UnityEngine;

namespace EchoesOfNeon.Player
{
    /// <summary>
    /// Phase 3: first-person movement/look on top of InputManager +
    /// AccessibilityManager. Reads InputManager's raw Move/Look/Jump/
    /// Interact events directly, but reads Sprint/Crouch/Aim state from
    /// AccessibilityManager (not InputManager's raw press/release) since
    /// that's where toggle-vs-hold translation already happened.
    ///
    /// Two accessibility behaviours live here because they're inherently
    /// about movement/camera, not settings state: camera head-bob (skipped
    /// when Settings.cameraBobEnabled is off or reduceMotion is on), and
    /// single-stick mode (movement stick also drives facing, removing the
    /// need for a second look stick/mouse entirely).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class TacticalPlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cameraPivot;

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float sprintSpeed = 7f;
        [SerializeField] private float crouchSpeed = 2f;
        [SerializeField] private float aimMovementMultiplier = 0.6f;
        [SerializeField] private float jumpHeight = 1.1f;
        [SerializeField] private float gravity = -18f;

        [Header("Crouch")]
        [SerializeField] private float standingHeight = 1.8f;
        [SerializeField] private float crouchHeight = 1.0f;
        [SerializeField] private float crouchTransitionSpeed = 8f;

        [Header("Look")]
        [SerializeField] private float mouseSensitivity = 0.12f;   // degrees per pixel-delta unit
        [SerializeField] private float gamepadLookSpeed = 180f;    // degrees per second at full stick deflection
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;
        [SerializeField] private float singleStickTurnSpeed = 220f; // degrees per second, auto-face move direction

        [Header("Camera Bob")]
        [SerializeField] private float bobFrequency = 1.8f;
        [SerializeField] private float bobAmplitude = 0.045f;

        [Header("Interact")]
        [SerializeField] private float interactRange = 2.5f;
        [SerializeField] private LayerMask interactMask = ~0;

        private CharacterController _controller;
        private InputManager _input;
        private AccessibilityManager _accessibility;

        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private float _pitch;
        private float _verticalVelocity;
        private bool _jumpQueued;
        private float _currentHeight;
        private float _bobTimer;
        private Vector3 _cameraBasePosition;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _currentHeight = standingHeight;
            _controller.height = _currentHeight;
            _controller.center = new Vector3(0f, _currentHeight / 2f, 0f);
            if (cameraPivot != null) _cameraBasePosition = cameraPivot.localPosition;
        }

        private void Start()
        {
            // Start(), not Awake() - guarantees InputManager/AccessibilityManager
            // have already set their Instance in their own Awake(), regardless
            // of GameObject ordering in the scene.
            _input = InputManager.Instance;
            _accessibility = AccessibilityManager.Instance;

            if (_input == null)
            {
                Debug.LogError("[TacticalPlayerController] No InputManager in scene - disabling.");
                enabled = false;
                return;
            }

            _input.OnMove += HandleMove;
            _input.OnLook += HandleLook;
            _input.OnJump += HandleJump;
            _input.OnInteract += HandleInteract;
            _input.OnPause += HandlePause;

            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDestroy()
        {
            if (_input == null) return;
            _input.OnMove -= HandleMove;
            _input.OnLook -= HandleLook;
            _input.OnJump -= HandleJump;
            _input.OnInteract -= HandleInteract;
            _input.OnPause -= HandlePause;
        }

        private void HandleMove(Vector2 value) => _moveInput = value;
        private void HandleLook(Vector2 value) => _lookInput = value;

        private void HandleJump()
        {
            if (_controller.isGrounded) _jumpQueued = true;
        }

        private void HandlePause()
        {
            // Placeholder until a real pause-menu system (Phase 5/6) owns
            // this - without it, there'd be no way to free the mouse cursor
            // during testing.
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
                ? CursorLockMode.None
                : CursorLockMode.Locked;
        }

        private void HandleInteract()
        {
            if (cameraPivot == null) return;
            if (!Physics.Raycast(cameraPivot.position, cameraPivot.forward, out var hit, interactRange, interactMask))
                return;
            hit.collider.GetComponentInParent<IInteractable>()?.Interact(gameObject);
        }

        private void Update()
        {
            bool singleStick = _accessibility != null && _accessibility.Settings.singleStickMode;
            UpdateLook(singleStick);
            UpdateCrouchHeight();
            UpdateMovementAndGravity();
            UpdateCameraBob();
        }

        private void UpdateLook(bool singleStick)
        {
            if (cameraPivot == null) return;

            if (singleStick)
            {
                // Accessibility: the movement stick also drives facing, so
                // play is possible with only one analog stick/no mouse.
                if (_moveInput.sqrMagnitude > 0.0001f)
                {
                    var moveDir = transform.TransformDirection(new Vector3(_moveInput.x, 0f, _moveInput.y));
                    float targetYaw = Quaternion.LookRotation(new Vector3(moveDir.x, 0f, moveDir.z)).eulerAngles.y;
                    float newYaw = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetYaw, singleStickTurnSpeed * Time.deltaTime);
                    transform.rotation = Quaternion.Euler(0f, newYaw, 0f);
                }
                return;
            }

            float sensitivity = _accessibility != null ? _accessibility.Settings.lookSensitivity : 1f;
            float yawDelta, pitchDelta;
            if (_input.CurrentDevice == InputManager.DeviceKind.Gamepad)
            {
                yawDelta = _lookInput.x * gamepadLookSpeed * sensitivity * Time.deltaTime;
                pitchDelta = _lookInput.y * gamepadLookSpeed * sensitivity * Time.deltaTime;
            }
            else
            {
                // Mouse delta is already a per-frame pixel movement, not a
                // continuous rate - do not scale it by Time.deltaTime too,
                // or look speed becomes framerate-dependent in the wrong way.
                yawDelta = _lookInput.x * mouseSensitivity * sensitivity;
                pitchDelta = _lookInput.y * mouseSensitivity * sensitivity;
            }

            transform.Rotate(Vector3.up, yawDelta);
            _pitch = Mathf.Clamp(_pitch - pitchDelta, minPitch, maxPitch);
            cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void UpdateCrouchHeight()
        {
            bool crouching = _accessibility != null && _accessibility.IsCrouching;
            float targetHeight = crouching ? crouchHeight : standingHeight;
            _currentHeight = Mathf.MoveTowards(_currentHeight, targetHeight, crouchTransitionSpeed * Time.deltaTime);
            _controller.height = _currentHeight;
            _controller.center = new Vector3(0f, _currentHeight / 2f, 0f);
        }

        private void UpdateMovementAndGravity()
        {
            float speed = walkSpeed;
            if (_accessibility != null)
            {
                if (_accessibility.IsSprinting) speed = sprintSpeed;
                else if (_accessibility.IsCrouching) speed = crouchSpeed;
                if (_accessibility.IsAiming) speed *= aimMovementMultiplier;
            }

            Vector3 inputDir = Vector3.ClampMagnitude(new Vector3(_moveInput.x, 0f, _moveInput.y), 1f);
            Vector3 worldMove = transform.TransformDirection(inputDir) * speed;

            if (_controller.isGrounded)
            {
                _verticalVelocity = -1f; // small downward force to keep the controller grounded
                if (_jumpQueued)
                {
                    _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                    _jumpQueued = false;
                }
            }
            else
            {
                _verticalVelocity += gravity * Time.deltaTime;
            }

            Vector3 velocity = worldMove + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);
        }

        private void UpdateCameraBob()
        {
            if (cameraPivot == null) return;

            bool bobEnabled = _accessibility == null ||
                (_accessibility.Settings.cameraBobEnabled && !_accessibility.Settings.reduceMotion);
            Vector3 horizontalVelocity = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z);
            bool isMoving = _controller.isGrounded && horizontalVelocity.magnitude > 0.1f;

            if (bobEnabled && isMoving)
            {
                _bobTimer += Time.deltaTime * bobFrequency * (horizontalVelocity.magnitude / walkSpeed);
                float bobOffset = Mathf.Sin(_bobTimer * Mathf.PI * 2f) * bobAmplitude;
                cameraPivot.localPosition = _cameraBasePosition + new Vector3(0f, bobOffset, 0f);
            }
            else
            {
                _bobTimer = 0f;
                cameraPivot.localPosition = Vector3.Lerp(cameraPivot.localPosition, _cameraBasePosition, Time.deltaTime * 8f);
            }
        }
    }
}
