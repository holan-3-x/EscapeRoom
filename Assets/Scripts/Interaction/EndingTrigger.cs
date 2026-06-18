using UnityEngine;
namespace SYSTEMESCAPE {
    public class EndingTrigger : MonoBehaviour {
        private bool fired = false;
        void OnTriggerEnter(Collider other) {
            if (fired) return;
            if (other.CompareTag("Player")) {
                fired = true;
                QuizManager.Instance?.StartEnding();
            }
        }
    }
}