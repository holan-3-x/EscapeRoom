using System.Collections;
using UnityEngine;
using TMPro;

namespace SYSTEMESCAPE
{
    // Attach to the dialogue text TMP object.
    // Call TypewriterEffect.Instance.Play("text here") to animate text letter by letter.
    // Plays a typing sound per character if assigned.
    // Player can press E or Space to skip to the end instantly.

    public class TypewriterEffect : MonoBehaviour
    {
        public static TypewriterEffect Instance { get; private set; }

        [SerializeField] private TextMeshProUGUI textLabel;
        [Tooltip("Seconds per character. Higher = slower, easier to read. 0.045 is a calm pace.")]
        [SerializeField] private float           charDelay    = 0.045f; // seconds per character
        [SerializeField] private AudioSource     audioSource;
        [SerializeField] private AudioClip       typingSound;
        [SerializeField] private int             soundEvery   = 2;      // play sound every N chars

        private Coroutine _coroutine;
        private bool      _isTyping = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (textLabel == null) textLabel = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            // Skip animation on E or Space
            if (_isTyping)
            {
                var kb = UnityEngine.InputSystem.Keyboard.current;
                if (kb != null && (kb.eKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame))
                    SkipToEnd();
            }
        }

        public void Play(string fullText)
        {
            if (_coroutine != null) StopCoroutine(_coroutine);
            _coroutine = StartCoroutine(TypeRoutine(fullText));
        }

        public void SkipToEnd()
        {
            if (_coroutine != null) StopCoroutine(_coroutine);
            _isTyping = false;
            // Reveal whatever the last full text was
            if (textLabel != null) textLabel.maxVisibleCharacters = int.MaxValue;
        }

        public bool IsTyping => _isTyping;

        private IEnumerator TypeRoutine(string fullText)
        {
            _isTyping = true;
            textLabel.text = fullText;
            textLabel.maxVisibleCharacters = 0;

            int charCount = 0;
            foreach (char _ in fullText)
            {
                textLabel.maxVisibleCharacters++;
                charCount++;

                // Play typing sound every N characters
                if (typingSound != null && audioSource != null && charCount % soundEvery == 0)
                    audioSource.PlayOneShot(typingSound, 0.4f);

                yield return new WaitForSeconds(charDelay);
            }

            _isTyping = false;
        }
    }
}
