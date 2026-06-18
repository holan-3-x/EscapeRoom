using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    
    // Allow GateSlot to see which slot this gate currently belongs to
    public GateSlot CurrentSlot => currentSlot;

    [Header("Current slot of this gate")]
    [SerializeField] private GateSlot currentSlot;

    public float snapDistance = 100f; 

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void Start()
    {
        // If not assigned in Inspector, check if it's inside a slot at start
        if (currentSlot == null)
        {
            currentSlot = GetComponentInParent<GateSlot>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = false;
        
        // We lift the gate up to the root canvas layer during the drag 
        // so it renders on top of all other UI slots while moving
        transform.SetAsLastSibling(); 
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position += (Vector3)eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        GateSlot closestSlot = FindClosestSlot();

        if (closestSlot != null)
        {
            float distance = Vector3.Distance(rectTransform.position, closestSlot.transform.position);

            if (distance <= snapDistance)
            {
                // This triggers the swap logic inside GateSlot
                closestSlot.PlaceGate(this);
                return;
            }
        }

        // If dropped outside the snap distance, return safely to its current slot
        if (currentSlot != null)
        {
            SetSlot(currentSlot);
        }
    }

    // Sets the slot reference and snaps the UI transform directly into the slot layout
    public void SetSlot(GateSlot slot)
    {
        currentSlot = slot;
        rectTransform.SetParent(slot.GetComponent<RectTransform>());
        rectTransform.localPosition = Vector3.zero;
        rectTransform.localScale = Vector3.one;
        rectTransform.SetAsLastSibling();
    }

    GateSlot FindClosestSlot()
    {
        GateSlot[] slots = FindObjectsByType<GateSlot>(FindObjectsSortMode.None);
        GateSlot closest = null;
        float minDistance = Mathf.Infinity;

        foreach (var slot in slots)
        {
            float dist = Vector3.Distance(rectTransform.position, slot.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = slot;
            }
        }
        return closest;
    }
}