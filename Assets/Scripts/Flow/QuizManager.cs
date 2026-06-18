using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    [System.Serializable]
    public class QuizQuestion
    {
        [TextArea] public string question;
        public string[] options = new string[4];
        [Tooltip("Index (0-3) of the correct option.")]
        public int correctIndex;
    }

    // The ending: a noise wakes the student — it was all a dream — and the teacher
    // asks ONE random CS question. Answer correctly to win.
    //
    // SETUP: build a Canvas with:
    //   WakeOverlay (full-screen black Image)
    //   QuizPanel (CanvasGroup): QuestionText (TMP) + 4 Buttons (each with a TMP label)
    //   ResultText (TMP)
    // Then call StartEnding() from your final trigger (e.g. bottom of the stairs).

    public class QuizManager : MonoBehaviour
    {
        public static QuizManager Instance { get; private set; }

        [Header("Questions (one is picked at random)")]
        [SerializeField] private List<QuizQuestion> questions = new();

        [Header("UI")]
        [SerializeField] private Image       wakeOverlay;
        [SerializeField] private CanvasGroup quizPanel;
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private Button[]    optionButtons = new Button[4];
        [SerializeField] private TextMeshProUGUI resultText;

        [Header("Result buttons (appear after answering)")]
        [SerializeField] private Button retryButton;     // shown on a wrong answer (Game Over)
        [SerializeField] private Button mainMenuButton;  // back to main menu

        [Header("Ending atmosphere")]
        [Tooltip("Move the player back to their desk for the wake-up. Optional.")]
        [SerializeField] private Transform deskReturnPoint;
        [SerializeField] private GameObject playerObject;
        [Tooltip("The player's CameraArm — reset so the view faces forward, not the dream angle.")]
        [SerializeField] private Transform cameraArm;
        [Tooltip("The player body's Animator — reset to idle so the walk animation stops.")]
        [SerializeField] private Animator playerBodyAnimator;
        [Tooltip("The teacher's Animator, to play a 'pointing' animation.")]
        [SerializeField] private Animator teacherAnimator;
        [SerializeField] private string teacherPointTrigger = "Point";
        [SerializeField] private bool stopBackgroundSound = true;

        private QuizQuestion _current;
        private bool _answered = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        private void Start()
        {
            if (quizPanel != null) { quizPanel.alpha = 0f; quizPanel.gameObject.SetActive(false); }
            if (resultText != null) resultText.gameObject.SetActive(false);
            if (wakeOverlay != null) { var c = wakeOverlay.color; c.a = 0f; wakeOverlay.color = c; wakeOverlay.gameObject.SetActive(false); }

            for (int i = 0; i < optionButtons.Length; i++)
            {
                int idx = i;
                if (optionButtons[i] != null)
                    optionButtons[i].onClick.AddListener(() => OnAnswer(idx));
            }

            if (retryButton != null)
            {
                retryButton.gameObject.SetActive(false);
                retryButton.onClick.AddListener(() => { retryButton.gameObject.SetActive(false); ShowQuestion(); });
            }
            if (mainMenuButton != null)
            {
                mainMenuButton.gameObject.SetActive(false);
                mainMenuButton.onClick.AddListener(() =>
                    UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"));
            }
        }

        // Call this from the final trigger (bottom of the stairs)
        public void StartEnding()
        {
            StartCoroutine(EndingRoutine());
        }

        private IEnumerator EndingRoutine()
        {
            FPSController.Instance?.DisableMovement();

            // Fade to white/black (he passes out / the dream ends)
            if (wakeOverlay != null)
            {
                wakeOverlay.gameObject.SetActive(true);
                yield return wakeOverlay.DOFade(1f, 1.5f).WaitForCompletion();
            }

            ClassroomSceneFlow.Instance?.ShowDialogue("...",
                "You step through the final door — and a loud BANG echoes behind you. " +
                "Everything goes white...",
                autoClose: true, autoCloseDelay: 3.5f);
            yield return new WaitForSeconds(4f);

            // The dream ends: stop the music/ambient and return the player to their desk
            if (stopBackgroundSound) SoundManager.Instance?.StopBackground();

            // Real classroom again — bring the teacher + classmates back, lights on
            ClassroomSceneFlow.Instance?.RestoreClassroomForEnding();

            // Freeze the walk animation (so the body isn't mid-stride at the desk)
            if (playerBodyAnimator != null)
            {
                if (HasParam(playerBodyAnimator, "X_Velocity")) playerBodyAnimator.SetFloat("X_Velocity", 0f);
                if (HasParam(playerBodyAnimator, "Y_Velocity")) playerBodyAnimator.SetFloat("Y_Velocity", 0f);
            }

            if (deskReturnPoint != null && playerObject != null)
            {
                var cc = playerObject.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;          // disable before teleport
                playerObject.transform.SetPositionAndRotation(deskReturnPoint.position, deskReturnPoint.rotation);
                if (cc != null) cc.enabled = true;
            }

            // Reset the camera so the player looks straight ahead at the teacher, not the dream angle
            if (cameraArm != null) cameraArm.localRotation = Quaternion.identity;

            // Fade back in — we're in the classroom again
            if (wakeOverlay != null)
                yield return wakeOverlay.DOFade(0f, 1.2f).WaitForCompletion();

            // Teacher points and speaks
            if (teacherAnimator != null && !string.IsNullOrEmpty(teacherPointTrigger))
                teacherAnimator.SetTrigger(teacherPointTrigger);

            ClassroomSceneFlow.Instance?.ShowDialogue("Teacher",
                "—and THAT'S why you don't sleep in my class! Wake up!",
                autoClose: true, autoCloseDelay: 3.5f);
            yield return new WaitForSeconds(4f);

            ClassroomSceneFlow.Instance?.ShowDialogue("You",
                "Wha—? It was... all a dream? I'm still at my desk?",
                autoClose: true, autoCloseDelay: 3.5f);
            yield return new WaitForSeconds(4f);

            ClassroomSceneFlow.Instance?.ShowDialogue("Teacher",
                "Since you napped through the whole lesson, let's see if any of it sank in. " +
                "Answer me this one question — correctly — and I'll let it slide.",
                autoClose: true, autoCloseDelay: 5f);
            yield return new WaitForSeconds(5.5f);

            ShowQuestion();
        }

        private static bool HasParam(Animator anim, string name)
        {
            foreach (var p in anim.parameters) if (p.name == name) return true;
            return false;
        }

        private void ShowQuestion()
        {
            if (questions == null || questions.Count == 0) return;

            _answered = false;
            _current = questions[Random.Range(0, questions.Count)];

            if (questionText != null) questionText.text = _current.question;
            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (optionButtons[i] == null) continue;
                bool has = i < _current.options.Length;
                optionButtons[i].gameObject.SetActive(has);
                if (has)
                {
                    var label = optionButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (label != null) label.text = _current.options[i];
                    optionButtons[i].interactable = true;
                }
            }

            if (quizPanel != null)
            {
                quizPanel.gameObject.SetActive(true);
                quizPanel.alpha = 0f;
                quizPanel.interactable = true;
                quizPanel.blocksRaycasts = true;
                quizPanel.DOFade(1f, 0.4f);
            }

            // Make sure the cursor is usable for clicking answers
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnAnswer(int index)
        {
            if (_answered) return;
            _answered = true;

            foreach (var b in optionButtons) if (b != null) b.interactable = false;

            bool correct = index == _current.correctIndex;
            if (resultText != null)
            {
                resultText.gameObject.SetActive(true);
                resultText.text = correct
                    ? "YOU WIN!\nThe teacher nods. You actually learned something.\nThanks for playing!"
                    : "GAME OVER\nThat's not right... the teacher sighs. Want to try again?";
                resultText.color = correct ? Color.green : new Color(1f, 0.4f, 0.4f);
                resultText.transform.localScale = Vector3.zero;
                resultText.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
            }

            if (correct)
            {
                SoundManager.Instance?.PlaySolve();
                // WIN — offer return to menu (or credits)
                if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(true);
            }
            else
            {
                SoundManager.Instance?.PlayWrong();
                // GAME OVER — let them retry the question or quit to menu
                if (retryButton != null)    retryButton.gameObject.SetActive(true);
                if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(true);
            }
        }
    }
}
