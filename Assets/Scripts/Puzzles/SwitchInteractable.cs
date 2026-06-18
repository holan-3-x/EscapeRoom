using UnityEngine;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // Attach to each of the 8 switch cubes. Set bitIndex 0-7 in the Inspector.
    public class SwitchInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private int bitIndex = 0;

        [Header("Visual — color feedback")]
        [SerializeField] private Renderer switchRenderer;
        [SerializeField] private Color offColor = Color.red;
        [SerializeField] private Color onColor  = Color.green;

        [Header("Optional moving part (the cap that pushes or the lever that flips)")]
        [SerializeField] private Transform movingPart;
        public enum MoveKind { PushIn, FlipRotate }
        [SerializeField] private MoveKind moveKind = MoveKind.PushIn;
        [Tooltip("PushIn: local Z distance to press. FlipRotate: local X angle when ON.")]
        [SerializeField] private float moveAmount = 0.03f;

        private bool isOn = false;
        private Vector3 _restPos;
        private Quaternion _restRot;

        // Lets the BinaryPuzzleManager assign the bit number automatically.
        public void SetBitIndex(int index) => bitIndex = index;
        public int BitIndex => bitIndex;

        private void Start()
        {
            if (switchRenderer == null) switchRenderer = GetComponent<Renderer>();
            if (movingPart != null)
            {
                _restPos = movingPart.localPosition;
                _restRot = movingPart.localRotation;
            }
            UpdateColor(instant: true);
        }

        public string GetPrompt()
        {
            var mgr = BinaryPuzzleManager.Instance;
            if (mgr != null && !mgr.PuzzleUnlocked) return "[E] Switch (locked)";
            return isOn ? "[E] Set to 0 (OFF)" : "[E] Set to 1 (ON)";
        }

        public void Interact()
        {
            var mgr = BinaryPuzzleManager.Instance;
            if (mgr == null || mgr.IsSolved) return;

            // Locked until ARIA gives the door code
            if (!mgr.PuzzleUnlocked)
            {
                ClassroomSceneFlow.Instance?.ShowDialogue("ARIA",
                    "These are the door switches. Talk to me first — I'll explain the code " +
                    "before you start flipping them.", autoClose: true, autoCloseDelay: 3f);
                return;
            }

            // ToggleBit returns false if not accepted; only flip visuals if accepted
            if (mgr.ToggleBit(bitIndex))
            {
                isOn = !isOn;
                UpdateColor(instant: false);
                AnimateMove();
                SoundManager.Instance?.PlaySwitch(isOn);   // up sound for 1, down sound for 0
            }
        }

        private void AnimateMove()
        {
            if (movingPart == null) return;

            if (moveKind == MoveKind.PushIn)
            {
                // Push in and spring back
                movingPart.DOKill();
                movingPart.localPosition = _restPos;
                movingPart.DOLocalMoveZ(_restPos.z + moveAmount, 0.08f)
                    .SetLoops(2, LoopType.Yoyo);
            }
            else // FlipRotate — lever flips to an angle when ON, back when OFF
            {
                movingPart.DOKill();
                float angle = isOn ? moveAmount : 0f;
                movingPart.DOLocalRotate(
                    _restRot.eulerAngles + new Vector3(angle, 0, 0), 0.18f);
            }
        }

        private void UpdateColor(bool instant)
        {
            if (switchRenderer == null) return;
            Color target = isOn ? onColor : offColor;
            if (instant)
                switchRenderer.material.color = target;
            else
                switchRenderer.material.DOColor(target, 0.25f);
        }
    }
}
