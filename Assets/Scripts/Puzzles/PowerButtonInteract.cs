using UnityEngine;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // The PC power button. Press E to boot the PC — only works after it's fully assembled.
    // Put on the button object, layer = Interactable, add a Collider.

    public class PowerButtonInteract : MonoBehaviour, IInteractable
    {
        [SerializeField] private PCBuildStation pc;

        [Header("Button press animation (optional)")]
        [SerializeField] private Transform buttonCap;
        [SerializeField] private float pressDepth = 0.01f;

        [Header("Power LED that lights up (optional)")]
        [SerializeField] private GameObject powerLed;

        public string GetPrompt()
        {
            if (pc != null && pc.PoweredOn) return "";
            return "[E] Power button";
        }

        public void Interact()
        {
            if (pc == null || pc.PoweredOn) return;

            // Press animation regardless
            if (buttonCap != null)
                buttonCap.DOLocalMoveZ(buttonCap.localPosition.z - pressDepth, 0.08f)
                    .SetLoops(2, LoopType.Yoyo);

            if (!pc.ReadyToPowerOn)
            {
                ClassroomSceneFlow.Instance?.ShowDialogue("ARIA",
                    "Nothing happens — the PC isn't fully built yet. Finish installing the parts first.",
                    autoClose: true, autoCloseDelay: 3.5f);
                return;
            }

            if (powerLed != null) powerLed.SetActive(true);
            pc.PowerOn();
        }
    }
}
