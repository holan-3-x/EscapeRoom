using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

namespace SYSTEMESCAPE
{
    public class CharacterSelectManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string classroomSceneName = "Level0_Classroom";
        
        [Header("UI Elements")]
        [SerializeField] private Button maleButton;
        [SerializeField] private Button femaleButton;
        [SerializeField] private CanvasGroup selectionPanel;

        private void Start()
        {
            if (selectionPanel != null)
            {
                selectionPanel.alpha = 1f;
                selectionPanel.interactable = true;
                selectionPanel.blocksRaycasts = true;
            }

            if (maleButton != null) maleButton.onClick.AddListener(() => SelectCharacter(Gender.Male));
            if (femaleButton != null) femaleButton.onClick.AddListener(() => SelectCharacter(Gender.Female));
        }

        private void SelectCharacter(Gender gender)
        {
            if (maleButton != null) maleButton.interactable = false;
            if (femaleButton != null) femaleButton.interactable = false;

            // Store the choice in GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SelectedGender = gender;
            }
            else
            {
                Debug.LogError("No GameManager found in scene! Character selection will not be saved.");
            }

            // Fade out and load the classroom scene
            if (selectionPanel != null)
            {
                selectionPanel.DOFade(0f, 0.5f).OnComplete(() =>
                {
                    SceneManager.LoadScene(classroomSceneName);
                });
            }
            else
            {
                SceneManager.LoadScene(classroomSceneName);
            }

            Debug.Log($"Character {gender} selected. Loading {classroomSceneName}...");
        }
    }
}
