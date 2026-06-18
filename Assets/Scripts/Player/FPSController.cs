using UnityEngine;
using UnityEngine.InputSystem;

namespace SYSTEMESCAPE
{
    [RequireComponent(typeof(CharacterController))]
    public class FPSController : MonoBehaviour
    {
        public static FPSController Instance { get; private set; }

        [Header("Movement")]
        [SerializeField] private float walkSpeed    = 3.0f;
        [SerializeField] private float runSpeed     = 6.0f;
        [SerializeField] private float gravity      = -12f;

        [Header("Animation Sync")]
        // Match this to how fast the walk animation moves in world units per second.
        // Increase if feet slide forward, decrease if they slide backward.
        [SerializeField] private float walkAnimSpeed = 1.0f;
        [SerializeField] private float runAnimSpeed  = 2.0f;

        [Header("Look")]
        [SerializeField] private Transform cameraArm;
        [SerializeField] private float mouseSensitivity = 0.12f;
        [SerializeField] private float pitchMin         = -40f;
        [SerializeField] private float pitchMax         =  60f;

        [Header("Body")]
        [SerializeField] private Animator bodyAnimator;

        private CharacterController _cc;
        private float _verticalVelocity;
        private float _pitch;
        private bool  _movementEnabled = true;

        private static readonly int XVelHash = Animator.StringToHash("X_Velocity");
        private static readonly int YVelHash = Animator.StringToHash("Y_Velocity");

        // -- Lifecycle ---------------------------------------------------------

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        private void Start()
        {
            _cc = GetComponent<CharacterController>();
            _cc.stepOffset      = 0.3f;
            _cc.skinWidth       = 0.08f;
            _cc.slopeLimit      = 45f;
            _cc.minMoveDistance = 0f;
            _verticalVelocity   = -2f;
            LockCursor(true);
        }

        private void Update()
        {
            if (!_movementEnabled) return;
            HandleLook();
            HandleMove();
            HandleEscapeKey();
        }

        // -- Look --------------------------------------------------------------

        private void HandleLook()
        {
            if (Mouse.current == null) return;
            Vector2 delta = Mouse.current.delta.ReadValue() * mouseSensitivity;
            transform.Rotate(0f, delta.x, 0f);
            _pitch -= delta.y;
            _pitch  = Mathf.Clamp(_pitch, pitchMin, pitchMax);
            if (cameraArm != null)
                cameraArm.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        // -- Move --------------------------------------------------------------

        private void HandleMove()
        {
            if (Keyboard.current == null) return;

            bool running = Keyboard.current.leftShiftKey.isPressed ||
                           Keyboard.current.rightShiftKey.isPressed;

            float h = (Keyboard.current.dKey.isPressed ? 1f : 0f)
                    - (Keyboard.current.aKey.isPressed ? 1f : 0f);
            float v = (Keyboard.current.wKey.isPressed ? 1f : 0f)
                    - (Keyboard.current.sKey.isPressed ? 1f : 0f);

            Vector3 dir = (transform.forward * v + transform.right * h);
            dir.y = 0f;
            if (dir.sqrMagnitude > 1f) dir.Normalize();

            float speed = (dir.sqrMagnitude > 0f) ? (running ? runSpeed : walkSpeed) : 0f;

            if (_cc.isGrounded)
                _verticalVelocity = -2f;
            else
                _verticalVelocity = Mathf.Max(_verticalVelocity + gravity * Time.deltaTime, -20f);

            _cc.Move((dir * speed + Vector3.up * _verticalVelocity) * Time.deltaTime);

            // -- Animator: drive 2D blend tree ---------------------------------
            if (bodyAnimator != null && (_cc.isGrounded || speed > 0f))
            {
                Vector3 localDir = transform.InverseTransformDirection(dir);

                // Scale input by animation speed ratio so feet match ground movement.
                // walkAnimSpeed = 1.0 means animation naturally moves 1 unit/sec.
                // If feet slide, tune walkAnimSpeed in Inspector until they sync.
                float animScale = speed > 0f
                    ? speed / (running ? runAnimSpeed : walkAnimSpeed)
                    : 0f;

                if (HasParam(XVelHash))
                    bodyAnimator.SetFloat(XVelHash, localDir.x * animScale, 0.1f, Time.deltaTime);
                if (HasParam(YVelHash))
                    bodyAnimator.SetFloat(YVelHash, localDir.z * animScale, 0.1f, Time.deltaTime);
            }
        }

        // -- Helpers -----------------------------------------------------------

        private void HandleEscapeKey()
        {
            if (Keyboard.current == null) return;
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                LockCursor(Cursor.lockState == CursorLockMode.Locked ? false : true);
        }

        private bool HasParam(int hash)
        {
            if (bodyAnimator == null) return false;
            foreach (var p in bodyAnimator.parameters)
                if (p.nameHash == hash) return true;
            return false;
        }

        private static void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible   = !locked;
        }

        public void EnableMovement()  { _movementEnabled = true;  LockCursor(true); }
        public void DisableMovement() { _movementEnabled = false; LockCursor(false); }
        public Vector3 GetVelocity()  => _cc != null ? _cc.velocity : Vector3.zero;
        public void UpdateSensitivity(float v) { mouseSensitivity = v; }
    }
}
