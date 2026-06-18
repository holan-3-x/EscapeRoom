using UnityEngine;
using UnityEngine.InputSystem;

namespace SYSTEMESCAPE
{
    // The screen/terminal the player uses to play the logic-gate minigame.
    // Uses the main game's E-key interaction (IInteractable), NOT the friend's trigger.
    //
    // Flow:
    //   - Locked until the KEY is inserted in the access panel (requireKeyFirst).
    //   - First E press: ARIA introduces logic gates.
    //   - Next E press: opens the minigame (the friend's ProgressionManager).
    //   - While open: ESC closes it; solving all levels closes it and fires MarkLogicSolved.
    //
    // SETUP: put this on the terminal/screen object, layer = Interactable, add a Collider.
    //   Minigame = the ProgressionManager object (the friend's minigame root).
    //   Panel    = your AccessPanelManager (so it can require the key first).

    public class LogicGateTerminal : MonoBehaviour, IInteractable
    {
        [Header("References")]
        [Tooltip("The friend's ProgressionManager (root of the gate minigame).")]
        [SerializeField] private MinigameBase minigame;
        [Tooltip("Your access panel — used to require the key first.")]
        [SerializeField] private AccessPanelManager panel;
        [SerializeField] private bool requireKeyFirst = true;

        // True while the gate minigame is open, so the pause menu ignores ESC.
        public static bool MinigameIsOpen { get; private set; }

        // Clears the static flag when a new gameplay scene loads (builds keep statics alive)
        public static void ForceReset() { MinigameIsOpen = false; }

        private bool _introDone = false;
        private bool _isOpen = false;

        public string GetPrompt()
        {
            if (_isOpen) return "";
            if (minigame != null && minigame.isCompleted) return "";
            return "[E] Use the logic terminal";
        }

        public void Interact()
        {
            if (_isOpen || minigame == null || minigame.isCompleted) return;

            // Gate behind the key
            if (requireKeyFirst && panel != null && !panel.KeyInserted)
            {
                ClassroomSceneFlow.Instance?.ShowDialogue("ARIA",
                    "This terminal's locked. Insert the key into the panel first, then I'll " +
                    "walk you through the logic puzzle.",
                    autoClose: true, autoCloseDelay: 3.5f);
                return;
            }

            // First time: ARIA introduces logic gates (play BEFORE opening, while time runs normally)
            if (!_introDone)
            {
                _introDone = true;
                ClassroomSceneFlow.Instance?.ShowDialogue("ARIA",
                    "Time for logic gates. They're the tiny decision-makers inside every computer.\n\n" +
                    "AND outputs 1 only if BOTH inputs are 1.\n" +
                    "OR outputs 1 if AT LEAST ONE input is 1.\n" +
                    "NOT flips the input: 1 becomes 0, 0 becomes 1.\n\n" +
                    "Drag the right gate into each slot to make the circuit work. " +
                    "Press E again when you're ready to begin — and use the info button if you forget.",
                    autoClose: false);
                return;
            }

            OpenMinigame();
        }

        private void OpenMinigame()
        {
            _isOpen = true;
            MinigameIsOpen = true;                        // tell the pause menu to ignore ESC
            FPSController.Instance?.DisableMovement();   // freeze the player while the puzzle is open
            FindObjectOfType<PlayerInteraction>()?.DisableInteraction(); // hide the crosshair/prompt
            minigame.StartMinigame();                    // friend's code: shows canvas, timeScale=0, cursor on
        }

        private void Update()
        {
            if (!_isOpen) return;

            // Player closed it with ESC or Q
            var kb = Keyboard.current;
            if (kb != null && (kb.escapeKey.wasPressedThisFrame || kb.qKey.wasPressedThisFrame))
            {
                minigame.CloseMinigame();
                CloseAndReturnControl();
                return;
            }

            // Player finished all levels (the minigame ends itself)
            if (minigame.isCompleted)
            {
                CloseAndReturnControl();
            }
        }

        private void CloseAndReturnControl()
        {
            _isOpen = false;
            MinigameIsOpen = false;
            Time.timeScale = 1f;
            FPSController.Instance?.EnableMovement();     // also re-locks the cursor
            FindObjectOfType<PlayerInteraction>()?.EnableInteraction(); // crosshair back on
        }
    }
}
