using UnityEngine;
using UnityEngine.UI.Extensions;

// Draws a live circuit wire between two UI points with right-angle bends,
// and animates a glowing signal that flows along it when it powers on.
//
// Put this on a UI object that also has a UILineRenderer.
//
// SETUP:
//  - Start Point / End Point: the two RectTransforms to connect
//    (e.g. an input node -> a gate slot, or a gate slot -> the output node).
//  - Watch Slot (optional): if set, the wire turns ON (blue) when that slot
//    holds the CORRECT gate, OFF (grey) otherwise. Leave empty + use Manual State
//    for input/output wires that are always on.
//  - Routing: how the wire bends. Step = classic circuit-board right angles.
//
// Everything uses UNSCALED time, so it animates even while the minigame
// freezes the game (Time.timeScale = 0).

[RequireComponent(typeof(UILineRenderer))]
public class CircuitWire : MonoBehaviour
{
    public enum Routing { Straight, HorizontalThenVertical, VerticalThenHorizontal, Step }

    [Header("Endpoints to connect")]
    public RectTransform startPoint;
    public RectTransform endPoint;

    [Header("Wire shape")]
    [Tooltip("Step = classic circuit board look (horizontal, vertical, horizontal).")]
    public Routing routing = Routing.Step;

    [Header("Signal source (optional)")]
    [Tooltip("If set, wire is ON when this slot has the correct gate.")]
    public GateSlot watchSlot;
    [Tooltip("Used when no Watch Slot is set (e.g. an input wire that's always on).")]
    public bool manualState = false;

    [Header("Colors")]
    public Color offColor   = new Color(0.25f, 0.25f, 0.25f, 1f);
    public Color onColor    = new Color(0f, 0.8f, 1f, 1f);
    [Tooltip("Brighter color the glow pulses toward.")]
    public Color glowColor  = new Color(0.6f, 1f, 1f, 1f);

    [Header("Animation")]
    [Tooltip("Seconds for the signal to travel along the wire when it powers on.")]
    public float flowTime  = 0.35f;
    [Tooltip("How fast the glow pulses once the wire is fully on.")]
    public float pulseSpeed = 2.5f;

    private UILineRenderer _line;
    private bool  _wasOn = false;
    private float _flowProgress = 1f;   // 0..1: how far the signal has travelled

    private void Awake()
    {
        _line = GetComponent<UILineRenderer>();
    }

    private void LateUpdate()
    {
        if (_line == null || startPoint == null || endPoint == null) return;

        // Endpoints in this object's local space (UILineRenderer uses local points)
        Vector2 a = transform.InverseTransformPoint(startPoint.position);
        Vector2 b = transform.InverseTransformPoint(endPoint.position);

        bool on = watchSlot != null ? watchSlot.IsCorrect() : manualState;

        // When it just turned ON, restart the "signal travels along the wire" animation
        if (on && !_wasOn) _flowProgress = 0f;
        if (!on)           _flowProgress = 1f;   // off = show the full grey wire
        _wasOn = on;

        // Advance the flow using UNSCALED time (works at timeScale 0)
        if (on && _flowProgress < 1f)
            _flowProgress = Mathf.Min(1f, _flowProgress + Time.unscaledDeltaTime / Mathf.Max(0.01f, flowTime));

        // The signal end-point grows from A toward B as it "flows"
        Vector2 reachedB = Vector2.Lerp(a, b, _flowProgress);
        _line.Points = BuildPath(a, reachedB);

        // Color: grey when off; pulsing glow when on
        if (on)
        {
            float t = Mathf.PingPong(Time.unscaledTime * pulseSpeed, 1f);
            _line.color = Color.Lerp(onColor, glowColor, t);
        }
        else
        {
            _line.color = offColor;
        }

        _line.SetAllDirty();
    }

    // Builds the right-angle path between two local points
    private Vector2[] BuildPath(Vector2 a, Vector2 b)
    {
        switch (routing)
        {
            case Routing.HorizontalThenVertical:
                return new[] { a, new Vector2(b.x, a.y), b };

            case Routing.VerticalThenHorizontal:
                return new[] { a, new Vector2(a.x, b.y), b };

            case Routing.Step:
                float midX = (a.x + b.x) * 0.5f;
                return new[] { a, new Vector2(midX, a.y), new Vector2(midX, b.y), b };

            default: // Straight
                return new[] { a, b };
        }
    }

    // Lets other scripts force the wire on/off (for input wires, etc.)
    public void SetManualState(bool on) => manualState = on;
}
