using UnityEngine;
namespace SYSTEMESCAPE {
    public class PuzzleZoneTrigger : MonoBehaviour {
        void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Player"))
                BinaryPuzzleManager.Instance?.ShowTeachingUI();
        }
        void OnTriggerExit(Collider other) {
            if (other.CompareTag("Player"))
                BinaryTeachingUI.Instance?.HidePanel();
        }
    }
}