using UnityEngine;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // A dummy door that never opens. Put it on the fake doors in the hallway.
    // When the player tries it, it rattles a little and shows a "locked" line.

    public class LockedDoor : MonoBehaviour, IInteractable
    {
        [Header("Optional — the handle/door part that rattles")]
        [SerializeField] private Transform rattlePart;

        [Header("Message shown when the player tries it")]
        [TextArea]
        [SerializeField] private string lockedMessage =
            "Locked. This one won't budge — must be one of the classrooms they sealed for the night.";

        [SerializeField] private string speaker = "You";

        [Header("Optional locked-rattle sound")]
        [SerializeField] private AudioSource rattleSound;

        public string GetPrompt() => "[E] Try the door";

        public void Interact()
        {
            // Little locked-door rattle
            if (rattlePart != null)
            {
                rattlePart.DOKill();
                rattlePart.DOShakeRotation(0.4f, new Vector3(0, 3f, 0), 10, 90);
            }

            if (rattleSound != null) rattleSound.Play();

            ClassroomSceneFlow.Instance?.ShowDialogue(speaker, lockedMessage,
                autoClose: true, autoCloseDelay: 3f);
        }
    }
}
