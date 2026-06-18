using UnityEngine;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // The final approve slider/switch on the access panel. Press E to approve.
    // Only works after key + logic gate are done; otherwise ARIA tells you what's missing.
    // Put on the slider, layer = Interactable, add a Collider.

    public class ApproveSliderInteract : MonoBehaviour, IInteractable
    {
        public enum Axis { X, Y, Z }
        public enum Motion { Rotate, Slide }

        [SerializeField] private AccessPanelManager panel;

        [Header("Slider/switch animation")]
        [SerializeField] private Transform movingPart;
        [SerializeField] private Motion motion = Motion.Slide;
        [SerializeField] private Axis axis = Axis.X;
        [Tooltip("Slide: local distance. Rotate: angle in degrees. Negative flips direction.")]
        [SerializeField] private float amount = 0.05f;

        public string GetPrompt()
        {
            if (panel != null && panel.Approved) return "";
            return "[E] Approve";
        }

        private Vector3 _restPos;
        private Vector3 _restRot;
        private bool _captured = false;

        private void CaptureRest()
        {
            if (_captured || movingPart == null) return;
            _restPos = movingPart.localPosition;
            _restRot = movingPart.localEulerAngles;
            _captured = true;
        }

        public void Interact()
        {
            if (panel == null || panel.Approved) return;
            CaptureRest();

            bool approved = panel.TryApprove();

            // Stay UP if it worked; flick up and SPRING BACK if requirements aren't met.
            Animate(stayUp: approved);
        }

        private void Animate(bool stayUp)
        {
            if (movingPart == null) return;
            movingPart.DOKill();

            if (motion == Motion.Slide)
            {
                Vector3 target = _restPos + AxisVec(amount);
                if (stayUp)
                    movingPart.DOLocalMove(target, 0.18f).SetEase(Ease.OutBack);
                else
                    movingPart.DOLocalMove(target, 0.12f)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() => movingPart.DOLocalMove(_restPos, 0.12f)); // spring back
            }
            else // Rotate
            {
                Vector3 target = _restRot + AxisVec(amount);
                if (stayUp)
                    movingPart.DOLocalRotate(target, 0.18f).SetEase(Ease.OutBack);
                else
                    movingPart.DOLocalRotate(target, 0.12f)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() => movingPart.DOLocalRotate(_restRot, 0.12f)); // spring back
            }
        }

        private Vector3 AxisVec(float v) => axis switch
        {
            Axis.X => new Vector3(v, 0, 0),
            Axis.Y => new Vector3(0, v, 0),
            _      => new Vector3(0, 0, v),
        };
    }
}
