using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RotateOnClick : MonoBehaviour, IPointerClickHandler
{
    private PuzzleManager2x2 manager;

    void Awake()
    {
        manager = GetComponentInParent<PuzzleManager2x2>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Rotate 90 degrees clockwise
        transform.Rotate(0, 0, -90f);
        
        if (manager != null)
        {
            manager.UpdatePuzzleState();
        }
    }

    public float GetCurrentZ()
    {
        float z = Mathf.Round(transform.localEulerAngles.z);
        if (z < 0) z += 360f;
        return z % 360f;
    }
}