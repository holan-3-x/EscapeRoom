using UnityEngine;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // A reusable interactable prop that plays an animation + sound on E.
    // Perfect for the vintage computer, projector, monitor — anything decorative
    // that should react when the player examines it.
    //
    // SETUP: put on the prop, layer = Interactable, add a Collider.
    //   - If the prop has its own Animator: assign it + the trigger/state name.
    //   - Optionally assign a sound and a line for ARIA / the player.

    public class AnimatedProp : MonoBehaviour, IInteractable
    {
        [Header("Prompt")]
        [SerializeField] private string prompt = "[E] Examine";

        [Header("Animation")]
        [Tooltip("The prop's Animator (if it came with one).")]
        [SerializeField] private Animator animator;
        [Tooltip("Trigger name to fire on the Animator (leave blank to just Play a state).")]
        [SerializeField] private string animatorTrigger = "";
        [Tooltip("State name to Play if no trigger is used.")]
        [SerializeField] private string animatorState = "";
        [Tooltip("If no Animator, do a little wiggle instead.")]
        [SerializeField] private bool wiggleIfNoAnimator = true;

        [Header("Sound")]
        [SerializeField] private AudioSource sound;

        [Header("Optional line")]
        [TextArea] [SerializeField] private string line = "";
        [SerializeField] private string speaker = "You";

        [Header("Behaviour")]
        [SerializeField] private bool repeatable = true;
        private bool _used = false;

        public string GetPrompt() => (!repeatable && _used) ? "" : prompt;

        public void Interact()
        {
            if (!repeatable && _used) return;
            _used = true;

            // Animation
            if (animator != null)
            {
                if (!string.IsNullOrEmpty(animatorTrigger)) animator.SetTrigger(animatorTrigger);
                else if (!string.IsNullOrEmpty(animatorState)) animator.Play(animatorState);
            }
            else if (wiggleIfNoAnimator)
            {
                transform.DOKill();
                transform.DOPunchRotation(new Vector3(0, 8f, 0), 0.4f, 6, 0.6f);
            }

            // Sound
            if (sound != null) sound.Play();

            // Line
            if (!string.IsNullOrEmpty(line))
                ClassroomSceneFlow.Instance?.ShowDialogue(speaker, line,
                    autoClose: true, autoCloseDelay: 3f);
        }
    }
}
