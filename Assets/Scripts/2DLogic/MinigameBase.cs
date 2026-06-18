namespace BlockGame {
using UnityEngine;

public class MinigameBase : MonoBehaviour
{
    public bool isCompleted = false;

    // Whenever the block game is ACTIVE, 2D physics must be in 'Script' mode
    // (the player steps it manually). This guarantees it even if StartMinigame
    // wasn't called, and restores normal physics when the game closes.
    protected virtual void OnEnable()
    {
        Physics2D.simulationMode = SimulationMode2D.Script;
    }

    protected virtual void OnDisable()
    {
        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
    }

    public virtual void StartMinigame()
    {
        gameObject.SetActive(true);

        Time.timeScale = 0f;

        // --- ATTIVA LA FISICA REALE A TEMPO CONGELATO ---
        // Dice a Unity di non simulare la fisica 2D in automatico
        Physics2D.simulationMode = SimulationMode2D.Script;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public virtual void CloseMinigame()
    {
        Time.timeScale = 1f;
        
        // Ripristina la fisica automatica per il mondo 3D/2D globale
        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void EndMinigame()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        
        // Ripristina la fisica automatica per il mondo 3D/2D globale
        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    protected void Complete()
    {
        isCompleted = true;
        Debug.Log("Minigame completed!");
        EndMinigame();
    }
}
}
