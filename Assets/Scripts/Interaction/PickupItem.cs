using UnityEngine;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // ONE script for every pick-up-able object: the key, CPU, RAM, GPU, etc.
    // Put it on the 3D object, set the name + icon, layer = Interactable, add a Collider.
    // Press E -> it goes into the backpack (inventory) and disappears.

    public class PickupItem : MonoBehaviour, IInteractable
    {
        [Header("What is this item?")]
        [Tooltip("Unique name, e.g. 'Key', 'CPU', 'RAM', 'GPU'. Other scripts check this name.")]
        [SerializeField] private string itemName = "Key";
        [Tooltip("Picture shown in the backpack.")]
        [SerializeField] private Sprite icon;

        [Header("Optional line spoken on pickup")]
        [TextArea]
        [SerializeField] private string pickupLine = "A key! This might open something.";
        [SerializeField] private string speaker = "You";

        public string ItemName => itemName;

        public string GetPrompt() => $"[E] Take {itemName}";

        public void Interact()
        {
            // Add to the backpack
            InventorySystem.Instance?.AddItem(new InventoryItem
            {
                itemName = itemName,
                icon     = icon
            });

            SoundManager.Instance?.PlayPickup();

            if (!string.IsNullOrEmpty(pickupLine))
                ClassroomSceneFlow.Instance?.ShowDialogue(speaker, pickupLine,
                    autoClose: true, autoCloseDelay: 3f);

            // Shrink away then remove
            transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack)
                .OnComplete(() => gameObject.SetActive(false));
        }
    }
}
