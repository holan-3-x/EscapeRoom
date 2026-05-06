using UnityEngine;

public class MinigameBase : MonoBehaviour
{
    public bool isCompleted = false;

    public virtual void StartMinigame()
    {
        gameObject.SetActive(true);
    }

    public virtual void EndMinigame()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
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