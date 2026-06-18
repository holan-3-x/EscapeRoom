using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using TMPro;

public class PuzzleManager : MonoBehaviour
{
    public GateSlot[] slots;

    public TextMeshProUGUI successText;

    // References needed for the info button
    [Header("Level Instructions")]
    [TextArea(3, 10)] // Makes the text area comfortable in the Inspector
    public string levelInstructions = "Write the instructions for this level here...";
    private GameObject infoPanelGlobal;
    private TextMeshProUGUI infoTextGlobal;

    [Header("Outside-World Interactions")]
    public UnityEvent onPuzzleSuccessReward;

    void Awake()
    {
        // AUTO-CONFIGURATION LOGIC:
        // Find all GateSlot components that live under the same parent object
        if (transform.parent != null)
        {
            // GetComponentInChildren searches the object and all its descendants.
            // Used on the parent, it finds all the slots of the current level.
            slots = transform.parent.GetComponentsInChildren<GateSlot>();
            successText = transform.parent.GetComponentInChildren<TextMeshProUGUI>(true);

            // FIND THE INFO PANEL ON THE MAIN CANVAS
            // We look for the panel object named "GlobalInfoPanel" under GatesMinigameCanvas
            Transform canvasTransform = transform.parent.parent; // Go up to GatesMinigameCanvas
            if (canvasTransform != null)
            {
                Transform infoPanelTransform = canvasTransform.Find("GlobalInfoPanel");
                if (infoPanelTransform != null)
                {
                    infoPanelGlobal = infoPanelTransform.gameObject;
                    // Find the text component inside the info panel
                    infoTextGlobal = infoPanelGlobal.GetComponentInChildren<TextMeshProUGUI>(true);
                }
            }
        }
    }

    public void Evaluate()
    {
        Debug.Log("=== CHECK PUZZLE ===");

        bool isAllCorrect = true;

        // 1. Check ALL slots instead of stopping at the first error
        foreach (var slot in slots)
        {
            if (!slot.IsCorrect())
            {
                Debug.Log("Puzzle failed at slot: " + slot.name + " (Contains: " + slot.currentGate + " but requires: " + slot.correctGate + ")");

                // Play the error animation on this specific slot
                slot.PlayWrongFeedback();

                isAllCorrect = false;
            }
        }

        // 2. If even one is wrong, exit without completing the level
        if (!isAllCorrect)
        {
            return;
        }

        // 3. Play the success-text animation (about 0.4s for the pop)
        if (successText != null)
        {
            successText.gameObject.SetActive(true);
            successText.transform.localScale = Vector3.zero;

            successText.transform.DOScale(Vector3.one, 0.4f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }

        // 4. The big triumphant pause (give the player time to read)
        // Wait 0.75s after the text appears before doing anything else
        DOVirtual.DelayedCall(0.75f, () => {

            // 5. Success-text exit animation
            // Instead of disappearing abruptly, shrink it quickly in 0.2s
            if (successText != null)
            {
                successText.transform.DOScale(Vector3.zero, 0.2f)
                    .SetEase(Ease.InBack)
                    .SetUpdate(true)
                    .OnComplete(() => {
                        // Once the text has fully vanished...
                        successText.gameObject.SetActive(false);

                        // ...ONLY NOW move to the next level via the ProgressionManager
                        OnPuzzleCompleted();
                    });
            }
            else
            {
                // Failsafe if there is no success text
                OnPuzzleCompleted();
            }

        }).SetUpdate(true);
    }

    // Called when the player clicks the "i" (info) button
    public void ShowLevelInstructions()
    {
        if (infoPanelGlobal != null && infoTextGlobal != null)
        {
            // 1. Assign THIS level's specific text to the shared global panel
            infoTextGlobal.text = levelInstructions;

            // 2. Activate the panel with a small DOTween pop-in animation
            infoPanelGlobal.SetActive(true);
            infoPanelGlobal.transform.localScale = Vector3.zero;
            infoPanelGlobal.transform.DOScale(Vector3.one, 0.3f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }
    }

    void OnPuzzleCompleted()
    {
        Debug.Log("SUCCESS!");

        // 1. Fire the reward event (if any is wired)
        if (onPuzzleSuccessReward != null)
        {
            onPuzzleSuccessReward.Invoke();
        }

        // 2. Restore game state
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
        //Time.timeScale = 1f;

        // The ProgressionManager decides which levels to turn on/off
        ProgressionManager progression = GetComponentInParent<ProgressionManager>();
        if (progression != null)
        {
            progression.AdvanceLevel();
        }
    }
}
