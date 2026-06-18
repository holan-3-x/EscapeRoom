using UnityEngine;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // A normal room light switch for the hallway, computer lab, etc.
    // Controls its OWN set of lights (unlike the classroom teaching switch).
    // Put on the switch model, layer = Interactable, add a Collider.

    public class AreaLightSwitch : MonoBehaviour, IInteractable
    {
        public enum Axis { X, Y, Z }

        [Header("Lights this switch controls (drag the Light GameObjects)")]
        [SerializeField] private GameObject[] lights;
        [Tooltip("Should the lights start ON or OFF?")]
        [SerializeField] private bool startOn = false;

        [Header("Switch handle (optional)")]
        [SerializeField] private Transform handle;
        [SerializeField] private Axis handleAxis = Axis.X;
        [SerializeField] private float onAngle  = -30f;
        [SerializeField] private float offAngle =  30f;

        [Header("ARIA joke on first flip (optional)")]
        [TextArea]
        [SerializeField] private string firstFlipLine =
            "Ha — flipping switches again? On is 1, off is 0. See, you DO still remember your binary.";
        [SerializeField] private bool ariaJokeOnce = true;

        private bool _isOn;
        private bool _jokeDone = false;

        private void Start()
        {
            _isOn = startOn;
            ApplyLights();
            SetHandle(instant: true);
        }

        public string GetPrompt() => _isOn ? "[E] Turn lights OFF" : "[E] Turn lights ON";

        public void Interact()
        {
            _isOn = !_isOn;
            ApplyLights();
            SetHandle(instant: false);
            SoundManager.Instance?.PlaySwitch(_isOn);

            // ARIA's callback joke (first time, when turning ON)
            if (_isOn && !_jokeDone && !string.IsNullOrEmpty(firstFlipLine))
            {
                if (ariaJokeOnce) _jokeDone = true;
                ClassroomSceneFlow.Instance?.ShowDialogue("ARIA", firstFlipLine,
                    autoClose: true, autoCloseDelay: 4f);
            }
        }

        private void ApplyLights()
        {
            if (lights == null) return;
            foreach (var l in lights)
                if (l != null) l.SetActive(_isOn);
        }

        private void SetHandle(bool instant)
        {
            if (handle == null) return;
            float a = _isOn ? onAngle : offAngle;
            Vector3 e = handleAxis switch
            {
                Axis.X => new Vector3(a, 0, 0),
                Axis.Y => new Vector3(0, a, 0),
                _      => new Vector3(0, 0, a),
            };
            if (instant) handle.localRotation = Quaternion.Euler(e);
            else handle.DOLocalRotate(e, 0.2f);
        }
    }
}
