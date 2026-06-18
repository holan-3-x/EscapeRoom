using UnityEngine;

namespace SYSTEMESCAPE
{
    // Attach to a child of the CRT desk. Add a Sphere Collider, tick Is Trigger.
    // Radius about 2.5 units. This fires the CRT intro conversation.

    public class CRTDeskTrigger : MonoBehaviour
    {
        [SerializeField] private PortableComputer portableComputer;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                portableComputer?.OnPlayerEnterCRTZone();
        }
    }
}
