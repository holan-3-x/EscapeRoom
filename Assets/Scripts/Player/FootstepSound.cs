using UnityEngine;

namespace SYSTEMESCAPE
{
    // Attach to the Player root object.
    // Plays footstep sounds based on movement speed.
    // No animation events needed — purely time-based.

    public class FootstepSound : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] footstepClips;   // 2-4 different step sounds
        [SerializeField] private float walkStepInterval = 0.5f;  // seconds between steps walking
        [SerializeField] private float runStepInterval  = 0.3f;  // seconds between steps running

        private CharacterController _cc;
        private float _stepTimer = 0f;
        private int   _lastClip  = -1;

        private void Start()
        {
            _cc = GetComponent<CharacterController>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f;  // 2D sound — no positional audio needed
            audioSource.volume       = 0.6f;
        }

        private void Update()
        {
            if (_cc == null || footstepClips == null || footstepClips.Length == 0) return;

            Vector3 vel = _cc.velocity;
            float horizontalSpeed = new Vector2(vel.x, vel.z).magnitude;

            if (horizontalSpeed < 0.5f || !_cc.isGrounded)
            {
                _stepTimer = 0f;  // reset when standing still
                return;
            }

            bool running = horizontalSpeed > 4f;
            float interval = running ? runStepInterval : walkStepInterval;

            _stepTimer -= Time.deltaTime;
            if (_stepTimer <= 0f)
            {
                PlayStep();
                _stepTimer = interval;
            }
        }

        private void PlayStep()
        {
            if (footstepClips.Length == 0) return;

            // Pick a random clip, avoid repeating the same one twice
            int index;
            do { index = Random.Range(0, footstepClips.Length); }
            while (index == _lastClip && footstepClips.Length > 1);

            _lastClip = index;
            audioSource.PlayOneShot(footstepClips[index], 0.5f);
        }
    }
}
