namespace BlockGame {
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    
    public SequenceSlot CurrentSlot => currentSlot;

    [Header("Slot attuale di questo blocco")]
    [SerializeField] private SequenceSlot currentSlot;

    [Tooltip("Distanza massima per agganciarsi a uno slot")]
    public float snapDistance = 80f; 

    // Variabile di supporto per capire se questo specifico blocco è un clone volante
    private bool isClone = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void Start()
    {
        if (currentSlot == null)
        {
            currentSlot = GetComponentInParent<SequenceSlot>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Se il blocco parte dall'inventario (currentSlot è null) e NON è già un clone
        if (currentSlot == null && !isClone)
        {
            // 1. Memorizziamo la dimensione geometrica esatta di QUESTO blocco nell'inventario
            Vector2 originalSize = rectTransform.sizeDelta;
            
            // 2. Troviamo il Canvas radice (UI_Panel o Canvas principale)
            Transform canvasRoot = transform.parent.parent; 

            // 3. Cloniamo noi stessi direttamente sotto il Canvas Radice 
            // In questo modo il clone non tocca MAI l'inventario e la lista a sinistra non si muove di un millimetro!
            GameObject cloneGO = Instantiate(gameObject, canvasRoot);
            cloneGO.name = this.name;

            // 4. Configuriamos lo script del CLONE dicendogli che lui è l'oggetto volante
            DraggableUI cloneDrag = cloneGO.GetComponent<DraggableUI>();
            cloneDrag.isClone = true;

            // 5. Ripristiniamo sul CLONE la dimensione esatta per non fargli perdere la forma
            cloneGO.GetComponent<RectTransform>().sizeDelta = originalSize;

            // 6. Forza il BlockItem del CLONE a copiarsi i dati e aggiornare colore/testo immediatamente
            BlockItem myBlockItem = GetComponent<BlockItem>();
            BlockItem cloneBlockItem = cloneGO.GetComponent<BlockItem>();
            if (myBlockItem != null && cloneBlockItem != null)
            {
                cloneBlockItem.Setup(myBlockItem.data); // Passa i dati dell'asset e aggiorna la UI
            }

            // 7. Posizioniamo il CLONE sul mouse e passiamo il controllo del drag a lui
            cloneGO.GetComponent<RectTransform>().position = eventData.position;
            eventData.pointerDrag = cloneGO;

            // 8. Prepariamo il CanvasGroup del clone per il movimento
            cloneDrag.canvasGroup.blocksRaycasts = false;
            cloneGO.transform.SetParent(GetComponentInParent<Canvas>().transform);
            cloneGO.transform.SetAsLastSibling();

            // L'oggetto originale (questo) si ferma qui: resta fermo, intatto e immobile nell'inventario!
            return;
        }

        // Logica normale se stiamo trascinando un blocco che era già dentro la sequenza
        canvasGroup.blocksRaycasts = false;
        
        if (currentSlot != null)
        {
            currentSlot.RemoveGate();
        }

        transform.SetParent(GetComponentInParent<Canvas>().transform);
        transform.SetAsLastSibling(); 
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position += (Vector3)eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        // Memorizziamo lo slot da cui proveniva QUESTO blocco PRIMA del drop
        SequenceSlot slotOfOrigin = currentSlot;

        SequenceSlot closestSlot = FindClosestSlot();

        if (closestSlot != null)
        {
            float distance = Vector3.Distance(rectTransform.position, closestSlot.transform.position);

            if (distance <= snapDistance)
            {
                isClone = false; 
                closestSlot.PlaceGate(this);
                return;
            }
        }

        // --- SE IL DROP FALLISCE (Il blocco viene rilasciato nel vuoto) ---
        
        // 1. Se il blocco proveniva da uno slot della sequenza, significa che stiamo lasciando un buco definitivo
        if (slotOfOrigin != null)
        {
            // Forziamo lo slot di origine a compattare tutti i blocchi sotto di lui verso l'alto
            slotOfOrigin.CompandSequenceUp();
        }

        // 2. Distruggiamo il blocco volante in sicurezza
        Destroy(gameObject);
    }

    public void SetSlot(SequenceSlot slot)
    {
        currentSlot = slot;
        if (slot != null)
        {
            rectTransform.SetParent(slot.GetComponent<RectTransform>());
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.SetAsLastSibling();
        }
    }

    public void ClearSlot()
    {
        currentSlot = null;
    }

    private SequenceSlot FindClosestSlot()
    {
        SequenceSlot[] slots = FindObjectsByType<SequenceSlot>(FindObjectsSortMode.None);
        SequenceSlot closest = null;
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
}
