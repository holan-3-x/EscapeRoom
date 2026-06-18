using UnityEngine;
using UnityEngine.Events;
using TMPro;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // Final-door cipher lock. The player reads a clue (a poster) and uses the old
    // phone-keypad letter mapping to turn a word into a 4-digit PIN.
    //
    //   2=ABC  3=DEF  4=GHI  5=JKL  6=MNO  7=PQRS  8=TUV  9=WXYZ
    //
    // Example: the word "CODE" -> C=2, O=6, D=3, E=3 -> PIN 2633.
    //
    // SETUP: put this on the keypad panel. Set Target Code. Assign a display TMP.
    //   Add KeypadButton scripts to each number button (digit 0-9) + a Clear button.
    //   Wire On Unlock to open the final door.

    public class PhoneCipherLock : MonoBehaviour
    {
        public static PhoneCipherLock Instance { get; private set; }

        [Header("The answer (4 digits)")]
        [SerializeField] private string targetCode = "2633";

        [Header("The word the clue points to (for ARIA's hint)")]
        [SerializeField] private string clueWord = "CODE";

        [Header("Display (3D OR UI TMP — both work)")]
        [SerializeField] private TMP_Text display;   // TMP_Text = base type for 3D and UI TMP

        [Header("Fires when the correct PIN is entered")]
        public UnityEvent OnUnlock;

        private string _entered = "";
        private bool _unlocked = false;
        private bool _introShown = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        private void Start() => UpdateDisplay();

        // Called by each KeypadButton
        public void EnterDigit(int digit)
        {
            if (_unlocked) return;

            // First touch: ARIA explains the cipher before any digit is accepted
            if (!_introShown)
            {
                _introShown = true;
                ExplainCipher();
                return;
            }

            if (_entered.Length >= targetCode.Length) return;

            _entered += digit.ToString();
            SoundManager.Instance?.PlayClick();
            UpdateDisplay();

            if (_entered.Length == targetCode.Length)
                CheckCode();
        }

        public void ClearInput()
        {
            if (_unlocked) return;
            _entered = "";
            SoundManager.Instance?.PlayClick();
            UpdateDisplay();
        }

        private void CheckCode()
        {
            if (_entered == targetCode)
            {
                _unlocked = true;
                SoundManager.Instance?.PlaySolve();
                if (display != null) display.color = Color.green;
                ClassroomSceneFlow.Instance?.ShowDialogue("ARIA",
                    "That's it — the lock just released. Let's get out of here.",
                    autoClose: true, autoCloseDelay: 3.5f);
                OnUnlock?.Invoke();
            }
            else
            {
                SoundManager.Instance?.PlayWrong();
                if (display != null)
                    display.transform.DOShakePosition(0.4f, 12f, 18, 90f);
                ClassroomSceneFlow.Instance?.ShowDialogue("ARIA",
                    "Wrong code. Re-read the poster and convert the letters to phone-keypad numbers.",
                    autoClose: true, autoCloseDelay: 4f);
                _entered = "";
                DOVirtual.DelayedCall(0.5f, UpdateDisplay);
            }
        }

        // ARIA explains the cipher (call from the poster, or the lock's first use)
        public void ExplainCipher()
        {
            ClassroomSceneFlow.Instance?.ShowDialogue("ARIA",
                "A keypad lock — classic. And look, it's the old telephone layout. " +
                "Back then, each number key held a few letters:\n\n" +
                "2 = ABC   3 = DEF   4 = GHI   5 = JKL\n" +
                "6 = MNO   7 = PQRS   8 = TUV   9 = WXYZ\n\n" +
                $"The poster gives you a word. Find the key for each letter, type the numbers, " +
                $"and that's the PIN. It's only {targetCode.Length} letters long — you've got this.",
                autoClose: true, autoCloseDelay: 7f);
        }

        private void UpdateDisplay()
        {
            if (display == null) return;
            string shown = _entered.PadRight(targetCode.Length, '_');
            display.text = string.Join(" ", shown.ToCharArray());
        }
    }
}
