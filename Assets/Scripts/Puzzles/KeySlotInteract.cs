using UnityEngine;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // The keyhole on the access panel. Press E to insert the key (if you have it).
    // Put on the keyhole object, layer = Interactable, add a Collider.

    public class KeySlotInteract : MonoBehaviour, IInteractable
    {
        public enum Axis { X, Y, Z }

        [SerializeField] private AccessPanelManager panel;

        [Header("Key-insert animation")]
        [Tooltip("A key model parented in the slot at its FINAL resting position.")]
        [SerializeField] private Transform insertedKeyVisual;
        [Tooltip("Which local axis the key slides IN along.")]
        [SerializeField] private Axis slideAxis = Axis.Z;
        [Tooltip("How far out the key starts before sliding in. Negative flips the side.")]
        [SerializeField] private float slideInDistance = 0.08f;
        [Tooltip("Which local axis the key TURNS around after slotting in.")]
        [SerializeField] private Axis turnAxis = Axis.Z;
        [Tooltip("Turn angle in degrees (like unlocking). Negative reverses.")]
        [SerializeField] private float turnAngle = 90f;

        public string GetPrompt()
        {
            if (panel != null && panel.KeyInserted) return "";
            return "[E] Insert key";
        }

        public void Interact()
        {
            if (panel == null || panel.KeyInserted) return;

            bool ok = panel.TryInsertKey();
            if (ok && insertedKeyVisual != null)
                StartCoroutine(PlayInsertAnimation());
        }

        private System.Collections.IEnumerator PlayInsertAnimation()
        {
            // Show the key slightly out of the slot, then slide it in along the chosen axis
            insertedKeyVisual.gameObject.SetActive(true);
            Vector3 seated  = insertedKeyVisual.localPosition;
            Vector3 startOut = seated + AxisVec(slideAxis, slideInDistance);
            insertedKeyVisual.localPosition = startOut;

            insertedKeyVisual.DOLocalMove(seated, 0.35f).SetEase(Ease.OutQuad);
            yield return new WaitForSeconds(0.4f);

            // Turn the key like a lock, around the chosen axis
            insertedKeyVisual.DOLocalRotate(
                insertedKeyVisual.localEulerAngles + AxisVec(turnAxis, turnAngle), 0.35f)
                .SetEase(Ease.OutBack);
        }

        private static Vector3 AxisVec(Axis a, float v) => a switch
        {
            Axis.X => new Vector3(v, 0, 0),
            Axis.Y => new Vector3(0, v, 0),
            _      => new Vector3(0, 0, v),
        };
    }
}
