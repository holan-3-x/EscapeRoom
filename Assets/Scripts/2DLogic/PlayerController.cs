namespace BlockGame {
using System.Collections;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Griglia di movimento")]
    [SerializeField] private float cellSize    = 100f;  
    [SerializeField] private float moveSpeed   = 300f;  

    [Header("Posizione di partenza del livello")]
    [SerializeField] private Vector2 startPosition;

    private Rigidbody2D _rb;
    private RectTransform _rectTransform;
    private bool _isMoving;
    private bool _blockedThisRun;   

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rectTransform = GetComponent<RectTransform>();
    }

    public IEnumerator Move(Vector2 direction, int steps)
    {
        if (_blockedThisRun) yield break;

        for (int i = 0; i < steps; i++)
        {
            if (_blockedThisRun) yield break;

            // Calcoliamo il target basandoci sulla posizione logica attuale della Transform
            Vector2 target = (Vector2)transform.position + direction * cellSize;
            yield return StartCoroutine(SmoothMove(target));
        }
    }

    private IEnumerator SmoothMove(Vector2 target)
    {
        _isMoving = true;
        Vector2 start = transform.position;
        float elapsed = 0f;
        float duration = cellSize / moveSpeed;

        while (elapsed < duration)
        {
            if (_blockedThisRun) yield break;

            elapsed += Time.unscaledDeltaTime;  
            float t = Mathf.Clamp01(elapsed / duration);
            t = 1f - (1f - t) * (1f - t); 

            transform.position = Vector2.Lerp(start, target, t);
            if (_rb != null) _rb.position = transform.position;

            // --- FORZA IL MOTORE FISICO A CONTROLLARE LE COLLISIONI ---
            // Fa avanzare la fisica 2D nel tempo reale corrente, attivando automaticamente tutti i Trigger!
            Physics2D.Simulate(Time.unscaledDeltaTime);

            yield return null;
        }

        if (!_blockedThisRun)
        {
            transform.position = target;
            if (_rb != null) _rb.position = target;
            
            // Un ultimo check fisco finale per sicurezza sulla posizione d'arrivo
            Physics2D.Simulate(Time.unscaledDeltaTime);
        }
        
        _isMoving = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ExitDoor"))
        {
            Debug.Log("[PlayerController] Porta raggiunta! Livello completato.");
            LevelManager.Instance?.LevelComplete();
        }
        
        if (other.CompareTag("UI_Obstacle"))
        {
            Debug.Log("[PlayerController] Ostacolo rilevato! Avvio feedback visivo di errore.");
            
            // Comunichiamo il blocco immediato di questo ciclo di coroutine
            _blockedThisRun = true;
            
            Object.FindFirstObjectByType<CodeExecutor>()?.StopExecution();

            // Animazione sobbalzo di Aria (Ignora pausa)
            transform.DOComplete(); 
            transform.DOPunchScale(new Vector3(-0.2f, -0.2f, 0f), 0.25f, 10, 1f).SetUpdate(true);

            // Wobble dello sfondo/griglia del livello (Ignora pausa)
            Transform levelPanel = transform.parent; 
            if (levelPanel != null)
            {
                levelPanel.DOComplete();
                levelPanel.DOShakePosition(0.3f, new Vector3(15f, 0f, 0f), 20, 90f, false, true).SetUpdate(true);
            }

            // Timing del reset basato su tempo reale (unscaled)
            DOVirtual.DelayedCall(0.35f, () => 
            {
                if (levelPanel != null)
                {
                    levelPanel.DOComplete(true); // Ripristina la mappa perfettamente al centro
                }

                Object.FindFirstObjectByType<SequenceManager>()?.ClearSequenceArea();

                ResetToStart();
            }).SetUpdate(true);
        }
    }

    public void ResetToStart()
    {
        _blockedThisRun  = false;
        _isMoving        = false;
        
        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.position = startPosition;
        }
        
        if (_rectTransform != null)
        {
            _rectTransform.anchoredPosition = startPosition;
        }
        else
        {
            transform.position = startPosition;
        }
    }
}
}
