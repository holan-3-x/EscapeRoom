using UnityEngine;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // Place this on the backpack 3D object sitting on a desk/floor.
    // When the player picks it up:
    //   - The 3D pickup disappears
    //   - The worn backpack on the player's BodyMesh becomes visible
    //   - Inventory UI unlocks

    public class BackpackPickup : MonoBehaviour, IInteractable
    {
        [Header("References")]
        [SerializeField] private GameObject wornBackpack;  // the backpack child on the player BodyMesh

        public string GetPrompt() => "[E] Pick up backpack";

        public void Interact()
        {
            // GameManager.Instance auto-creates itself, so it is never null now
            if (GameManager.Instance.HasBackpack) return;

            GameManager.Instance.HasBackpack = true;

            // Show the backpack on the player body
            if (wornBackpack != null) wornBackpack.SetActive(true);

            // Notify inventory system (unlocks TAB)
            InventorySystem.Instance?.UnlockInventory();

            SoundManager.Instance?.PlayPickup();

            // Show dialogue
            ClassroomSceneFlow.Instance?.ShowDialogue(
                "You",
                "My backpack — good, I've still got it. Press TAB to check what's inside.",
                autoClose: true, autoCloseDelay: 3.5f
            );

            // Animate pickup then destroy
            transform.DOScale(Vector3.zero, 0.3f).OnComplete(() => Destroy(gameObject));

            SaveSystem.Save();
        }
    }
}
