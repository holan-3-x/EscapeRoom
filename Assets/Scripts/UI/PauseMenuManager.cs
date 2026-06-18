using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // In-game pause menu. Press ESC to open/close.
    // Place this script on an empty GameObject in every gameplay scene.
    //
    // HIERARCHY inside your gameplay Canvas:
    //   PausePanel (CanvasGroup)
    //   --- Title: "PAUSED"
    //   --- Button: Resume
    //   --- Button: Save & Exit
    //   --- Button: Settings  -> opens PauseSettingsPanel
    //   --- Button: Main Menu
    //
    //   PauseSettingsPanel (CanvasGroup) — same sliders as main menu
    //   --- Slider: Volume
    //   --- Slider: Mouse Sensitivity
    //   --- Button: Back

    public class PauseMenuManager : MonoBehaviour
    {
        public static PauseMenuManager Instance { get; private set; }

        [Header("Panels")]
        [SerializeField] private CanvasGroup pausePanel;
        [SerializeField] private CanvasGroup pauseSettingsPanel;

        [Header("Pause Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button saveExitButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Settings inside Pause")]
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private Button settingsBackButton;

        [Header("Fade overlay (same one from ClassroomSceneFlow is fine)")]
        [SerializeField] private Image fadeOverlay;

        private bool _isPaused = false;

        // So the inventory knows not to open while paused
        public static bool IsPaused { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        private void Start()
        {
            ForceHide(pausePanel);
            ForceHide(pauseSettingsPanel);

            resumeButton?   .onClick.AddListener(Resume);
            saveExitButton? .onClick.AddListener(SaveAndExit);
            settingsButton? .onClick.AddListener(OpenPauseSettings);
            mainMenuButton? .onClick.AddListener(GoToMainMenu);
            settingsBackButton?.onClick.AddListener(ClosePauseSettings);

            if (GameManager.Instance != null)
            {
                if (volumeSlider      != null) volumeSlider.value      = GameManager.Instance.MasterVolume;
                if (sensitivitySlider != null) sensitivitySlider.value = GameManager.Instance.MouseSensitivity;
            }

            volumeSlider?     .onValueChanged.AddListener(v => {
                AudioListener.volume = v;
                if (GameManager.Instance != null) GameManager.Instance.MasterVolume = v;
            });
            sensitivitySlider?.onValueChanged.AddListener(v => {
                if (GameManager.Instance != null) GameManager.Instance.MouseSensitivity = v;
                FPSController.Instance?.UpdateSensitivity(v);
            });
        }

        private void Update()
        {
            // While a minigame is open, let IT handle ESC (don't open pause)
            if (LogicGateTerminal.MinigameIsOpen || MinigameLauncher.AnyOpen) return;
            // While the backpack is open, ESC is handled by the inventory (closes the bag)
            if (InventorySystem.IsOpen) return;

            if (UnityEngine.InputSystem.Keyboard.current == null) return;
            if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (_isPaused) Resume(); else Pause();
            }
        }

        // -- Actions -----------------------------------------------------------

        public void Pause()
        {
            _isPaused = true;
            IsPaused  = true;
            Time.timeScale = 0f;
            // Show cursor immediately so player can click buttons
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
            FPSController.Instance?.DisableMovement();
            Show(pausePanel);
        }

        public void Resume()
        {
            _isPaused = false;
            IsPaused  = false;
            Time.timeScale = 1f;
            Hide(pausePanel);
            Hide(pauseSettingsPanel);
            // Re-lock cursor after a tiny delay so ESC release doesn't re-trigger
            Invoke(nameof(RelockCursor), 0.05f);
        }

        private void RelockCursor()
        {
            FPSController.Instance?.EnableMovement();
        }

        private void SaveAndExit()
        {
            Time.timeScale = 1f;
            SaveSystem.Save();
            FadeAndLoad("MainMenu");
        }

        private void GoToMainMenu()
        {
            Time.timeScale = 1f;
            FadeAndLoad("MainMenu");
        }

        private void OpenPauseSettings()
        {
            Hide(pausePanel);
            Show(pauseSettingsPanel);
        }

        private void ClosePauseSettings()
        {
            if (GameManager.Instance != null) GameManager.Instance.SaveSettings();
            Hide(pauseSettingsPanel);
            Show(pausePanel);
        }

        // -- Helpers -----------------------------------------------------------

        private void Show(CanvasGroup g)
        {
            if (g == null) return;
            g.DOKill();
            g.gameObject.SetActive(true);
            g.interactable   = true;   // clickable immediately, not after fade
            g.blocksRaycasts = true;
            g.alpha = 0f;
            g.DOFade(1f, 0.2f).SetUpdate(true);
        }

        private void Hide(CanvasGroup g)
        {
            if (g == null) return;
            g.DOKill();
            g.interactable = false; g.blocksRaycasts = false;
            g.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(() => g.gameObject.SetActive(false));
        }

        private void ForceHide(CanvasGroup g)
        {
            if (g == null) return;
            g.alpha = 0f; g.interactable = false;
            g.blocksRaycasts = false; g.gameObject.SetActive(false);
        }

        private void FadeAndLoad(string scene)
        {
            if (fadeOverlay != null)
            {
                fadeOverlay.gameObject.SetActive(true);
                fadeOverlay.DOFade(1f, 0.5f).SetUpdate(true)
                    .OnComplete(() => SceneManager.LoadScene(scene));
            }
            else SceneManager.LoadScene(scene);
        }
    }
}
