using UnityEngine;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // Attach to the CONFIRM button/lever object near the switches.
    public class ConfirmButton : MonoBehaviour, IInteractable
    {
        public enum Axis { X, Y, Z }
        public enum Motion { Rotate, Slide }

        [Header("Animation")]
        [Tooltip("The part that moves (the lever or button cap).")]
        [SerializeField] private Transform movingPart;
        [Tooltip("Rotate = swings like a lever/switch. Slide = pushes/slides.")]
        [SerializeField] private Motion motion = Motion.Rotate;
        [SerializeField] private Axis  axis = Axis.X;
        [Tooltip("Rotate: angle in degrees. Slide: distance in local units.")]
        [SerializeField] private float amount = 40f;
        [SerializeField] private float time = 0.15f;

        public string GetPrompt() => "[E] Confirm code";

        public void Interact()
        {
            SoundManager.Instance?.PlayConfirm();
            Animate();
            BinaryPuzzleManager.Instance?.TrySubmit();
        }

        private void Animate()
        {
            if (movingPart == null) return;
            movingPart.DOKill();

            if (motion == Motion.Rotate)
            {
                Vector3 add = axis switch
                {
                    Axis.X => new Vector3(amount, 0, 0),
                    Axis.Y => new Vector3(0, amount, 0),
                    _      => new Vector3(0, 0, amount),
                };
                // Swing down and back up
                movingPart.DOLocalRotate(movingPart.localEulerAngles + add, time)
                    .SetLoops(2, LoopType.Yoyo);
            }
            else // Slide
            {
                Vector3 p = movingPart.localPosition;
                Tween t = axis switch
                {
                    Axis.X => movingPart.DOLocalMoveX(p.x + amount, time),
                    Axis.Y => movingPart.DOLocalMoveY(p.y + amount, time),
                    _      => movingPart.DOLocalMoveZ(p.z + amount, time),
                };
                t.SetLoops(2, LoopType.Yoyo);
            }
        }
    }
}
