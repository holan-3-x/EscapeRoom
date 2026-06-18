using UnityEngine;
// Namespace needed to talk to the Unity UI Extensions package (UILineRenderer)
using UnityEngine.UI.Extensions;

[RequireComponent(typeof(UILineRenderer))]
public class WireUI : MonoBehaviour
{
    private UILineRenderer lineRenderer;

    [Header("Signal Colors")]
    [Tooltip("Wire color when the signal is FALSE (0)")]
    [SerializeField] private Color offColor = new Color(0.2f, 0.2f, 0.2f, 1f); // Dim grey

    [Tooltip("Wire color when the signal is TRUE (1)")]
    [SerializeField] private Color onColor = new Color(0f, 0.8f, 1f, 1f);       // Neon blue / glow

    void Awake()
    {
        // Grab the UILineRenderer on this same object
        lineRenderer = GetComponent<UILineRenderer>();

        // Start the wire in the "off" state
        SetState(false);
    }

    // Public method called by the PuzzleManager to update the circuit color
    public void SetState(bool isActive)
    {
        if (lineRenderer != null)
        {
            // Change the color based on the logical state
            lineRenderer.color = isActive ? onColor : offColor;

            // Force Unity's UI to immediately redraw the line with the new color
            lineRenderer.SetVerticesDirty();
        }
    }
}
