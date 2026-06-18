using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RotateOnClick_Lvl2 : MonoBehaviour, IPointerClickHandler
{
    [Header("Custom Pipe Sprites")]
    public Sprite unlitSprite;
    public Sprite litSprite;

    private PuzzleManager_Lvl2 manager;

    void Awake()
    {
        manager = GetComponentInParent<PuzzleManager_Lvl2>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        transform.Rotate(0, 0, -90f);
        
        if (manager != null)
        {
            manager.RegisterClick();
        }
    }

    public float GetCurrentZ()
    {
        // Clean out any floating point precision errors from Unity transform updates
        float z = transform.localEulerAngles.z;
        
        // Wrap negative rotations cleanly into a positive 0-360 range
        while (z < 0f) z += 360f;
        return z % 360f;
    }
}