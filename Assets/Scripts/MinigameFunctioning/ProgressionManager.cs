using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class ProgressionManager : MinigameBase
{
    [Header("Level Configuration")]
    public GameObject[] levels; // This list holds your level Panels ("Level 1", "Level 2", ...)

    [Header("Outside-World Interactions (whole minigame finished)")]
    public GameObject interactionPromptUI;
    public GameObject worldTriggerObject;
    public UnityEvent onAllLevelsCompletedReward;

    private int currentLevelIndex = 0;

    private void Awake()
    {
        SetupLevels();
    }

    public override void StartMinigame()
    {
        if (isCompleted) return;

        if (levels == null || levels.Length == 0)
        {
            SetupLevels();
        }

        base.StartMinigame();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;

        InitializeFirstLevel();
    }

    private void InitializeFirstLevel()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] != null)
            {
                CanvasGroup cg = levels[i].GetComponent<CanvasGroup>();
                if (cg == null) cg = levels[i].AddComponent<CanvasGroup>();

                if (i == currentLevelIndex)
                {
                    levels[i].SetActive(true);
                    cg.alpha = 1f;
                    levels[i].transform.localScale = Vector3.one;
                }
                else
                {
                    levels[i].SetActive(false);
                }
            }
        }
    }

    public void AdvanceLevel()
    {
        int oldLevelIndex = currentLevelIndex;
        currentLevelIndex++;

        if (currentLevelIndex >= levels.Length)
        {
            CompleteAllMinigame();
        }
        else
        {
            AnimateLevelTransition(oldLevelIndex, currentLevelIndex);
        }
    }

    private void AnimateLevelTransition(int oldIndex, int newIndex)
    {
        GameObject oldPanel = levels[oldIndex];
        GameObject newPanel = levels[newIndex];

        if (oldPanel != null)
        {
            CanvasGroup oldCg = oldPanel.GetComponent<CanvasGroup>();
            if (oldCg == null) oldCg = oldPanel.AddComponent<CanvasGroup>();

            // Stop any previous animations for safety
            oldPanel.transform.DOKill();
            oldCg.DOKill();

            // 1. EXIT ANIMATION (the current panel shrinks and fades out)
            oldCg.DOFade(0f, 0.35f).SetUpdate(true);
            oldPanel.transform.DOScale(Vector3.one * 0.75f, 0.35f)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(() => {
                    // Disable the old panel ONLY once DOTween finished shrinking it
                    oldPanel.SetActive(false);

                    // 2. ENTER ANIMATION of the next panel
                    if (newPanel != null)
                    {
                        PrepareAndIntroduceNewLevel(newPanel);
                    }
                });
        }
        else if (newPanel != null)
        {
            PrepareAndIntroduceNewLevel(newPanel);
        }
    }

    private void PrepareAndIntroduceNewLevel(GameObject newPanel)
    {
        CanvasGroup newCg = newPanel.GetComponent<CanvasGroup>();
        if (newCg == null) newCg = newPanel.AddComponent<CanvasGroup>();

        newPanel.transform.DOKill();
        newCg.DOKill();

        // Prepare the hidden starting state before showing it
        newCg.alpha = 0f;
        newPanel.transform.localScale = Vector3.one * 0.6f;

        // Activate the panel so DOTween can start animating it
        newPanel.SetActive(true);

        // ENTER ANIMATION (the new panel does an elastic "pop")
        newCg.DOFade(1f, 0.4f).SetUpdate(true);
        newPanel.transform.DOScale(Vector3.one, 0.5f)
            .SetEase(Ease.OutBack) // bounce effect
            .SetUpdate(true);
    }

    void SetupLevels()
    {
        PuzzleManager[] allManagers = GetComponentsInChildren<PuzzleManager>(true);
        levels = new GameObject[allManagers.Length];

        for (int i = 0; i < allManagers.Length; i++)
        {
            // Go up one level (the direct parent of the PuzzleManager is the level Panel)
            if (allManagers[i].transform.parent != null)
            {
                levels[i] = allManagers[i].transform.parent.gameObject;
            }
        }
        Debug.Log($"[ProgressionManager] Setup complete. Found {levels.Length} level panels.");
    }

    void CompleteAllMinigame()
    {
        Debug.Log("CONGRATULATIONS! All levels completed!");
        if (interactionPromptUI != null) interactionPromptUI.SetActive(false);

        if (worldTriggerObject != null)
        {
            Collider triggerCollider = worldTriggerObject.GetComponent<Collider>();
            if (triggerCollider != null) triggerCollider.enabled = false;
        }

        if (onAllLevelsCompletedReward != null)
        {
            onAllLevelsCompletedReward.Invoke();
        }

        Complete();
    }
}
