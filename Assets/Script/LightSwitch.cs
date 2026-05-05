using UnityEngine;
using UnityEngine.InputSystem;

public class LightSwitch : MonoBehaviour
{
    [Header("Switch Parts")]
    [Tooltip("The bone or object that will rotate (e.g., switchbone001)")]
    public Transform switchHandle;
    
    [Header("Target Lights")]
    [Tooltip("The light objects to be toggled on/off")]
    public GameObject[] lights;

    [Header("Settings")]
    public float onRotation = -40f;
    public float offRotation = 40f;
    public float transitionSpeed = 10f;

    private bool isOn = false;

    void Start()
    {
        // Initialize to OFF (40 degrees)
        if (switchHandle != null)
        {
            switchHandle.localRotation = Quaternion.Euler(offRotation, 0, 0);
        }
        UpdateLights();
    }

    void Update()
    {
        // 1. Click Detection (using Raycast)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleClick();
        }

        // 2. Smooth Rotation
        if (switchHandle != null)
        {
            float targetAngle = isOn ? onRotation : offRotation;
            Quaternion targetRotation = Quaternion.Euler(targetAngle, 0, 0);
            switchHandle.localRotation = Quaternion.Slerp(switchHandle.localRotation, targetRotation, Time.deltaTime * transitionSpeed);
        }
    }

    void HandleClick()
    {
        if (Camera.main == null)
        {
            Debug.LogError("LightSwitch: No Camera tagged 'MainCamera' found in the scene! Please tag your camera as MainCamera.");
            return;
        }

        // We cast a ray from the Main Camera to the mouse position
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Check if we clicked this object or the handle directly
            // Note: The object MUST have a Collider component for this to work!
            if (hit.transform == transform || hit.transform == switchHandle)
            {
                Toggle();
            }
        }
    }

    public void Toggle()
    {
        isOn = !isOn;
        UpdateLights();
        Debug.Log("Switch Toggled: " + (isOn ? "ON" : "OFF"));
    }

    void UpdateLights()
    {
        if (lights == null) return;
        
        foreach (GameObject lightObj in lights)
        {
            if (lightObj != null)
            {
                lightObj.SetActive(isOn);
            }
        }
    }
}
