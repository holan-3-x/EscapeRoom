namespace BlockGame {
using UnityEngine;

public class SequenceSlot : MonoBehaviour
{
    [Header("Stato attuale")]
    public DraggableUI currentGate;          

    public bool IsOccupied => currentGate != null;

    public void PlaceGate(DraggableUI draggable)
    {
        SequenceSlot sourceSlot = draggable.CurrentSlot; 

        if (!IsOccupied)
        {
            SequenceSlot highestFreeSlot = FindHighestFreeSlot();
            
            if (highestFreeSlot != null && highestFreeSlot != this)
            {
                highestFreeSlot.PlaceGate(draggable);
                return;
            }
        }

        if (IsOccupied)
        {
            DraggableUI oldGate = currentGate;

            if (sourceSlot != null)
            {
                sourceSlot.currentGate = oldGate;
                oldGate.SetSlot(sourceSlot);
            }
            else
            {
                Destroy(oldGate.gameObject);
            }
        }
        else
        {
            if (sourceSlot != null)
            {
                sourceSlot.currentGate = null;
            }
        }

        currentGate = draggable;
        draggable.SetSlot(this);
    }

    public void RemoveGate()
    {
        currentGate = null;
    }

    public void CompandSequenceUp()
    {
        Transform container = transform.parent;
        if (container == null) return;

        int myIndex = transform.GetSiblingIndex();

        for (int i = myIndex + 1; i < container.childCount; i++)
        {
            Transform child = container.GetChild(i);
            SequenceSlot nextSlot = child.GetComponent<SequenceSlot>();

            if (nextSlot != null && nextSlot.IsOccupied)
            {
                Transform previousChild = container.GetChild(i - 1);
                SequenceSlot previousSlot = previousChild.GetComponent<SequenceSlot>();

                if (previousSlot != null)
                {
                    DraggableUI gateToMove = nextSlot.currentGate;
                    nextSlot.currentGate = null;

                    previousSlot.currentGate = gateToMove;
                    gateToMove.SetSlot(previousSlot); 
                }
            }
        }
    }

    private SequenceSlot FindHighestFreeSlot()
    {
        Transform container = transform.parent;
        if (container == null) return null;

        for (int i = 0; i < container.childCount; i++)
        {
            Transform child = container.GetChild(i);
            SequenceSlot slot = child.GetComponent<SequenceSlot>();
            
            if (slot != null && !slot.IsOccupied)
            {
                return slot;
            }
        }

        return null;
    }
}
}
