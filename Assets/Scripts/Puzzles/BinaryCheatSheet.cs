using UnityEngine;
using TMPro;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // A reference panel the player can open if they forget how binary works.
    // Hook a "Binary Help" button in the Pause menu to Toggle().
    //
    // UI SETUP:
    //   HelpPanel (CanvasGroup)
    //   --- HelpText (TMP)  <- leave empty; this script fills it in
    //
    // The text is written here so it always matches the lesson ARIA teaches.

    public class BinaryCheatSheet : MonoBehaviour
    {
        public static BinaryCheatSheet Instance { get; private set; }

        [SerializeField] private CanvasGroup helpPanel;
        [SerializeField] private TextMeshProUGUI helpText;

        private bool _open = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        private void Start()
        {
            if (helpText != null) helpText.text = BuildText();
            HideInstant();
        }

        private string BuildText()
        {
            string code = BinaryPuzzleManager.Instance != null
                ? BinaryPuzzleManager.Instance.TargetValue.ToString()
                : "?";

            return
                "<b>HOW BINARY WORKS</b>\n\n" +
                "Each switch is one BIT:  ON = 1,  OFF = 0.\n\n" +
                "Eight switches make a BYTE. Each position has a value:\n\n" +
                "   128   64   32   16   8   4   2   1\n\n" +
                "Add up the values of every switch set to 1.\n\n" +
                "<b>Example:</b>  to make 13\n" +
                "   13 = 8 + 4 + 1\n" +
                "   so the switches are: 0 0 0 0 1 1 0 1\n\n" +
                "<b>Method:</b> take your number, subtract the biggest\n" +
                "value that fits, repeat until you reach 0.\n\n" +
                $"<b>Your door code is: {code}</b>";
        }

        // Hook this to a Pause-menu "Binary Help" button
        public void Toggle()
        {
            if (_open) Hide(); else Show();
        }

        public void Show()
        {
            if (helpPanel == null) return;
            // Refresh in case the code wasn't ready at Start
            if (helpText != null) helpText.text = BuildText();
            _open = true;
            helpPanel.DOKill();
            helpPanel.gameObject.SetActive(true);
            helpPanel.alpha = 0f;
            helpPanel.interactable = true;
            helpPanel.blocksRaycasts = true;
            helpPanel.DOFade(1f, 0.25f).SetUpdate(true);
        }

        public void Hide()
        {
            _open = false;
            if (helpPanel == null) return;
            helpPanel.DOKill();
            helpPanel.interactable = false;
            helpPanel.blocksRaycasts = false;
            helpPanel.DOFade(0f, 0.25f).SetUpdate(true)
                .OnComplete(() => helpPanel.gameObject.SetActive(false));
        }

        private void HideInstant()
        {
            if (helpPanel == null) return;
            helpPanel.alpha = 0f;
            helpPanel.interactable = false;
            helpPanel.blocksRaycasts = false;
            helpPanel.gameObject.SetActive(false);
        }
    }
}
