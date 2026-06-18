using UnityEngine;
using UnityEngine.Events;

namespace SYSTEMESCAPE
{
    public class BinaryPuzzleManager : MonoBehaviour
    {
        public static BinaryPuzzleManager Instance { get; private set; }

        [Header("Puzzle Config")]
        [SerializeField] private int targetValue = -1; // -1 = randomise on Start

        [Header("Switch Manager — drag the 8 switches LEFT to RIGHT")]
        [Tooltip("Element 0 = leftmost (value 128, bit 7). " +
                 "Element 7 = rightmost (value 1, bit 0). " +
                 "Bit numbers are assigned automatically by their order here, " +
                 "so the switches can have any names.")]
        [SerializeField] private SwitchInteractable[] switchesLeftToRight;

        public int TargetValue { get; private set; }
        public int CurrentValue { get; private set; }
        public bool IsSolved { get; private set; }

        // The 8 switches + confirm stay LOCKED until ARIA hands over the door code.
        public bool PuzzleUnlocked { get; private set; }

        // Fired when the player correctly submits the answer
        public UnityEvent OnPuzzleSolved;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        private void Start()
        {
            TargetValue = (targetValue < 0 || targetValue > 255)
                ? Random.Range(1, 256)
                : targetValue;

            CurrentValue = 0;
            IsSolved = false;

            AssignBitNumbers();

            Debug.Log($"[BinaryPuzzle] Target = {TargetValue} ({System.Convert.ToString(TargetValue, 2).PadLeft(8, '0')})");
        }

        // Auto-assigns bit numbers from the ordered list: leftmost = bit 7, rightmost = bit 0.
        private void AssignBitNumbers()
        {
            if (switchesLeftToRight == null || switchesLeftToRight.Length == 0) return;

            int n = switchesLeftToRight.Length;
            for (int i = 0; i < n; i++)
            {
                if (switchesLeftToRight[i] == null) continue;
                // i = 0 (leftmost) should be the highest bit
                switchesLeftToRight[i].SetBitIndex(n - 1 - i);
            }

            if (n != 8)
                Debug.LogWarning($"[BinaryPuzzle] Expected 8 switches but got {n}. " +
                    "The door code is an 8-bit number — you need exactly 8.");
        }

        // DEBUG ONLY — instantly solves the puzzle and fires the solved event.
        public void DebugForceSolve()
        {
            if (IsSolved) return;
            PuzzleUnlocked = true;
            CurrentValue = TargetValue;
            IsSolved = true;
            OnPuzzleSolved?.Invoke();
        }

        // ARIA calls this when she gives the door code — opens the puzzle for input.
        public void UnlockPuzzle()
        {
            PuzzleUnlocked = true;
            ShowTeachingUI();
        }

        // Called by SwitchInteractable; bitIndex 0 = LSB (rightmost)
        // Returns true if the toggle was accepted (puzzle unlocked).
        public bool ToggleBit(int bitIndex)
        {
            if (IsSolved || !PuzzleUnlocked) return false;
            CurrentValue ^= (1 << bitIndex);
            BinaryTeachingUI.Instance?.Refresh();
            return true;
        }

        public void ShowTeachingUI() => BinaryTeachingUI.Instance?.ShowPanel();

        public bool IsBitOn(int bitIndex) => (CurrentValue & (1 << bitIndex)) != 0;

        // Called by ConfirmButton
        public void TrySubmit()
        {
            if (IsSolved) return;
            if (!PuzzleUnlocked)
            {
                ClassroomSceneFlow.Instance?.ShowDialogue("ARIA",
                    "Hold on — talk to me first. I'll give you the door code and " +
                    "show you how to enter it.", autoClose: true, autoCloseDelay: 3f);
                return;
            }

            if (CurrentValue == TargetValue)
            {
                IsSolved = true;
                SoundManager.Instance?.PlaySolve();
                OnPuzzleSolved?.Invoke();
            }
            else
            {
                SoundManager.Instance?.PlayWrong();
                ClassroomSceneFlow.Instance?.ShowWrongAnswerFeedback();
            }
        }
    }
}
