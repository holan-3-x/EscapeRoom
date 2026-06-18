using UnityEngine;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // A swinging door. Two ways to open it:
    //  1) Event-driven: wire Open() to BinaryPuzzleManager.OnPuzzleSolved (auto-opens).
    //  2) Interactable: tick "Player Can Open" so the player presses E to open it.
    //
    // For the computer-lab left/right doors: tick Player Can Open, and (optionally)
    // tick Require Puzzle Done so they only open after the binary puzzle is solved.

    public class DoorController : MonoBehaviour, IInteractable
    {
        public enum RotateAxis { X, Y, Z }

        [Header("Door Motion")]
        [SerializeField] private RotateAxis axis = RotateAxis.Y;
        [SerializeField] private float openAngle    = 90f;   // use -90 to swing the other way
        [SerializeField] private float openDuration = 1.2f;
        [SerializeField] private Ease  openEase     = Ease.OutBack;

        [Header("Player interaction")]
        [Tooltip("If true, the player can press E to open this door.")]
        [SerializeField] private bool playerCanOpen = false;
        [Tooltip("If true, E only works after the binary puzzle is solved.")]
        [SerializeField] private bool requirePuzzleDone = false;
        [Tooltip("If set (e.g. 'Key'), the player must have this item in the backpack.")]
        [SerializeField] private string requireItemNamed = "";
        [TextArea]
        [SerializeField] private string lockedLine =
            "It won't open yet — I should finish in here first.";
        [TextArea]
        [SerializeField] private string needKeyLine =
            "It's locked. I need to find a key for this one.";

        [Header("Optional")]
        [SerializeField] private AudioSource doorAudio;

        private bool isOpen = false;

        // -- IInteractable (press E) ---------------------------------------------

        public string GetPrompt()
        {
            if (!playerCanOpen || isOpen) return "";
            return "[E] Open door";
        }

        public void Interact()
        {
            if (!playerCanOpen || isOpen) return;

            if (requirePuzzleDone && !PuzzleDone())
            {
                ClassroomSceneFlow.Instance?.ShowDialogue("You", lockedLine,
                    autoClose: true, autoCloseDelay: 3f);
                return;
            }

            // Needs a key/item from the backpack?
            if (!string.IsNullOrEmpty(requireItemNamed))
            {
                bool hasItem = InventorySystem.Instance != null
                    && InventorySystem.Instance.HasItem(requireItemNamed);
                if (!hasItem)
                {
                    ClassroomSceneFlow.Instance?.ShowDialogue("You", needKeyLine,
                        autoClose: true, autoCloseDelay: 3f);
                    return;
                }
            }

            Open();
        }

        private bool PuzzleDone()
        {
            return BinaryPuzzleManager.Instance == null
                || BinaryPuzzleManager.Instance.IsSolved;
        }

        // -- Open (also called by OnPuzzleSolved event) --------------------------

        public void Open()
        {
            if (isOpen) return;
            isOpen = true;

            if (doorAudio != null) doorAudio.Play();
            SoundManager.Instance?.PlayDoor();

            Vector3 rot = axis switch
            {
                RotateAxis.X => new Vector3(openAngle, 0, 0),
                RotateAxis.Y => new Vector3(0, openAngle, 0),
                RotateAxis.Z => new Vector3(0, 0, openAngle),
                _            => new Vector3(0, openAngle, 0),
            };

            transform.DOLocalRotate(rot, openDuration, RotateMode.LocalAxisAdd)
                .SetEase(openEase);
        }

        [ContextMenu("Test Open")]
        private void TestOpen() => Open();
    }
}
