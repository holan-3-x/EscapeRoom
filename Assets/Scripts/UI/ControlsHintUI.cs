using UnityEngine;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // Shows a "how to play" controls panel.
    // - Pops up automatically a moment after the player wakes (first time only).
    // - Can be reopened any time from the Pause menu (call Show()).
    //
    // UI SETUP (inside your Canvas):
    //   ControlsPanel (CanvasGroup)
    //   --- Title TMP: "CONTROLS"
    //   --- an image/text showing:  W A S D = Move   Mouse = Look
    //   |      Shift = Run   E = Interact   TAB = Backpack   ESC = Pause
    //   --- CloseButton (optional) — or it auto-hides after a few seconds
    //
    // Assign ControlsPanel below. Hook CloseButton's OnClick to Hide() if you add one.

    public class ControlsHintUI : MonoBehaviour
    {
        public static ControlsHintUI Instance { get; private set; }

        [SerializeField] private CanvasGroup controlsPanel;
        [SerializeField] private float autoShowDelay   = 1.5f;  // after waking
        [SerializeField] private float autoHideAfter   = 6f;    // 0 = stay until closed
        [SerializeField] private bool  showOnWakeOnce  = true;

        private bool _shownOnce = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        private void Start()
        {
            HideInstant();
        }

        // Called by ClassroomSceneFlow when the player gains control
        public void ShowOnWake()
        {
            if (!showOnWakeOnce || _shownOnce) return;
            _shownOnce = true;
            DOVirtual.DelayedCall(autoShowDelay, () => Show(autoHide: true));
        }

        public void Show(bool autoHide = false)
        {
            if (controlsPanel == null) return;
            controlsPanel.DOKill();
            controlsPanel.gameObject.SetActive(true);
            controlsPanel.alpha = 0f;
            controlsPanel.interactable   = true;
            controlsPanel.blocksRaycasts = true;
            controlsPanel.DOFade(1f, 0.3f).SetUpdate(true);

            if (autoHide && autoHideAfter > 0f)
                DOVirtual.DelayedCall(autoHideAfter, Hide).SetUpdate(true);
        }

        public void Hide()
        {
            if (controlsPanel == null) return;
            controlsPanel.DOKill();
            controlsPanel.interactable   = false;
            controlsPanel.blocksRaycasts = false;
            controlsPanel.DOFade(0f, 0.3f).SetUpdate(true)
                .OnComplete(() => controlsPanel.gameObject.SetActive(false));
        }

        private void HideInstant()
        {
            if (controlsPanel == null) return;
            controlsPanel.alpha = 0f;
            controlsPanel.interactable   = false;
            controlsPanel.blocksRaycasts = false;
            controlsPanel.gameObject.SetActive(false);
        }
    }
}
