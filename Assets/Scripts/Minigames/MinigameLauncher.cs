using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace SYSTEMESCAPE
{
    // Generic launcher for a 2D minigame embedded in the 3D world (network pipes,
    // block game, etc.). Put it on a 3D screen/terminal/server object the player
    // presses E on. It opens the minigame's UI, freezes the player, and shows ARIA's
    // intro. When the friend's game is solved, IT calls CompleteMinigame() (wire that
    // to the game's win event), which closes it and fires OnSolved.
    //
    // SETUP (3D object, layer = Interactable, Collider):
    //   Minigame Root  = the minigame Canvas/panel (starts DISABLED)
    //   Require Flag    = optional gate (e.g. only after the PC is built)
    //   Intro Line      = what ARIA says before it opens
    //   OnSolved        = wire to open the door / next step
    //   (In the friend's game, call this object's CompleteMinigame() on win.)

    public class MinigameLauncher : MonoBehaviour, IInteractable
    {
        // True while ANY launcher's minigame is open (pause/inventory check this)
        public static bool AnyOpen { get; private set; }

        // Clears the static flag when a new gameplay scene loads (builds keep statics alive)
        public static void ForceReset() { AnyOpen = false; }

        [Header("The minigame UI to open (starts disabled)")]
        [SerializeField] private GameObject minigameRoot;

        [Header("Gating (optional)")]
        [Tooltip("If set, this launcher only works after RequireCompleted() is called " +
                 "(e.g. wired from PCBuildStation.OnPCPoweredOn).")]
        [SerializeField] private bool startsLocked = false;
        [TextArea] [SerializeField] private string lockedLine =
            "This won't respond yet — something else needs to happen first.";

        [Header("ARIA intro (first open)")]
        [TextArea] [SerializeField] private string introLine =
            "Let me bring up the interface. Solve it and we move forward.";
        [SerializeField] private string introSpeaker = "ARIA";

        [Header("Prompt")]
        [SerializeField] private string prompt = "[E] Use";

        [Header("Pause game time while open?")]
        [SerializeField] private bool freezeTime = true;

        [Header("Fires when the minigame is solved")]
        public UnityEvent OnSolved;
        [Tooltip("Optional line ARIA says the moment the minigame is solved " +
                 "(e.g. 'Network's back online, the door just unlocked').")]
        [TextArea] [SerializeField] private string ariaSolvedLine = "";

        [Header("Optional — extra setup/teardown")]
        [Tooltip("Called when the minigame opens. For the block game, wire this to its " +
                 "MinigameBase.StartMinigame() so its frozen-time physics initialise.")]
        public UnityEvent OnOpened;
        [Tooltip("Called when it closes (e.g. block game's CloseMinigame()).")]
        public UnityEvent OnClosed;

        private bool _locked;
        private bool _introDone = false;
        private bool _isOpen = false;
        private bool _solved = false;

        private void Awake()
        {
            _locked = startsLocked;
        }

        private void Start()
        {
            if (minigameRoot != null) minigameRoot.SetActive(false);
        }

        // Wire this to e.g. PCBuildStation.OnPCPoweredOn to unlock the server terminal
        public void Unlock() => _locked = false;

        public string GetPrompt()
        {
            if (_isOpen || _solved) return "";
            return prompt;
        }

        public void Interact()
        {
            if (_isOpen || _solved || minigameRoot == null) return;

            if (_locked)
            {
                ClassroomSceneFlow.Instance?.ShowDialogue("ARIA", lockedLine,
                    autoClose: true, autoCloseDelay: 3.5f);
                return;
            }

            // ARIA intro first (plays before time freezes so dialogue animates)
            if (!_introDone)
            {
                _introDone = true;
                if (!string.IsNullOrEmpty(introLine))
                {
                    ClassroomSceneFlow.Instance?.ShowDialogue(introSpeaker, introLine,
                        autoClose: false);
                    return;
                }
            }

            Open();
        }

        private void Open()
        {
            _isOpen = true;
            AnyOpen = true;
            minigameRoot.SetActive(true);
            FPSController.Instance?.DisableMovement();
            FindObjectOfType<PlayerInteraction>()?.DisableInteraction();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
            if (freezeTime) Time.timeScale = 0f;
            OnOpened?.Invoke();   // e.g. block game's StartMinigame() (sets up its physics)
        }

        private void Update()
        {
            if (!_isOpen) return;
            var kb = Keyboard.current;
            if (kb != null && (kb.escapeKey.wasPressedThisFrame || kb.qKey.wasPressedThisFrame))
                Close();
        }

        // Player backed out (can re-open to resume)
        private void Close()
        {
            _isOpen = false;
            AnyOpen = false;
            if (minigameRoot != null) minigameRoot.SetActive(false);
            if (freezeTime) Time.timeScale = 1f;
            FPSController.Instance?.EnableMovement();
            FindObjectOfType<PlayerInteraction>()?.EnableInteraction();
            OnClosed?.Invoke();   // e.g. block game's CloseMinigame()
        }

        // CALL THIS FROM THE FRIEND'S GAME WHEN IT'S SOLVED (wire to its win event)
        public void CompleteMinigame()
        {
            if (_solved) return;
            _solved = true;
            _isOpen = false;
            AnyOpen = false;

            if (minigameRoot != null) minigameRoot.SetActive(false);
            if (freezeTime) Time.timeScale = 1f;
            FPSController.Instance?.EnableMovement();
            FindObjectOfType<PlayerInteraction>()?.EnableInteraction();
            OnClosed?.Invoke();   // e.g. block game's CloseMinigame()

            SoundManager.Instance?.PlaySolve();

            // ARIA reacts to the win, so the player gets clear feedback
            if (!string.IsNullOrEmpty(ariaSolvedLine))
                ClassroomSceneFlow.Instance?.ShowDialogue("ARIA", ariaSolvedLine,
                    autoClose: true, autoCloseDelay: 4f);

            OnSolved?.Invoke();
        }
    }
}
