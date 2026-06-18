using UnityEngine;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // Attach to the Potato PC 3D object on the desk.
    // The CRT on the teacher desk has a red spotlight. When the player enters
    // the spotlight trigger, the CRT "wakes up" and tells the player to pick up
    // the portable computer. Once picked up, it goes to the backpack and can
    // be used from inventory to get hints at any time.

    public class PortableComputer : MonoBehaviour, IInteractable
    {
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip   bootSound;

        [Header("Visual — spotlight on CRT desk")]
        [SerializeField] private Light redSpotlight;      // the red spot light on CRT

        [Header("Inventory")]
        [Tooltip("Icon shown in the backpack for ARIA. Assign any sprite.")]
        [SerializeField] private Sprite ariaIcon;

        private ARIATerminal _aria;
        private bool _introduced = false;   // has CRT started talking yet
        private bool _pickedUp   = false;

        private void Awake() => _aria = GetComponent<ARIATerminal>();

        private void Start()
        {
            // Red spotlight starts off — turns on when player wakes up
            if (redSpotlight != null) redSpotlight.enabled = false;
        }

        // Called by ClassroomSceneFlow after the wake-up sequence
        public void WakeUp()
        {
            if (redSpotlight != null)
            {
                redSpotlight.enabled = true;
                // Pulse the red light to draw attention
                DOTween.To(() => redSpotlight.intensity,
                    x => redSpotlight.intensity = x,
                    0.2f, 0.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            }
        }

        // Player walks into trigger zone around the CRT desk (red spotlight area)
        // Add a sphere/box trigger collider on a child of the CRT desk and call this
        public void OnPlayerEnterCRTZone()
        {
            if (_introduced || _pickedUp) return;

            // The player should grab their backpack first.
            if (GameManager.Instance != null && !GameManager.Instance.HasBackpack)
            {
                ClassroomSceneFlow.Instance?.ShowDialogue("You",
                    "That red glow on the teacher's desk is new... but where's my backpack? " +
                    "I should find that first.",
                    autoClose: true, autoCloseDelay: 3.5f);
                return;
            }

            _introduced = true;

            if (bootSound != null && audioSource != null) audioSource.PlayOneShot(bootSound);

            ClassroomSceneFlow.Instance?.ShowDialogue("Old Monitor",
                "*bzzt* ... *static crackle* ...\n\n" +
                "Hey. HEY — over here, the red glow. Yeah, you, the one who slept through class.\n\n" +
                "See that little handheld unit on the desk beside me? Pick it up. " +
                "It runs ARIA — she's smart, she's portable, and she's your only way out of this room.",
                autoClose: false);
        }

        // Before pickup: "Pick up". After pickup: "Talk to ARIA" (it stays on the desk).
        public string GetPrompt() => _pickedUp ? "[E] Talk to ARIA" : "[E] Pick up computer";

        public void Interact()
        {
            if (!_pickedUp)
            {
                // Must have the backpack first (same gate as the CRT intro)
                if (GameManager.Instance != null && !GameManager.Instance.HasBackpack)
                {
                    ClassroomSceneFlow.Instance?.ShowDialogue("You",
                        "I should grab my backpack first — I'll have nowhere to put this otherwise.",
                        autoClose: true, autoCloseDelay: 3f);
                    return;
                }

                _pickedUp = true;

                // Stop the red light pulsing
                if (redSpotlight != null)
                {
                    DOTween.Kill(redSpotlight);
                    redSpotlight.enabled = false;
                }

                // Add ARIA to the backpack (with an icon)
                InventorySystem.Instance?.AddItem(new InventoryItem
                {
                    itemName = "ARIA Terminal",
                    icon     = ariaIcon
                });

                if (audioSource != null && bootSound != null) audioSource.PlayOneShot(bootSound);
                SoundManager.Instance?.PlayBoot();

                // A little "powering on" pop, but DO NOT hide the object — ARIA lives here
                transform.DOPunchScale(Vector3.one * 0.15f, 0.4f, 6, 0.6f);

                // Start the lesson immediately (ARIA introduces herself)
                if (_aria != null) _aria.AdvanceLesson();
                return;
            }

            // Already picked up — pressing E continues ARIA's lesson
            if (_aria != null) _aria.AdvanceLesson();
        }
    }
}
