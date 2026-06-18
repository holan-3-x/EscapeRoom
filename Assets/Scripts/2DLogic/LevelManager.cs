namespace BlockGame {
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using DG.Tweening;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Fires when ALL block levels are solved (wire to MinigameLauncher.CompleteMinigame)")]
    public UnityEvent onAllSolved;

    [Header("Riferimenti Finestre Principali")]
    [Tooltip("Trascina qui l'intero GameObject Canvas del minigioco per poterlo spegnere correttamente.")]
    [SerializeField] private GameObject mainCanvasGameObject; 

    [Header("Riferimenti Entità")]
    [SerializeField] private PlayerController player; 
    [SerializeField] private GameObject levelCompleteUI; 

    [Header("Gestione Livelli UI")]
    [Tooltip("Inserisci qui i pannelli dei livelli (es. Mappa_Livello_1, Mappa_Livello_2) nell'ordine corretto.")]
    [SerializeField] private List<GameObject> levelsUI = new List<GameObject>();
    
    [Header("Sistema Informazioni / Istruzioni")]
    [SerializeField] private GameObject infoPanelGlobal; 
    [SerializeField] private TextMeshProUGUI infoTextGlobal; 
    [Tooltip("Scrivi qui le istruzioni per ciascun livello nello stesso ordine dei pannelli.")]
    [TextArea(3, 10)] [SerializeField] private List<string> levelInstructions = new List<string>();

    [Header("Impostazioni Transizione (DoTween)")]
    [SerializeField] private float fadeDuration = 0.5f;

    private int _currentLevelIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; } 
        Instance = this; 
    }

    private void Start()
    {
        InitializeLevels();
    }

    private void Update()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame)
        {
            PauseAndReturnTo3D();
        }
    }

    private void InitializeLevels()
    {
        if (levelsUI.Count == 0) return;

        for (int i = 0; i < levelsUI.Count; i++)
        {
            if (levelsUI[i] != null)
            {
                levelsUI[i].SetActive(i == _currentLevelIndex);
                CanvasGroup cg = levelsUI[i].GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = (i == _currentLevelIndex) ? 1f : 0f;
                }
            }
        }
    }

    private void PauseAndReturnTo3D()
    {
        Debug.Log("[LevelManager] Tasto ESC premuto. Messa in pausa del minigioco.");

        Object.FindFirstObjectByType<CodeExecutor>()?.StopExecution();

        if (infoPanelGlobal != null) infoPanelGlobal.SetActive(false);
        if (levelCompleteUI) levelCompleteUI.SetActive(false);

        MinigameBase parentMinigame = GetComponentInParent<MinigameBase>();
        if (parentMinigame != null)
        {
            parentMinigame.EndMinigame(); 
        }

        if (mainCanvasGameObject != null)
        {
            mainCanvasGameObject.SetActive(false);
        }
    }

    public void ShowCurrentLevelInstructions()
    {
        if (infoPanelGlobal != null && infoTextGlobal != null)
        {
            if (_currentLevelIndex < levelInstructions.Count)
            {
                infoTextGlobal.text = levelInstructions[_currentLevelIndex];
            }
            else
            {
                infoTextGlobal.text = "Raggiungi la porta evitando gli ostacoli!";
            }

            infoPanelGlobal.SetActive(true);
            infoPanelGlobal.transform.localScale = Vector3.zero;
            infoPanelGlobal.transform.DOScale(Vector3.one, 0.3f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true); // Ignora il Time.timeScale = 0
        }
    }

    public void HideInstructions()
    {
        if (infoPanelGlobal != null)
        {
            infoPanelGlobal.transform.DOScale(Vector3.zero, 0.2f)
                .SetEase(Ease.InBack)
                .SetUpdate(true) // Ignora il Time.timeScale = 0
                .OnComplete(() => infoPanelGlobal.SetActive(false));
        }
    }

    public void LevelComplete() 
    {
        Debug.Log("[LevelManager] Livello completato!"); 
        Object.FindFirstObjectByType<CodeExecutor>()?.StopExecution();

        if (infoPanelGlobal != null && infoPanelGlobal.activeSelf) infoPanelGlobal.SetActive(false);

        if (levelCompleteUI) levelCompleteUI.SetActive(true); 
        StartLevelTransition();
    }

    private void StartLevelTransition()
    {
        if (_currentLevelIndex >= levelsUI.Count) return;

        GameObject currentLevelGO = levelsUI[_currentLevelIndex];
        CanvasGroup currentCG = currentLevelGO.GetComponent<CanvasGroup>();

        if (currentCG != null)
        {
            currentCG.DOFade(0f, fadeDuration).SetUpdate(true).OnComplete(() =>
            {
                currentLevelGO.SetActive(false);
                if (levelCompleteUI) levelCompleteUI.SetActive(false);
                _currentLevelIndex++;
                LoadNextLevelPanel();
            });
        }
        else
        {
            if (levelCompleteUI) levelCompleteUI.SetActive(false);
            currentLevelGO.SetActive(false);
            _currentLevelIndex++;
            LoadNextLevelPanel();
        }
    }

    private void LoadNextLevelPanel()
    {
        if (_currentLevelIndex < levelsUI.Count)
        {
            GameObject nextLevelGO = levelsUI[_currentLevelIndex];
            
            if (nextLevelGO != null)
            {
                CanvasGroup nextCG = nextLevelGO.GetComponent<CanvasGroup>();
                if (nextCG != null)
                {
                    nextCG.alpha = 0f; 
                    nextLevelGO.SetActive(true);
                    nextCG.DOFade(1f, fadeDuration).SetUpdate(true);
                }
                else
                {
                    nextLevelGO.SetActive(true);
                }
            }

            ResetLevel();
        }
        else
        {
            Debug.Log("[LevelManager] Tutti i livelli completati! Chiusura definitiva della Canvas.");
            
            MinigameBase parentMinigame = GetComponentInParent<MinigameBase>();
            if (parentMinigame != null)
            {
                parentMinigame.isCompleted = true;
                parentMinigame.EndMinigame();
            }

            // Tell the main game ARIA is free — wire this to MinigameLauncher.CompleteMinigame
            onAllSolved?.Invoke();

            if (mainCanvasGameObject != null)
            {
                mainCanvasGameObject.SetActive(false);
            }
        }
    }

    public void ResetLevel() 
    {
        player.ResetToStart(); 
        if (levelCompleteUI) levelCompleteUI.SetActive(false); 

        Object.FindFirstObjectByType<SequenceManager>()?.ClearSequenceArea();
    }
}
}
