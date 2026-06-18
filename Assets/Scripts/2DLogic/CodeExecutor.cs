namespace BlockGame {
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CodeExecutor : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private PlayerController player;
    [SerializeField] private SequenceManager  sequenceManager;

    [Header("Pausa tra un'istruzione e la prossima (secondi)")]
    [SerializeField] private float stepDelay = 0.4f;

    private bool _isRunning;
    private Coroutine _mainSequenceCoroutine; 

    public void Execute(List<BlockData> sequence)
    {
        if (_isRunning)
        {
            Debug.LogWarning("[CodeExecutor] Esecuzione già in corso.");
            return;
        }
        _mainSequenceCoroutine = StartCoroutine(RunSequence(sequence));
    }

    private IEnumerator RunSequence(List<BlockData> sequence)
    {
        _isRunning = true;
        sequenceManager.SetExecuteInteractable(false);

        foreach (BlockData block in sequence)
        {
            yield return StartCoroutine(ExecuteBlock(block));
            
            // --- FIX PAUSA: Usa il tempo reale dell'orologio ---
            yield return new WaitForSecondsRealtime(stepDelay);
        }

        StopExecution();
    }

    /// <summary>
    /// Blocca immediatamente l'esecuzione del codice (chiamato anche da PlayerController in caso di urto).
    /// </summary>
    public void StopExecution()
    {
        if (_mainSequenceCoroutine != null)
        {
            StopCoroutine(_mainSequenceCoroutine);
            _mainSequenceCoroutine = null;
        }

        player.StopAllCoroutines(); 

        _isRunning = false;
        sequenceManager.SetExecuteInteractable(true);
    }

    private IEnumerator ExecuteBlock(BlockData block)
    {
        switch (block.blockType)
        {
            case BlockData.BlockType.MoveUp:
                yield return StartCoroutine(player.Move(Vector2.up, block.parameter));
                break;

            case BlockData.BlockType.MoveDown:
                yield return StartCoroutine(player.Move(Vector2.down, block.parameter));
                break;

            case BlockData.BlockType.MoveLeft:
                yield return StartCoroutine(player.Move(Vector2.left, block.parameter));
                break;

            case BlockData.BlockType.MoveRight:
                yield return StartCoroutine(player.Move(Vector2.right, block.parameter));
                break;

            case BlockData.BlockType.Loop:
                Debug.Log($"[CodeExecutor] Loop x{block.parameter} — espandibile in futuro.");
                for (int i = 0; i < block.parameter; i++)
                    yield return new WaitForSecondsRealtime(stepDelay);
                break;
        }
    }
}
}
