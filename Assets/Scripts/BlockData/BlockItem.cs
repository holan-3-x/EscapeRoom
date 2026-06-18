using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Componente da mettere sul prefab di ogni blocco, insieme a DraggableUI.
/// Porta il BlockData e aggiorna la visuale.
/// 
/// PREFAB STRUTTURA CONSIGLIATA:
///   BlockPrefab (DraggableUI + CanvasGroup + BlockItem)
///     -- Background (Image)
///     -- Icon (Image, opzionale)
///     -- Label (TextMeshProUGUI)
/// </summary>
public class BlockItem : MonoBehaviour
{
    [Header("Dati (assegna il BlockData asset)")]
    public BlockData data;

    [Header("Riferimenti UI interni")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private Image iconImage;

    // ------------------------------------------------------------------

    private void Awake()
    {
        if (data != null) Refresh();
    }

    /// <summary>Chiama questo dopo aver assegnato data a runtime (es. dall'inventario).</summary>
    public void Setup(BlockData blockData)
    {
        data = blockData;
        Refresh();
    }

    private void Refresh()
    {
        if (backgroundImage) backgroundImage.color = data.blockColor;
        if (labelText)       labelText.text        = data.label;
        if (iconImage && data.blockIcon)
        {
            iconImage.sprite  = data.blockIcon;
            iconImage.enabled = true;
        }
    }
}
