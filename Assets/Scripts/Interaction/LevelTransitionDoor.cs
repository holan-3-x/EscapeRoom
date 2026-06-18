using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // The door that takes the player to the next level (a different scene).
    // Put this on the Level 2 door(s) in the hallway.
    //
    // It only works after the current level's puzzle is finished (requirePuzzleDone),
    // so the player can't skip ahead. Walk up, press E, screen fades, next scene loads.

    public class LevelTransitionDoor : MonoBehaviour, IInteractable
    {
        [Header("Where this door leads")]
        [Tooltip("Exact scene name, e.g. Level1_ComputerLab. Must be in Build Settings.")]
        [SerializeField] private string nextSceneName = "Level1_ComputerLab";

        [Header("Gate — must the puzzle be solved first?")]
        [SerializeField] private bool requirePuzzleDone = true;

        [Header("Fade (full-screen black Image)")]
        [SerializeField] private Image fadeOverlay;
        [SerializeField] private float fadeTime = 0.8f;

        [Header("Lines")]
        [TextArea] [SerializeField] private string lockedLine =
            "It's locked from this side. I need to deal with this room first.";
        [TextArea] [SerializeField] private string enterLine =
            "This one's open... let's see what's on the other side.";

        private bool _leaving = false;

        public string GetPrompt() => "[E] Go through the door";

        public void Interact()
        {
            if (_leaving) return;

            if (requirePuzzleDone && !PuzzleIsDone())
            {
                ClassroomSceneFlow.Instance?.ShowDialogue("You", lockedLine,
                    autoClose: true, autoCloseDelay: 3f);
                return;
            }

            _leaving = true;
            SoundManager.Instance?.PlayDoor();
            ClassroomSceneFlow.Instance?.ShowDialogue("You", enterLine,
                autoClose: true, autoCloseDelay: 2f);
            StartCoroutine(LoadAfterDelay());
        }

        private bool PuzzleIsDone()
        {
            // Level 0 is "done" once the binary puzzle is solved.
            if (BinaryPuzzleManager.Instance != null)
                return BinaryPuzzleManager.Instance.IsSolved;
            // If there's no puzzle in this scene, treat as done.
            return true;
        }

        private IEnumerator LoadAfterDelay()
        {
            yield return new WaitForSeconds(2f);

            // Remember progress before leaving
            if (GameManager.Instance != null) GameManager.Instance.BinaryPuzzleDone = true;
            SaveSystem.Save();

            if (fadeOverlay != null)
            {
                fadeOverlay.gameObject.SetActive(true);
                fadeOverlay.DOFade(1f, fadeTime);
                yield return new WaitForSeconds(fadeTime);
            }

            SceneManager.LoadScene(nextSceneName);
        }
    }
}
