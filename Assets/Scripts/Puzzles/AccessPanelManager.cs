using UnityEngine;
using TMPro;

namespace SYSTEMESCAPE
{
    // The computer-lab door access panel. Tracks a 3-step unlock then opens the doors:
    //   1) Insert the Key   (KeySlotInteract)
    //   2) Solve the logic gate puzzle  (friend's puzzle calls MarkLogicSolved())
    //   3) Flip the approve slider  (ApproveSliderInteract)  -> doors open
    //
    // SETUP: put this on the panel object. Assign the two doors + an optional screen text.

    public class AccessPanelManager : MonoBehaviour
    {
        public static AccessPanelManager Instance { get; private set; }

        [Header("Doors to open (left & right)")]
        [SerializeField] private DoorController leftDoor;
        [SerializeField] private DoorController rightDoor;

        [Header("Optional status screen (3D TextMeshPro OR canvas TMP both work)")]
        [SerializeField] private TMP_Text screenText;   // TMP_Text = base type for 3D and UI TMP
        [Tooltip("The lit screen / glow object. Starts OFF, turns ON when the key is inserted.")]
        [SerializeField] private GameObject screenOnVisual;
        [Tooltip("Optional boot sound when the screen powers on.")]
        [SerializeField] private AudioSource screenBootSound;

        [Header("Item name required as the key")]
        [SerializeField] private string keyItemName = "Key";

        public bool KeyInserted { get; private set; }
        public bool LogicSolved { get; private set; }
        public bool Approved    { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        private void Start()
        {
            // Screen starts dark until the key powers it on
            if (screenOnVisual != null) screenOnVisual.SetActive(false);
            UpdateScreen();
        }

        // Powers on the panel screen (called when the key is inserted)
        public void PowerOnScreen()
        {
            if (screenOnVisual != null) screenOnVisual.SetActive(true);
            if (screenBootSound != null) screenBootSound.Play();
            else SoundManager.Instance?.PlayBoot();
        }

        // -- Step 1: key ---------------------------------------------------------

        public string KeyName => keyItemName;

        public bool TryInsertKey()
        {
            if (KeyInserted) return true;

            if (InventorySystem.Instance != null &&
                InventorySystem.Instance.RemoveItem(keyItemName))
            {
                KeyInserted = true;
                SoundManager.Instance?.PlayClick();
                PowerOnScreen();        // screen lights up
                UpdateScreen();
                ClassroomSceneFlow.Instance?.ShowDialogue("ARIA",
                    "Key accepted — the panel's powering up. Now the logic-gate lock needs solving. " +
                    "Work the gates until the output reads 1.",
                    autoClose: true, autoCloseDelay: 4.5f);
                return true;
            }

            // No key yet — hint to go search
            ClassroomSceneFlow.Instance?.ShowDialogue("ARIA",
                "There's a keyhole here, but you don't have the key. " +
                "Try searching the hallway — check the drawers, the cabinets, even the trash bins.",
                autoClose: true, autoCloseDelay: 4.5f);
            return false;
        }

        // -- Step 2: logic gate (called by the friend's puzzle, or debug) ---------

        public void MarkLogicSolved()
        {
            if (LogicSolved) return;
            LogicSolved = true;
            SoundManager.Instance?.PlaySolve();
            UpdateScreen();
            ClassroomSceneFlow.Instance?.ShowDialogue("ARIA",
                "Logic gate solved! One step left — flip the approve switch to release the doors.",
                autoClose: true, autoCloseDelay: 4f);
        }

        // -- Step 3: approve slider -----------------------------------------------

        // Returns true if approval succeeded (so the slider knows to stay up or spring back).
        public bool TryApprove()
        {
            if (Approved) return true;

            if (!KeyInserted)
            {
                ClassroomSceneFlow.Instance?.ShowDialogue("ARIA",
                    "The panel's dead — you need to insert the key first.",
                    autoClose: true, autoCloseDelay: 3.5f);
                return false;
            }
            if (!LogicSolved)
            {
                ClassroomSceneFlow.Instance?.ShowDialogue("ARIA",
                    "The lock won't approve yet — solve the logic-gate puzzle first.",
                    autoClose: true, autoCloseDelay: 3.5f);
                return false;
            }

            Approved = true;
            SoundManager.Instance?.PlayConfirm();
            UpdateScreen();
            OpenDoors();
            return true;
        }

        private void OpenDoors()
        {
            if (leftDoor  != null) leftDoor.Open();
            if (rightDoor != null) rightDoor.Open();

            ClassroomSceneFlow.Instance?.ShowDialogue("ARIA",
                "Access granted. Doors opening — welcome to the computer lab.",
                autoClose: true, autoCloseDelay: 4f);
        }

        // -- Screen ---------------------------------------------------------------

        private void UpdateScreen()
        {
            if (screenText == null) return;
            string k = KeyInserted ? "<color=#3fd>[OK]</color>" : "[  ]";
            string l = LogicSolved ? "<color=#3fd>[OK]</color>" : "[  ]";
            string a = Approved    ? "<color=#3fd>[OK]</color>" : "[  ]";
            screenText.text =
                $"LAB ACCESS\n\n{k} 1. Key\n{l} 2. Logic Gate\n{a} 3. Approve";
        }
    }
}
