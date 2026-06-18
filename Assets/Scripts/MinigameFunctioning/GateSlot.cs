using UnityEngine;
// DOTween animation library namespace
using DG.Tweening;

public class GateSlot : MonoBehaviour
{
    public LogicGate currentGate;

    [Header("Correct solution for this slot")]
    public GateType correctGate;

    public void PlaceGate(DraggableUI draggable)
    {
        LogicGate gate = draggable.GetComponent<LogicGate>();
        if (gate == null) return;

        GateSlot sourceSlot = draggable.CurrentSlot;

        if (currentGate != null)
        {
            // SWAP SCENARIO: There is already a gate in the slot
            DraggableUI gateToKickDraggable = currentGate.GetComponent<DraggableUI>();

            if (sourceSlot != null)
            {
                // Move this slot's current gate over to the slot of the incoming gate
                sourceSlot.currentGate = this.currentGate;
                gateToKickDraggable.SetSlot(sourceSlot);
            }
        }
        else
        {
            // NO SWAP SCENARIO: This slot was empty.
            // Just clear the old slot the incoming gate came from.
            if (sourceSlot != null)
            {
                sourceSlot.RemoveGate();
            }
        }

        // Assign the incoming gate to this slot
        currentGate = gate;
        draggable.SetSlot(this);
    }

    // Empties the slot when its gate is dragged away
    public void RemoveGate()
    {
        currentGate = null;
    }

    // Evaluate each gate slot internally based on current and correct gate
    public bool IsCorrect()
    {
        if (correctGate == GateType.INVENTORY) return true;
        if (currentGate == null) return false;

        return currentGate.type == correctGate;
    }

    // Shake animation played when the wrong gate is in the slot
    public void PlayWrongFeedback()
    {
        // Get the slot's RectTransform
        RectTransform slotTransform = GetComponent<RectTransform>();

        if (slotTransform != null)
        {
            // Reset to the starting position before shaking (avoids odd offsets on repeated clicks)
            slotTransform.DOComplete();

            // Parameters: duration (0.5s), shake strength (15 px), vibration (20), randomness (90 deg)
            slotTransform.DOShakePosition(0.5f, new Vector3(15f, 0f, 0f), 20, 90f)
                         .SetUpdate(true); // IMPORTANT: still runs while Time.timeScale = 0
        }

        // If there is a gate inside the slot, shake it too for extra impact
        if (currentGate != null)
        {
            RectTransform gateTransform = currentGate.GetComponent<RectTransform>();
            if (gateTransform != null)
            {
                gateTransform.DOComplete();

                // A light rotation shake on the gate to make it look unstable
                gateTransform.DOShakeRotation(0.5f, new Vector3(0f, 0f, 10f), 20, 90f)
                             .SetUpdate(true);

                // Optional: briefly flash it red (if it has an Image)
                UnityEngine.UI.Image gateImage = currentGate.GetComponent<UnityEngine.UI.Image>();
                if (gateImage != null)
                {
                    Color originalColor = gateImage.color;
                    gateImage.DOColor(Color.red, 0.15f).SetLoops(2, LoopType.Yoyo).SetUpdate(true);
                }
            }
        }
    }
}
