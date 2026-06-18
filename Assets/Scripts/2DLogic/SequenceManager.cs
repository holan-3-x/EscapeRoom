namespace BlockGame {
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SequenceManager : MonoBehaviour
{
    [Header("Riferimenti ai Componenti")]
    [SerializeField] private CodeExecutor codeExecutor;
    [SerializeField] private Button startButton;

    [Header("Contenitore degli Slot")]
    [Tooltip("Assegna qui il GameObject 'SlotsContainer' che contiene tutti i singoli Slot.")]
    [SerializeField] private Transform slotsContainer;

    private List<SequenceSlot> _sequenceSlots = new List<SequenceSlot>();

    private void Awake()
    {
        if (codeExecutor == null)
        {
            codeExecutor = GetComponent<CodeExecutor>();
        }

        if (slotsContainer != null)
        {
            RefreshSlotsList();
        }
    }

    public void RefreshSlotsList()
    {
        _sequenceSlots.Clear();
        foreach (Transform child in slotsContainer)
        {
            SequenceSlot slot = child.GetComponent<SequenceSlot>();
            if (slot != null)
            {
                _sequenceSlots.Add(slot);
            }
        }
    }

    /// <summary>
    /// Metodo Pubblico da agganciare all'On Click() del bottone Execute nell'Inspector!
    /// </summary>
    public void OnExecuteButtonPress()
    {
        List<BlockData> blocksToExecute = new List<BlockData>();

        foreach (SequenceSlot slot in _sequenceSlots)
        {
            if (slot.IsOccupied && slot.currentGate != null)
            {
                BlockItem blockItem = slot.currentGate.GetComponent<BlockItem>();
                if (blockItem != null && blockItem.data != null)
                {
                    blocksToExecute.Add(blockItem.data);
                }
            }
        }

        if (blocksToExecute.Count > 0)
        {
            if (codeExecutor != null)
            {
                codeExecutor.Execute(blocksToExecute);
            }
            else
            {
                Debug.LogError("[SequenceManager] Manca il riferimento a CodeExecutor!");
            }
        }
        else
        {
            Debug.LogWarning("[SequenceManager] Nessun blocco inserito nella sequenza. Inserisci almeno un'istruzione prima di premere START.");
        }
    }

    public void SetExecuteInteractable(bool isInteractable)
    {
        if (startButton != null)
        {
            startButton.interactable = isInteractable;
        }
    }

    public void ClearSequenceArea()
    {
        if (slotsContainer != null)
        {
            foreach (Transform child in slotsContainer.transform)
            {
                if (child.childCount > 0)
                {
                    foreach (Transform grandChild in child)
                    {
                        Destroy(grandChild.gameObject);
                    }
                }
                
                // Libera lo slot a livello logico azzerando il riferimento al blocco
                SequenceSlot slot = child.GetComponent<SequenceSlot>();
                if (slot != null)
                {
                    slot.RemoveGate();
                }
            }
        }

        Debug.Log("[SequenceManager] Grafica e riferimenti degli slot svuotati.");
    }
}
}
