using UnityEngine;

namespace SYSTEMESCAPE
{
    // Attach to the Main Camera.
    // Moves the camera closer to the player when a wall is between the camera and the player.
    // Does NOT cause flying — only changes the camera's local Z position.

    public class CameraCollision : MonoBehaviour
    {
        [SerializeField] private Transform cameraArm;
        [SerializeField] private float     normalDistance  = 0.1f;  // your preferred Z offset
        [SerializeField] private float     collisionRadius = 0.1f;
        [SerializeField] private LayerMask collisionLayers;          // set in Inspector — see below

        private void LateUpdate()
        {
            if (cameraArm == null) return;

            float targetDist = normalDistance;

            // Cast from the player pivot point toward the camera's desired position
            Vector3 origin  = cameraArm.position;
            Vector3 camDir  = -cameraArm.forward; // behind the player

            if (normalDistance > 0.01f &&
                Physics.SphereCast(origin, collisionRadius, camDir,
                    out RaycastHit hit, normalDistance, collisionLayers,
                    QueryTriggerInteraction.Ignore))
            {
                // Stop before the wall, leave a small gap
                targetDist = Mathf.Clamp(hit.distance - 0.05f, 0.05f, normalDistance);
            }

            // Only update Z — never touches Y so it cannot cause flying
            Vector3 pos = transform.localPosition;
            pos.z = Mathf.Lerp(pos.z, -targetDist, Time.deltaTime * 10f);
            transform.localPosition = pos;
        }
    }
}
