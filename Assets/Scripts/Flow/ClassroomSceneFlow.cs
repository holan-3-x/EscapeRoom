using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // -- State machine for Level 0: Classroom --
    // States:  Sleeping -> DarkExplore -> PuzzleActive -> PuzzleSolved -> Exit
    //
    // HOW TO SET UP IN THE INSPECTOR:
    //  1. Create a Canvas (Screen Space - Overlay).
    //  2. Add a full-screen black Image as "fadeOverlay".
    //  3. Add a Panel as "dialoguePanel" with a child TMP text "dialogueText"
    //     and a child TMP text "speakerName".
    //  4. Add a Button as "dialogueCloseButton" (label: "Continue").
    //  5. Add a Panel as "feedbackPanel" with a child TMP text "feedbackText".
    //  6. Wire up the classroom Light(s) to "classroomLights".
    //  7. In BinaryPuzzleManager.OnPuzzleSolved -> drag this GO -> call OnPuzzleSolved().

    public class ClassroomSceneFlow : MonoBehaviour
    {
        public static ClassroomSceneFlow Instance { get; private set; }

        public enum FlowState { Sleeping, DarkExplore, PuzzleActive, PuzzleSolved, Exit }
        public FlowState State { get; private set; }
        public bool IsDialogueOpen => dialoguePanel != null && dialoguePanel.gameObject.activeSelf;

        [Header("Scene References")]
        [SerializeField] private Light[] classroomLights;
        [SerializeField] private GameObject playerObject;
        [SerializeField] private PortableComputer portableComputer;

        [Header("Intro — objects visible BEFORE sleep, hidden AFTER wake")]
        [Tooltip("Teacher, all the other students, and their bags. " +
                 "They show during the falling-asleep intro, then vanish so the room is empty.")]
        [SerializeField] private GameObject[] classmatesAndTeacher;
        [Tooltip("Optional: a Light/object used only for the bright 'class in session' look.")]
        [SerializeField] private GameObject[] introOnlyObjects;

        [Header("Sleeping pose — head down on the desk")]
        [Tooltip("The CameraArm transform under the Player. Leave empty to skip the head-down effect.")]
        [SerializeField] private Transform cameraArm;
        [Tooltip("CameraArm Y height while head is resting on the desk.")]
        [SerializeField] private float sleepingHeadHeight = 0.85f;
        [Tooltip("CameraArm Y height when standing/sitting upright (normal eye level).")]
        [SerializeField] private float awakeHeadHeight = 1.4f;
        [Tooltip("Pitch (looking-down angle) while asleep on the desk.")]
        [SerializeField] private float sleepingPitch = 72f;

        [Header("UI - Fade")]
        [SerializeField] private Image fadeOverlay;

        [Header("UI - Dialogue")]
        [SerializeField] private CanvasGroup dialoguePanel;
        [SerializeField] private TextMeshProUGUI speakerName;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Button dialogueCloseButton;

        [Header("UI - Feedback")]
        [SerializeField] private CanvasGroup feedbackPanel;
        [SerializeField] private TextMeshProUGUI feedbackText;

        [Header("Timings")]
        [SerializeField] private float wakeUpFadeDuration  = 2f;
        [SerializeField] private float lightsOnDuration    = 1.5f;
        [SerializeField] private float feedbackDisplayTime = 2f;

        private PlayerInteraction playerInteraction;
        private FPSController fpsController;
        private bool _dialogueWaitingForInput = false;  // true after typing finishes on manual dialogues

        // -- Lifecycle ----------------------------------------------------------

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        private void Start()
        {
            playerInteraction = playerObject != null
                ? playerObject.GetComponentInChildren<PlayerInteraction>()
                : FindObjectOfType<PlayerInteraction>();

            fpsController = playerObject != null
                ? playerObject.GetComponent<FPSController>()
                : FindObjectOfType<FPSController>();

            // Intro starts BRIGHT — class is in session, classmates visible
            SetLights(true, instant: true);
            SetClassmatesVisible(true);
            HideDialogue(instant: true);
            HideFeedback(instant: true);

            // Start sitting UPRIGHT, head up, looking forward (awake pose)
            if (cameraArm != null)
            {
                Vector3 p = cameraArm.localPosition;
                p.y = awakeHeadHeight;
                cameraArm.localPosition = p;
                cameraArm.localRotation = Quaternion.identity;
            }

            // Fade overlay starts clear — player sees the full classroom first
            if (fadeOverlay != null)
            {
                Color c = Color.black; c.a = 0f;
                fadeOverlay.color = c;
                fadeOverlay.gameObject.SetActive(true);
            }

            if (dialogueCloseButton != null)
                dialogueCloseButton.onClick.AddListener(HideDialogue);

            // Hide dialogue panel at start — must be active in Editor but hidden in game
            HideDialogue(instant: true);

            StartCoroutine(RunWakeUpSequence());
        }

        // -- Wake-up sequence ---------------------------------------------------

        // Shows one line and WAITS until the player presses E/Space to continue.
        private IEnumerator ShowLine(string speaker, string message)
        {
            ShowDialogue(speaker, message, autoClose: false);
            yield return null;                                   // let the panel open
            yield return new WaitUntil(() => !IsDialogueOpen);   // player dismisses it
            yield return new WaitForSeconds(0.25f);              // small breath between lines
        }

        private IEnumerator RunWakeUpSequence()
        {
            State = FlowState.Sleeping;
            if (playerInteraction != null) playerInteraction.DisableInteraction();
            if (fpsController != null) fpsController.DisableMovement();

            yield return new WaitForSeconds(0.8f);

            // -- 1. Class in session (room is bright, classmates visible) --
            yield return ShowLine("Teacher",
                "...and that, class, is how a computer turns electricity into numbers. " +
                "Everything a computer does begins with just two states: on and off.");

            yield return ShowLine("You (thinking)",
                "First day of Computer Science... I can barely keep my eyes open. " +
                "Just five minutes. I'll only rest them for five minutes...");

            // -- 2. Falling asleep — head slowly droops down onto the desk --
            if (cameraArm != null)
            {
                cameraArm.DOLocalMoveY(sleepingHeadHeight, 3.5f).SetEase(Ease.InOutSine);
                cameraArm.DOLocalRotate(new Vector3(sleepingPitch, 0f, 0f), 3.5f)
                    .SetEase(Ease.InOutSine);
            }
            if (fadeOverlay != null) fadeOverlay.DOFade(1f, 3.5f);

            yield return new WaitForSeconds(3.6f);

            yield return ShowLine("...", "z z z . . .");

            // -- 3. While the screen is black: everyone leaves, lights go out --
            SetClassmatesVisible(false);
            SetLights(false, instant: true);

            yield return ShowLine("...", "Some time later...");

            // -- 4. Wake up with a jolt — fade in to an empty dark room --
            if (fadeOverlay != null)
                fadeOverlay.DOFade(0f, wakeUpFadeDuration).OnComplete(() =>
                    fadeOverlay.gameObject.SetActive(false));

            if (cameraArm != null)
            {
                cameraArm.DOLocalMoveY(awakeHeadHeight, 0.6f).SetEase(Ease.OutBack);
                cameraArm.DOLocalRotate(Vector3.zero, 0.6f).SetEase(Ease.OutBack);
            }

            yield return new WaitForSeconds(wakeUpFadeDuration);

            yield return ShowLine("You",
                "—Huh?! I fell asleep! Where is everyone? " +
                "The class is empty... and it's dark. How long was I out?");

            // -- 5. Hand control to the player --
            State = FlowState.DarkExplore;
            if (playerInteraction != null) playerInteraction.EnableInteraction();
            if (fpsController != null) fpsController.EnableMovement();
            if (portableComputer != null) portableComputer.WakeUp();
            ControlsHintUI.Instance?.ShowOnWake();

            ShowDialogue("You",
                "Okay — first things first. My backpack should be right here on my desk. " +
                "I'd better grab it before I go anywhere. Then I'll figure out that red glow " +
                "on the teacher's desk.",
                autoClose: true, autoCloseDelay: 5f);
        }

        // -- Puzzle solved ------------------------------------------------------

        // Wire this to BinaryPuzzleManager.OnPuzzleSolved in the Inspector
        public void OnPuzzleSolved()
        {
            State = FlowState.PuzzleSolved;
            StartCoroutine(PuzzleSolvedSequence());
        }

        private IEnumerator PuzzleSolvedSequence()
        {
            ShowDialogue(
                "ARIA",
                "That's it — the lock just clicked open. Nicely done.\n" +
                "Every number, letter, and picture inside a computer is stored exactly like " +
                "those eight switches: ones and zeros. You just spoke the computer's language.\n" +
                "The door's open. Take me with you — there's more ahead.",
                autoClose: true,
                autoCloseDelay: 6f
            );

            // Turn lights on with DOTween
            SetLights(true, instant: false);

            yield return new WaitForSeconds(5.5f);

            State = FlowState.Exit;
        }

        // -- Wrong answer feedback ----------------------------------------------

        public void ShowWrongAnswerFeedback()
        {
            StartCoroutine(ShowFeedbackCoroutine("Not quite! Check your binary conversion and try again."));
        }

        private IEnumerator ShowFeedbackCoroutine(string message)
        {
            if (feedbackText != null) feedbackText.text = message;
            if (feedbackPanel != null)
            {
                feedbackPanel.gameObject.SetActive(true);
                feedbackPanel.DOFade(1f, 0.3f);
            }
            yield return new WaitForSeconds(feedbackDisplayTime);
            HideFeedback(instant: false);
        }

        // -- Dialogue input (E or Space to skip/advance) -----------------------

        private void Update()
        {
            if (dialoguePanel == null || !dialoguePanel.gameObject.activeSelf) return;
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null) return;

            bool pressed = kb.eKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame;
            if (!pressed) return;

            if (TypewriterEffect.Instance != null && TypewriterEffect.Instance.IsTyping)
            {
                // First press: skip typing animation — show full text instantly
                TypewriterEffect.Instance.SkipToEnd();
            }
            else if (_dialogueWaitingForInput)
            {
                // Second press: close the dialogue
                _dialogueWaitingForInput = false;
                HideDialogue();
            }
        }

        // -- Dialogue helpers ---------------------------------------------------

        public void ShowDialogue(string speaker, string message, bool autoClose = false, float autoCloseDelay = 3f)
        {
            _dialogueWaitingForInput = false;

            // IMPORTANT: activate the panel BEFORE starting the typewriter coroutine
            // so DialogueText is active when StartCoroutine runs
            if (dialoguePanel != null)
            {
                dialoguePanel.DOKill();
                dialoguePanel.gameObject.SetActive(true);
                dialoguePanel.interactable   = true;
                dialoguePanel.blocksRaycasts = true;
                dialoguePanel.alpha = 0f;
                dialoguePanel.DOFade(1f, 0.25f);
            }

            if (speakerName != null) speakerName.text = speaker;

            // Now start typewriter (DialogueText is active)
            if (TypewriterEffect.Instance != null)
                TypewriterEffect.Instance.Play(message);
            else if (dialogueText != null)
                dialogueText.text = message;

            // Show the Continue prompt on EVERY dialogue now (player reads at their pace)
            if (dialogueCloseButton != null)
                dialogueCloseButton.gameObject.SetActive(true);

            if (autoClose)
                StartCoroutine(AutoCloseDialogue(autoCloseDelay));
            else
                StartCoroutine(WaitForTypingThenFlag());
        }

        // After typing finishes, flag that next E/Space press should close dialogue
        private IEnumerator WaitForTypingThenFlag()
        {
            yield return new WaitUntil(() =>
                TypewriterEffect.Instance == null || !TypewriterEffect.Instance.IsTyping);
            _dialogueWaitingForInput = true;
        }

        // Auto-close, but ONLY after the text has finished typing, and the player can
        // press E/Space to advance early. The reading delay starts AFTER typing.
        private IEnumerator AutoCloseDialogue(float delay)
        {
            yield return new WaitUntil(() =>
                TypewriterEffect.Instance == null || !TypewriterEffect.Instance.IsTyping);
            _dialogueWaitingForInput = true;            // E/Space can close it now
            yield return new WaitForSeconds(delay);
            if (_dialogueWaitingForInput)               // still open? auto-close as a fallback
            {
                _dialogueWaitingForInput = false;
                HideDialogue();
            }
        }

        public void HideDialogue(bool instant = false)
        {
            if (dialoguePanel == null) return;

            if (instant)
            {
                dialoguePanel.alpha = 0f;
                dialoguePanel.interactable   = false;
                dialoguePanel.blocksRaycasts = false;
                dialoguePanel.gameObject.SetActive(false);
            }
            else
            {
                dialoguePanel.DOFade(0f, 0.3f).OnComplete(() =>
                {
                    dialoguePanel.interactable   = false;
                    dialoguePanel.blocksRaycasts = false;
                    dialoguePanel.gameObject.SetActive(false);
                });
            }
        }

        // Overload for Button.onClick binding (no parameters)
        private void HideDialogue() => HideDialogue(instant: false);

        // -- Intro classmates ---------------------------------------------------

        // Called at the ENDING — the player wakes in the real classroom, so the
        // teacher and classmates come back and the lights are on.
        public void RestoreClassroomForEnding()
        {
            SetClassmatesVisible(true);
            SetLights(true, instant: true);
        }

        // Shows/hides the teacher, other students, and their bags.
        // Visible during the falling-asleep intro, hidden once the player wakes.
        private void SetClassmatesVisible(bool visible)
        {
            if (classmatesAndTeacher != null)
                foreach (var go in classmatesAndTeacher)
                    if (go != null) go.SetActive(visible);

            if (introOnlyObjects != null)
                foreach (var go in introOnlyObjects)
                    if (go != null) go.SetActive(visible);
        }

        // -- Light helpers ------------------------------------------------------

        private bool _lightsOn = false;
        public bool AreLightsOn => _lightsOn;

        private void SetLights(bool on, bool instant)
        {
            _lightsOn = on;
            if (classroomLights == null) return;
            foreach (var light in classroomLights)
            {
                if (light == null) continue;
                light.enabled = on;   // reliable on/off — works no matter how lights are set up
            }
        }

        // Called by the LightSwitch (the 1-bit teaching lesson). Toggles the room lights.
        public void PlayerToggleLights()
        {
            SetLights(!_lightsOn, instant: true);
        }

        private void HideFeedback(bool instant)
        {
            if (feedbackPanel == null) return;
            if (instant)
            {
                feedbackPanel.alpha = 0f;
                feedbackPanel.gameObject.SetActive(false);
            }
            else
            {
                feedbackPanel.DOFade(0f, 0.3f).OnComplete(() =>
                    feedbackPanel.gameObject.SetActive(false)
                );
            }
        }
    }
}
