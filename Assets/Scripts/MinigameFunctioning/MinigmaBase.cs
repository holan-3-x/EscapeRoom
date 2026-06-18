using UnityEngine;

public class MinigameBase : MonoBehaviour
{
    public bool isCompleted = false;

    public virtual void StartMinigame()
    {
        gameObject.SetActive(true);
    }

    public virtual void CloseMinigame()
    {
        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.SetActive(false);
        }
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void EndMinigame()
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