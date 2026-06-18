using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // Simple inventory — stores item names + icons, shows in a UI panel.
    // Press TAB to open/close. Requires a backpack to be equipped first.
    //
    // UI SETUP:
    //   Canvas -> InventoryPanel (CanvasGroup)
    //     --- ItemGrid (GridLayoutGroup)
    //         --- ItemSlot prefab (Image + child TMP text for item name)

    public class InventorySystem : MonoBehaviour
    {
        public static InventorySystem Instance { get; private set; }

        [Header("UI")]
        [SerializeField] private CanvasGroup inventoryPanel;
        [SerializeField] private Transform   itemGrid;
        [SerializeField] private GameObject  itemSlotPrefab;  // prefab: Image + TMP label

        [Header("Hint")]
        [SerializeField] private TextMeshProUGUI tabHint;      // small "[TAB] Inventory" text

        private readonly List<InventoryItem> _items = new();
        private bool _isOpen   = false;
        private bool _unlocked = false;

        // So the pause menu knows the bag is open (and vice versa)
        public static bool IsOpen { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        private void Start()
        {
            HidePanel(instant: true);

            // If player already had backpack from a loaded save, unlock straight away
            if (GameManager.Instance != null && GameManager.Instance.HasBackpack)
                UnlockInventory();

            // Show tab hint only when unlocked
            if (tabHint != null) tabHint.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!_unlocked) return;
            // Don't open the bag while the pause menu or a minigame is up
            if (PauseMenuManager.IsPaused || LogicGateTerminal.MinigameIsOpen
                || MinigameLauncher.AnyOpen) return;
            if (UnityEngine.InputSystem.Keyboard.current == null) return;

            var kb = UnityEngine.InputSystem.Keyboard.current;
            // TAB toggles; ESC closes the bag if it's open
            if (kb.tabKey.wasPressedThisFrame) Toggle();
            else if (_isOpen && kb.escapeKey.wasPressedThisFrame) Toggle();
        }

        // -- Public API --------------------------------------------------------

        public void UnlockInventory()
        {
            _unlocked = true;
            if (tabHint != null)
            {
                tabHint.gameObject.SetActive(true);
                tabHint.text = "[TAB] Backpack";
            }
        }

        public void AddItem(InventoryItem item)
        {
            // Having any item means the bag is usable — make sure TAB works
            if (!_unlocked) UnlockInventory();

            _items.Add(item);
            RefreshUI();
            SoundManager.Instance?.PlayPickup();

            ClassroomSceneFlow.Instance?.ShowDialogue(
                "You",
                $"Got it: {item.itemName}",
                autoClose: true, autoCloseDelay: 2f
            );
        }

        public bool HasItem(string itemName) =>
            _items.Exists(i => i.itemName == itemName);

        // Removes one item by name (e.g. when the key is used in a lock). Returns true if removed.
        public bool RemoveItem(string itemName)
        {
            int idx = _items.FindIndex(i => i.itemName == itemName);
            if (idx < 0) return false;
            _items.RemoveAt(idx);
            RefreshUI();
            return true;
        }

        // -- UI ----------------------------------------------------------------

        private void Toggle()
        {
            if (_isOpen) HidePanel(); else ShowPanel();
            _isOpen = !_isOpen;
            IsOpen  = _isOpen;

            // Free the cursor + stop the player while the bag is open
            if (_isOpen)
            {
                FPSController.Instance?.DisableMovement();
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible   = true;
            }
            else
            {
                FPSController.Instance?.EnableMovement();
            }
        }

        private void RefreshUI()
        {
            if (itemGrid == null || itemSlotPrefab == null) return;

            foreach (Transform child in itemGrid) Destroy(child.gameObject);

            foreach (var item in _items)
            {
                GameObject slot = Instantiate(itemSlotPrefab, itemGrid);

                // Prefer a child Image named "Icon" for the picture; fall back to slot image.
                Image iconImg = null;
                foreach (var img in slot.GetComponentsInChildren<Image>())
                {
                    if (img.gameObject.name == "Icon") { iconImg = img; break; }
                    if (iconImg == null) iconImg = img; // fallback: first image found
                }

                if (iconImg != null)
                {
                    if (item.icon != null)
                    {
                        iconImg.sprite  = item.icon;
                        iconImg.enabled = true;
                        iconImg.color   = Color.white;            // show the sprite fully
                    }
                    else
                    {
                        iconImg.color = new Color(0.3f, 0.3f, 0.35f); // grey box if no icon
                    }
                }

                // The name is a small caption under the icon
                var label = slot.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = item.itemName;
            }
        }

        private void ShowPanel()
        {
            if (inventoryPanel == null) return;
            inventoryPanel.gameObject.SetActive(true);
            inventoryPanel.DOFade(1f, 0.2f);
            inventoryPanel.interactable   = true;
            inventoryPanel.blocksRaycasts = true;
        }

        private void HidePanel(bool instant = false)
        {
            if (inventoryPanel == null) return;
            inventoryPanel.interactable   = false;
            inventoryPanel.blocksRaycasts = false;
            if (instant) { inventoryPanel.alpha = 0f; inventoryPanel.gameObject.SetActive(false); }
            else inventoryPanel.DOFade(0f, 0.2f).OnComplete(() => inventoryPanel.gameObject.SetActive(false));
        }
    }

    [System.Serializable]
    public class InventoryItem
    {
        public string itemName;
        public Sprite icon;
    }
}
