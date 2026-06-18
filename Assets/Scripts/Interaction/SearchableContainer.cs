using UnityEngine;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // Drawers, counters, trash bins — anything the player searches.
    // Press E to open it. If it hides an item, that item appears so the player
    // can then pick it up. Empty containers just say "nothing here".
    //
    // SETUP:
    //  - Put this on the furniture, layer = Interactable, add a Collider.
    //  - If it hides an item: put a PickupItem object (the key, a part) INSIDE the
    //    furniture, DISABLE it (untick its checkbox), and drag it into Hidden Item.
    //  - Optional: assign Moving Part (the drawer front / bin lid) to animate it open.

    public class SearchableContainer : MonoBehaviour, IInteractable
    {
        public enum MotionType { Slide, Rotate, None }
        public enum Axis { X, Y, Z }

        [Header("What's inside (optional)")]
        [Tooltip("A disabled PickupItem placed inside. Leave empty for an empty container.")]
        [SerializeField] private GameObject hiddenItem;
        [Tooltip("If > 0, the found item rises up by this much so it's clearly visible.")]
        [SerializeField] private float revealRiseHeight = 0.15f;

        [Header("Open animation")]
        [Tooltip("Slide = drawer pulls out. Rotate = door/lid swings. None = no movement.")]
        [SerializeField] private MotionType motion = MotionType.Slide;
        [Tooltip("Which local axis to move/rotate along.")]
        [SerializeField] private Axis axis = Axis.Z;
        [Tooltip("The drawer front / cabinet door / bin lid that moves.")]
        [SerializeField] private Transform movingPart;
        [Tooltip("Slide: distance in local units (e.g. 0.3). Rotate: angle in degrees (e.g. 90). " +
                 "Use a NEGATIVE value to go the other direction.")]
        [SerializeField] private float amount = 0.3f;
        [SerializeField] private float openTime = 0.4f;

        [Header("Lines")]
        [TextArea] [SerializeField] private string foundLine  = "There's something in here!";
        [TextArea] [SerializeField] private string emptyLine  = "Nothing useful in here.";
        [SerializeField] private string speaker = "You";

        [Header("Sound")]
        [SerializeField] private AudioSource openSound;

        private bool _opened = false;

        public string GetPrompt() => _opened ? "" : "[E] Search";

        public void Interact()
        {
            if (_opened) return;
            _opened = true;

            if (openSound != null) openSound.Play();
            AnimateOpen();

            if (hiddenItem != null)
            {
                hiddenItem.SetActive(true);   // reveal the item so it can be picked up

                // Stop any leftover physics, then float it up so it's clearly visible
                var rb = hiddenItem.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;

                if (revealRiseHeight > 0f)
                {
                    Vector3 p = hiddenItem.transform.localPosition;
                    hiddenItem.transform.localPosition = p;
                    hiddenItem.transform.DOLocalMoveY(p.y + revealRiseHeight, 0.4f)
                        .SetEase(Ease.OutQuad);
                }

                ClassroomSceneFlow.Instance?.ShowDialogue(speaker, foundLine,
                    autoClose: true, autoCloseDelay: 3f);
            }
            else
            {
                ClassroomSceneFlow.Instance?.ShowDialogue(speaker, emptyLine,
                    autoClose: true, autoCloseDelay: 2.5f);
            }
        }

        private void AnimateOpen()
        {
            if (movingPart == null || motion == MotionType.None) return;
            movingPart.DOKill();

            if (motion == MotionType.Slide)
            {
                Vector3 p = movingPart.localPosition;
                switch (axis)
                {
                    case Axis.X: movingPart.DOLocalMoveX(p.x + amount, openTime).SetEase(Ease.OutQuad); break;
                    case Axis.Y: movingPart.DOLocalMoveY(p.y + amount, openTime).SetEase(Ease.OutQuad); break;
                    default:     movingPart.DOLocalMoveZ(p.z + amount, openTime).SetEase(Ease.OutQuad); break;
                }
            }
            else // Rotate
            {
                Vector3 add = axis switch
                {
                    Axis.X => new Vector3(amount, 0, 0),
                    Axis.Y => new Vector3(0, amount, 0),
                    _      => new Vector3(0, 0, amount),
                };
                movingPart.DOLocalRotate(movingPart.localEulerAngles + add, openTime)
                    .SetEase(Ease.OutQuad);
            }
        }

        // Right-click the component header -> Test Open to preview the motion in the Editor.
        [ContextMenu("Test Open")]
        private void TestOpenPreview()
        {
            if (movingPart != null) AnimateOpen();
        }
    }
}
