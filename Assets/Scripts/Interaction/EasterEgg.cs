using UnityEngine;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // A fun interactable. Press E -> plays a line and/or a sound, maybe wiggles.
    // Drop it on a rubber duck, a poster, a coffee mug, a sleeping classmate, etc.
    // Layer = Interactable, needs a Collider.

    public class EasterEgg : MonoBehaviour, IInteractable
    {
        [Header("Prompt shown when looked at")]
        [SerializeField] private string prompt = "[E] Look";

        [Header("Line spoken on interact")]
        [TextArea]
        [SerializeField] private string line = "...five more minutes, mom.";
        [SerializeField] private string speaker = "???";

        [Header("Optional sound (e.g. duck squeak)")]
        [SerializeField] private AudioSource sound;

        [Header("Optional little wiggle on interact")]
        [SerializeField] private bool wiggle = true;

        [Header("Can it be used more than once?")]
        [SerializeField] private bool repeatable = true;

        private bool _used = false;

        public string GetPrompt() => (!repeatable && _used) ? "" : prompt;

        public void Interact()
        {
            if (!repeatable && _used) return;
            _used = true;

            if (sound != null) sound.Play();

            if (wiggle)
            {
                transform.DOKill();
                transform.DOPunchRotation(new Vector3(0, 0, 12f), 0.4f, 8, 0.6f);
            }

            if (!string.IsNullOrEmpty(line))
                ClassroomSceneFlow.Instance?.ShowDialogue(speaker, line,
                    autoClose: true, autoCloseDelay: 2.5f);
        }
    }
}
