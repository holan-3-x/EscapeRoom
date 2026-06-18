using UnityEngine;
using UnityEngine.InputSystem;   // new Input System

public class CubeInteraction : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject interactionPrompt; // Drag your "Press E" text here
    public GameObject uiPanel;           // Drag your main UI Panel here

    private bool isPlayerClose = false;
    private bool isPanelOpen = false; // Tracks if the panel is currently on screen

    void Awake()
    {
        // This forces them to turn off automatically when the game boots up
        if (uiPanel != null) uiPanel.SetActive(false);
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
    }

    void Update()
    {
        if (!isPlayerClose) return;
        var kb = Keyboard.current;
        if (kb == null) return;

        // E opens (or closes) the panel
        if (kb.eKey.wasPressedThisFrame)
        {
            if (isPanelOpen) ClosePanel();
            else             OpenPanel();
        }
        // ESC closes the panel
        if (isPanelOpen && kb.escapeKey.wasPressedThisFrame)
        {
            ClosePanel();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerClose = true;
            if (!isPanelOpen)
            {
                interactionPrompt.SetActive(true); // Show "Press E" message
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerClose = false;
            interactionPrompt.SetActive(false); // Hide "Press E" message
            ClosePanel();                       // Automatically close panel if they walk away
        }
    }

    void OpenPanel()
    {
        uiPanel.SetActive(true);
        interactionPrompt.SetActive(false); // Hide the prompt while looking at the panel
        isPanelOpen = true;

        // FREEZE THE GAME (Stops physics, animations, and time-based movement)
        Time.timeScale = 0f;
        
        // Unlock mouse cursor to click panel buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ClosePanel()
    {
        uiPanel.SetActive(false);
        isPanelOpen = false;

        // UNFREEZE THE GAME (Resumes everything back to normal)
        Time.timeScale = 1f;
        
        // Lock mouse back to screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Show the prompt again if the player is still standing next to the cube
        if (isPlayerClose)
        {
            interactionPrompt.SetActive(true);
        }
    }
}