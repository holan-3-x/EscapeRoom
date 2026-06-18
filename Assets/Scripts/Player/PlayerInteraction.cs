using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // Attach to the Main Camera (child of CameraHolder).
    // Raycasts forward, detects IInteractable objects, shows crosshair + prompt.
    //
    // UI SETUP (on a Screen Space - Overlay Canvas):
    //   - crosshairDot    — small Image (16x16 white circle, center of screen)
    //   - promptLabel     — TextMeshProUGUI below crosshair
    //
    // Crosshair turns yellow and grows slightly when looking at something interactable.

    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Raycast")]
        [SerializeField] private float interactRange = 3f;
        [SerializeField] private LayerMask interactableLayer;

        [Header("UI")]
        [SerializeField] private Image            crosshairDot;
        [SerializeField] private TextMeshProUGUI  promptLabel;

        // Crosshair colors
        private static readonly Color ColorIdle        = new Color(1f, 1f, 1f, 0.6f);
        private static readonly Color ColorInteractable = new Color(1f, 0.9f, 0f, 1f); // yellow

        private Camera _cam;
        private bool   _interactionEnabled = true;
        private bool   _wasLookingAtSomething = false;

        // -- Lifecycle ---------------------------------------------------------

        private void Start()
        {
            _cam = GetComponent<Camera>();
            if (_cam == null) _cam = Camera.main;

            SetPrompt("");
            SetCrosshairState(active: false, instant: true);

            // Warn if layer mask is not set — most common cause of E key not working
            if (interactableLayer.value == 0)
                Debug.LogWarning("[PlayerInteraction] Interactable Layer mask is empty! " +
                    "Select the Main Camera -> PlayerInteraction -> set Interactable Layer to 'Interactable'.");
        }

        private void Update()
        {
            if (!_interactionEnabled)
            {
                SetPrompt("");
                return;
            }

            CheckForInteractable();
        }

        // -- Core --------------------------------------------------------------

        private void CheckForInteractable()
        {
            Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactRange, interactableLayer))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>()
                    ?? hit.collider.GetComponentInParent<IInteractable>();

                if (interactable != null)
                {
                    SetPrompt(interactable.GetPrompt());
                    SetCrosshairState(active: true, instant: false);
                    _wasLookingAtSomething = true;

                    if (UnityEngine.InputSystem.Keyboard.current != null &&
                        UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
                    {
                        // Don't interact with world objects while dialogue is open
                        bool dialogueOpen = ClassroomSceneFlow.Instance != null &&
                                            ClassroomSceneFlow.Instance.IsDialogueOpen;
                        if (!dialogueOpen)
                            interactable.Interact();
                    }
                    return;
                }
            }

            // Nothing found
            if (_wasLookingAtSomething)
            {
                SetPrompt("");
                SetCrosshairState(active: false, instant: false);
                _wasLookingAtSomething = false;
            }
        }

        // -- Crosshair helpers -------------------------------------------------

        private void SetCrosshairState(bool active, bool instant)
        {
            if (crosshairDot == null) return;

            Color target = active ? ColorInteractable : ColorIdle;
            Vector3 scale = active ? Vector3.one * 1.4f : Vector3.one;

            if (instant)
            {
                crosshairDot.color              = target;
                crosshairDot.transform.localScale = scale;
            }
            else
            {
                crosshairDot.DOColor(target, 0.15f);
                crosshairDot.transform.DOScale(scale, 0.15f);
            }
        }

        private void SetPrompt(string text)
        {
            if (promptLabel != null) promptLabel.text = text;
        }

        // -- Public API --------------------------------------------------------

        public void EnableInteraction()
        {
            _interactionEnabled = true;
            if (crosshairDot != null) crosshairDot.gameObject.SetActive(true);
        }

        public void DisableInteraction()
        {
            _interactionEnabled = false;
            SetPrompt("");
            SetCrosshairState(active: false, instant: true);
            // Fully hide the crosshair (e.g. during the minigame or pause)
            if (crosshairDot != null) crosshairDot.gameObject.SetActive(false);
        }
    }
}
