using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // Attach to an empty GameObject called "MainMenuManager" in the MainMenu scene.
    //
    // PANEL FLOW:
    //   MainPanel -> New Game -> CharSelectPanel (gender pick) -> loads Level0_Classroom
    //   MainPanel -> Continue -> loads saved scene
    //   MainPanel -> Settings -> SettingsPanel -> Back -> MainPanel
    //   MainPanel -> Credits  -> CreditsPanel  -> Back -> MainPanel

    public class MainMenuManager : MonoBehaviour
    {
        [Header("Scene to load when game starts")]
        [SerializeField] private string gameplayScene = "Level0_Classroom";

        [Header("Panels — all CanvasGroup components")]
        [SerializeField] private CanvasGroup mainPanel;
        [SerializeField] private CanvasGroup charSelectPanel;   // gender selection panel
        [SerializeField] private CanvasGroup settingsPanel;
        [SerializeField] private CanvasGroup creditsPanel;

        [Header("Main Panel Buttons")]
        [SerializeField] private Button continueButton;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;

        [Header("Char Select Buttons")]
        [SerializeField] private Button maleButton;
        [SerializeField] private Button femaleButton;
        [SerializeField] private Button charBackButton;

        [Header("Settings")]
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private Button settingsBackButton;

        [Header("Credits")]
        [SerializeField] private Button creditsBackButton;

        [Header("Full-screen black Image for fading")]
        [SerializeField] private Image fadeOverlay;

        private CanvasGroup _current;

        // -- Start -------------------------------------------------------------

        private void Start()
        {
            // Start fully black, then fade in
            SetOverlay(1f);
            DOVirtual.DelayedCall(0.1f, () => FadeOverlay(0f, 0.8f));

            // Hide everything except main panel
            ForceShow(mainPanel);
            ForceHide(charSelectPanel);
            ForceHide(settingsPanel);
            ForceHide(creditsPanel);
            _current = mainPanel;

            // Continue is hidden: the game is a single-session escape room, so it only
            // offers New Game. (The minimal save did not restore position/puzzle state,
            // which made "Continue" feel like a restart, so we removed it.)
            if (continueButton != null)
                continueButton.gameObject.SetActive(false);

            // Populate settings sliders
            if (GameManager.Instance != null)
            {
                if (volumeSlider      != null) volumeSlider.value      = GameManager.Instance.MasterVolume;
                if (sensitivitySlider != null) sensitivitySlider.value = GameManager.Instance.MouseSensitivity;
            }

            // Wire all buttons
            newGameButton? .onClick.AddListener(OnNewGame);
            continueButton?.onClick.AddListener(OnContinue);
            settingsButton?.onClick.AddListener(() => SwitchTo(settingsPanel));
            creditsButton? .onClick.AddListener(() => SwitchTo(creditsPanel));
            quitButton?    .onClick.AddListener(OnQuit);

            maleButton?    .onClick.AddListener(() => StartGame(Gender.Male));
            femaleButton?  .onClick.AddListener(() => StartGame(Gender.Female));
            charBackButton?.onClick.AddListener(() => SwitchTo(mainPanel));

            settingsBackButton?.onClick.AddListener(OnSettingsBack);
            creditsBackButton? .onClick.AddListener(() => SwitchTo(mainPanel));

            volumeSlider?     .onValueChanged.AddListener(OnVolumeChanged);
            sensitivitySlider?.onValueChanged.AddListener(OnSensChanged);
        }

        // -- Button actions ----------------------------------------------------

        private void OnNewGame()
        {
            // If a character-select panel exists, show it. Otherwise start the game directly.
            if (charSelectPanel != null && (maleButton != null || femaleButton != null))
                SwitchTo(charSelectPanel);
            else
                StartGame(GameManager.Instance != null ? GameManager.Instance.SelectedGender : Gender.Male);
        }

        private void StartGame(Gender gender)
        {
            SaveSystem.DeleteSave();   // New Game always wipes the old save
            if (GameManager.Instance != null) GameManager.Instance.SelectedGender = gender;
            FadeOverlay(1f, 0.6f, () => SceneManager.LoadScene(gameplayScene));
        }

        private void OnContinue()
        {
            SaveSystem.Load();
            string scene = SaveSystem.GetSavedScene();
            if (string.IsNullOrEmpty(scene)) scene = gameplayScene;
            FadeOverlay(1f, 0.6f, () => SceneManager.LoadScene(scene));
        }

        private void OnSettingsBack()
        {
            if (GameManager.Instance != null) GameManager.Instance.SaveSettings();
            SwitchTo(mainPanel);
        }

        private void OnVolumeChanged(float v)
        {
            AudioListener.volume = v;
            if (GameManager.Instance != null) GameManager.Instance.MasterVolume = v;
        }

        private void OnSensChanged(float v)
        {
            if (GameManager.Instance != null) GameManager.Instance.MouseSensitivity = v;
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // -- Panel switching (DOTween-safe) ------------------------------------

        private void SwitchTo(CanvasGroup next)
        {
            if (next == _current) return;
            CanvasGroup prev = _current;
            _current = next;

            // Kill any running tweens on both panels before starting new ones
            prev?.DOKill();
            next?.DOKill();

            // Fade out current
            if (prev != null)
            {
                prev.interactable   = false;
                prev.blocksRaycasts = false;
                prev.DOFade(0f, 0.25f).OnComplete(() =>
                {
                    if (prev != null) prev.gameObject.SetActive(false);
                });
            }

            // Fade in next
            if (next != null)
            {
                next.gameObject.SetActive(true);
                next.alpha = 0f;
                next.DOFade(1f, 0.25f).OnComplete(() =>
                {
                    next.interactable   = true;
                    next.blocksRaycasts = true;
                });
            }
        }

        // -- Helpers -----------------------------------------------------------

        private void ForceShow(CanvasGroup g)
        {
            if (g == null) return;
            g.gameObject.SetActive(true);
            g.alpha          = 1f;
            g.interactable   = true;
            g.blocksRaycasts = true;
        }

        private void ForceHide(CanvasGroup g)
        {
            if (g == null) return;
            g.gameObject.SetActive(false);
            g.alpha          = 0f;
            g.interactable   = false;
            g.blocksRaycasts = false;
        }

        private void SetOverlay(float alpha)
        {
            if (fadeOverlay == null) return;
            fadeOverlay.gameObject.SetActive(true);
            Color c = fadeOverlay.color; c.a = alpha;
            fadeOverlay.color = c;
        }

        private void FadeOverlay(float to, float dur, System.Action onDone = null)
        {
            if (fadeOverlay == null) { onDone?.Invoke(); return; }
            fadeOverlay.gameObject.SetActive(true);
            fadeOverlay.DOKill();
            fadeOverlay.DOFade(to, dur).OnComplete(() =>
            {
                if (to <= 0f) fadeOverlay.gameObject.SetActive(false);
                onDone?.Invoke();
            });
        }
    }
}
