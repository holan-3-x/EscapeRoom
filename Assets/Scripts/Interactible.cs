using UnityEngine;
using UnityEngine.InputSystem;   // new Input System

public class Interactable : MonoBehaviour
{
    [Header("Interaction UI")]
    public GameObject pressEText;

    [Header("Minigame Reference")]
    [Tooltip("Assign the object that has the ProgressionManager here.")]
    public MinigameBase minigame;

    private bool playerInRange = false;
    private bool isMinigameOpen = false;

    void Update()
    {
        // If the minigame is permanently completed, block all logic in this script
        if (minigame == null || minigame.isCompleted)
        {
            if (pressEText.activeSelf) pressEText.SetActive(false);
            return;
        }

        var kb = Keyboard.current;
        if (kb == null) return;

        // 1. OPEN with E
        if (playerInRange && !isMinigameOpen && kb.eKey.wasPressedThisFrame)
        {
            OpenMinigame();
        }

        // 2. CLOSE with ESC (only if the minigame is actually open)
        if (isMinigameOpen && kb.escapeKey.wasPressedThisFrame)
        {
            CloseMinigame();
        }
    }

    void OpenMinigame()
    {
        if (minigame == null) return;

        isMinigameOpen = true;
        pressEText.SetActive(false);

        // The ProgressionManager handles SetActive, timeScale and the mouse
        minigame.StartMinigame();
    }

    void CloseMinigame()
    {
        if (minigame == null) return;

        isMinigameOpen = false;

        // The ProgressionManager handles shutdown, timeScale and the mouse
        minigame.CloseMinigame();

        if (playerInRange)
        {
            pressEText.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // If the minigame is already completed, don't show the prompt
        if (minigame == null || minigame.isCompleted) return;

        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            pressEText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            pressEText.SetActive(false);
        }
    }
}
