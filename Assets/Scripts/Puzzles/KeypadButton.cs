using UnityEngine;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // One key on the cipher keypad. Press E to enter its digit.
    // Set Digit (0-9), or tick Is Clear for the clear/reset key.
    // Put on the button, layer = Interactable, add a Collider.

    public class KeypadButton : MonoBehaviour, IInteractable
    {
        [SerializeField] private PhoneCipherLock lockPanel;
        [SerializeField] private int digit = 0;
        [SerializeField] private bool isClear = false;

        [Header("Press animation (optional)")]
        [SerializeField] private Transform buttonCap;
        [SerializeField] private float pressDepth = 0.01f;

        public string GetPrompt() => isClear ? "[E] Clear" : $"[E] Press {digit}";

        public void Interact()
        {
            if (buttonCap != null)
                buttonCap.DOLocalMoveZ(buttonCap.localPosition.z - pressDepth, 0.07f)
                    .SetLoops(2, LoopType.Yoyo);

            if (lockPanel == null) return;
            if (isClear) lockPanel.ClearInput();
            else         lockPanel.EnterDigit(digit);
        }
    }
}
