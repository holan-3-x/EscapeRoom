using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5.0f;
    
    [Header("Look")]
    public float mouseSensitivity = 0.1f;
    public float upDownRange = 80.0f;

    private Vector2 rotation = Vector2.zero;

    void Start()
    {
        // Lock cursor for a better experience
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize rotation from current transform
        Vector3 angles = transform.localRotation.eulerAngles;
        rotation.y = angles.y;
        rotation.x = angles.x;
        
        // Normalize x rotation to handle Unity's 0-360 representation
        if (rotation.x > 180) rotation.x -= 360;
    }

    void Update()
    {
        HandleRotation();
        HandleMovement();
        
        // Unlock cursor with Escape key
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void HandleRotation()
    {
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity;

            rotation.y += mouseDelta.x;
            rotation.x -= mouseDelta.y;
            rotation.x = Mathf.Clamp(rotation.x, -upDownRange, upDownRange);

            transform.localRotation = Quaternion.Euler(rotation.x, rotation.y, 0);
        }
    }

    void HandleMovement()
    {
        Vector3 moveDir = Vector3.zero;
        if (Keyboard.current != null)
        {
            // Calculate directions based on horizontal plane
            Vector3 forward = transform.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = transform.right;
            right.y = 0;
            right.Normalize();

            if (Keyboard.current.wKey.isPressed) moveDir += forward;
            if (Keyboard.current.sKey.isPressed) moveDir -= forward;
            if (Keyboard.current.aKey.isPressed) moveDir -= right;
            if (Keyboard.current.dKey.isPressed) moveDir += right;
        }

        if (moveDir.sqrMagnitude > 0)
        {
            transform.position += moveDir.normalized * moveSpeed * Time.deltaTime;
        }
    }
}
