using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace SYSTEMESCAPE
{
    // This UI panel sits on screen while the player is solving the binary puzzle.
    // It shows:
    //   - The decimal number to convert
    //   - 8 bit slots (128, 64, 32, 16, 8, 4, 2, 1) that light up when switches are flipped
    //   - A running total so the player can see their current value
    //   - A HINT button that shows the answer
    //
    // HOW TO SET UP (Canvas children):
    //
    //   BinaryPanel (CanvasGroup) — shown when puzzle is active
    //   --- TargetText      (TMP) — "Convert: 170"
    //   --- BitRow          (Horizontal Layout Group)
    //   |   --- BitSlot_7   (Image + child TMP "128")   <- bit 7, value 128
    //   |   --- BitSlot_6   (Image + child TMP "64")
    //   |   --- BitSlot_5   (Image + child TMP "32")
    //   |   --- BitSlot_4   (Image + child TMP "16")
    //   |   --- BitSlot_3   (Image + child TMP "8")
    //   |   --- BitSlot_2   (Image + child TMP "4")
    //   |   --- BitSlot_1   (Image + child TMP "2")
    //   |   --- BitSlot_0   (Image + child TMP "1")    <- bit 0, value 1
    //   --- CurrentValueText (TMP) — "Current: 0"
    //   --- HintButton      (Button) — shows binary answer
    //   --- HintText        (TMP) — hidden until hint pressed

    public class BinaryTeachingUI : MonoBehaviour
    {
        public static BinaryTeachingUI Instance { get; private set; }

        [Header("Panel")]
        [SerializeField] private CanvasGroup binaryPanel;

        [Header("Text")]
        [SerializeField] private TextMeshProUGUI targetText;
        [SerializeField] private TextMeshProUGUI currentValueText;
        [SerializeField] private TextMeshProUGUI hintText;

        [Header("Bit Slots — index 0 = rightmost (value 1), index 7 = leftmost (value 128)")]
        [SerializeField] private Image[] bitSlots = new Image[8];  // drag slot images here

        [Header("Hint Button")]
        [SerializeField] private Button hintButton;

        [Header("Colors")]
        [SerializeField] private Color bitOffColor = new Color(0.2f, 0.2f, 0.2f);
        [SerializeField] private Color bitOnColor  = new Color(0.2f, 0.9f, 0.3f);

        private bool _isVisible = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        private void Start()
        {
            HidePanel(instant: true);
            if (hintText   != null) hintText.gameObject.SetActive(false);
            if (hintButton != null) hintButton.onClick.AddListener(ShowHint);

            // Set bit value labels (128, 64, 32 ... 1)
            // The bitSlots array goes from bit7 (index 0 in array) to bit0 (index 7 in array)
            // so label values are 128, 64, 32, 16, 8, 4, 2, 1
            int[] values = { 128, 64, 32, 16, 8, 4, 2, 1 };
            for (int i = 0; i < bitSlots.Length; i++)
            {
                if (bitSlots[i] == null) continue;
                bitSlots[i].color = bitOffColor;
                var label = bitSlots[i].GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = values[i].ToString();
            }
        }

        // Called by BinaryPuzzleManager when the puzzle starts / player is near switches
        public void ShowPanel()
        {
            if (_isVisible || BinaryPuzzleManager.Instance == null) return;
            _isVisible = true;

            int target = BinaryPuzzleManager.Instance.TargetValue;
            if (targetText != null)
                targetText.text = $"Convert to Binary: <b>{target}</b>";

            if (currentValueText != null)
                currentValueText.text = "Your value: 0";

            binaryPanel.gameObject.SetActive(true);
            binaryPanel.DOFade(1f, 0.4f);
        }

        public void HidePanel(bool instant = false)
        {
            _isVisible = false;
            if (binaryPanel == null) return;
            if (instant) { binaryPanel.alpha = 0f; binaryPanel.gameObject.SetActive(false); return; }
            binaryPanel.DOFade(0f, 0.4f).OnComplete(() => binaryPanel.gameObject.SetActive(false));
        }

        // Called by BinaryPuzzleManager every time a switch is toggled
        public void Refresh()
        {
            if (BinaryPuzzleManager.Instance == null) return;

            int current = BinaryPuzzleManager.Instance.CurrentValue;

            // Update each bit slot color
            // bitSlots[0] = bit 7 (value 128), bitSlots[7] = bit 0 (value 1)
            for (int i = 0; i < 8; i++)
            {
                int bitIndex = 7 - i; // slot 0 -> bit7, slot 7 -> bit0
                bool isOn = BinaryPuzzleManager.Instance.IsBitOn(bitIndex);
                if (bitSlots[i] != null)
                    bitSlots[i].DOColor(isOn ? bitOnColor : bitOffColor, 0.15f);
            }

            if (currentValueText != null)
                currentValueText.text = $"Your value: <b>{current}</b>";
        }

        private void ShowHint()
        {
            if (BinaryPuzzleManager.Instance == null || hintText == null) return;
            int target = BinaryPuzzleManager.Instance.TargetValue;
            string binary = System.Convert.ToString(target, 2).PadLeft(8, '0');
            hintText.gameObject.SetActive(true);
            hintText.text = $"Hint: {target} = <b>{binary}</b>\n(bit 7 to bit 0, left to right)";
        }
    }
}
