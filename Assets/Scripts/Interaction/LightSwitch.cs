using UnityEngine;
using DG.Tweening;
using SYSTEMESCAPE;

// Room light switch — the 1-bit teaching lesson. Uses E key.
// Keep this on the modern-light-switch 3D model (layer: Interactable, needs a Collider).
//
// It does NOT own the lights — ClassroomSceneFlow owns them so the intro and the
// switch never fight over light state. Flipping it just asks the flow to toggle.

public class LightSwitch : MonoBehaviour, IInteractable
{
    [Header("Switch handle (optional visual)")]
    public Transform switchHandle;
    public float onRotation  = -40f;
    public float offRotation =  40f;

    private bool _isOn = false;

    private void Start()
    {
        // Just set the handle to the OFF position. Do NOT touch the lights here —
        // the room starts lit for the intro and ClassroomSceneFlow controls it.
        if (switchHandle != null)
            switchHandle.localRotation = Quaternion.Euler(offRotation, 0, 0);
    }

    public string GetPrompt() => _isOn ? "[E] Turn light OFF" : "[E] Turn light ON";

    public void Interact()
    {
        _isOn = !_isOn;

        // Rotate the handle
        if (switchHandle != null)
        {
            float target = _isOn ? onRotation : offRotation;
            switchHandle.DOLocalRotate(new Vector3(target, 0, 0), 0.2f);
        }

        // Let the scene flow toggle the actual room lights
        ClassroomSceneFlow.Instance?.PlayerToggleLights();
        SoundManager.Instance?.PlaySwitchFlip();

        // Advance ARIA's binary lesson (teaches: ON = 1, OFF = 0)
        ARIATerminal.Instance?.OnTeachingLightFlipped();
    }
}
