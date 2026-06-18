using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

// Renamed from 'PuzzleManager' to avoid clashing with the logic-gate game's
// PuzzleManager. Class name now matches the file name (PuzzleManager2x2).
public class PuzzleManager2x2 : MonoBehaviour
{
    [Header("Fires when the WHOLE network puzzle is finished (wire to MinigameLauncher.CompleteMinigame)")]
    public UnityEvent onAllSolved;

    [Header("Puzzle Components")]
    public RotateOnClick[] allTiles;
    public Image pcImage;

    [Header("Sprites")]
    public Sprite unlitSprite;
    public Sprite litSprite;
    public Sprite pcConnectedSprite;
    public Sprite pcDisconnectedSprite;

    [Header("UI & Stats Elements")]
    public GameObject winPanel;

    [Header("Level Flow")]
    public GameObject currentLevelPanel;
    public GameObject nextLevelPanel;

    private int clickCount = 0;
    private bool levelSolved = false;

    void Start()
    {
        UpdatePuzzleState();
    }

    public void RegisterClick()
    {
        if (levelSolved) return;
        clickCount++;
        UpdatePuzzleState();
    }

    public void UpdatePuzzleState()
    {
        // Reset everything to unlit first
        foreach (var tile in allTiles)
        {
            if (tile != null) tile.GetComponent<Image>().sprite = unlitSprite;
        }
        if (pcImage != null) pcImage.sprite = pcDisconnectedSprite;

        // Track which tiles are actively powered using a path-tracing list
        HashSet<RotateOnClick> poweredTiles = new HashSet<RotateOnClick>();

       

        // Find the starting tile directly under the server (Tile1 - index 0)
        if (allTiles != null && allTiles.Length > 0)
        {
            RotateOnClick startTile = allTiles[0]; 

            // Server flows DOWN. Check if Tile1 has an opening facing UP to accept it.
            if (startTile != null && HasOpening(startTile, "UP"))
            {
                TraceFlow(startTile, poweredTiles);
            }
        }

        // Light up all discovered valid connections
        foreach (var tile in poweredTiles)
        {
            if (tile != null) tile.GetComponent<Image>().sprite = litSprite;
        }

        // Safety check: Check if the PC receives power from Tile1 (2)
        if (allTiles != null && allTiles.Length > 2)
        {
            RotateOnClick endTile = allTiles[2]; 
            if (endTile != null && poweredTiles.Contains(endTile) && HasOpening(endTile, "DOWN"))
            {
                if (pcImage != null) 
                {
                    pcImage.sprite = pcConnectedSprite;

                        //win panel display
                    if (!levelSolved)
                    {
                        levelSolved = true;
                        if (winPanel != null)
                        {
                            winPanel.SetActive(true);
                        }
                    }

                }
            }
        }
        else
        {
            Debug.LogWarning("Please assign at least 3 tiles to the All Tiles list on the PuzzleManager!");
        }
    }

    void TraceFlow(RotateOnClick current, HashSet<RotateOnClick> visited)
    {
        if (current == null || visited.Contains(current)) return;
        visited.Add(current);

        // Find neighbors based on layout positions
        foreach (var neighbor in allTiles)
        {
            if (neighbor == null || visited.Contains(neighbor)) continue;

            // Check connection directions between current tile and neighbor
            if (AreConnected(current, neighbor))
            {
                TraceFlow(neighbor, visited);
            }
        }
    }

    bool AreConnected(RotateOnClick a, RotateOnClick b)
    {
        Vector3 posA = a.transform.localPosition;
        Vector3 posB = b.transform.localPosition;

        float threshold = 20f; // Distance tolerance threshold

        // B is to the RIGHT of A
        if (posB.x - posA.x > threshold && Mathf.Abs(posB.y - posA.y) < threshold)
            return HasOpening(a, "RIGHT") && HasOpening(b, "LEFT");

        // B is to the LEFT of A
        if (posA.x - posB.x > threshold && Mathf.Abs(posB.y - posA.y) < threshold)
            return HasOpening(a, "LEFT") && HasOpening(b, "RIGHT");

        // B is BELOW A
        if (posA.y - posB.y > threshold && Mathf.Abs(posB.x - posA.x) < threshold)
            return HasOpening(a, "DOWN") && HasOpening(b, "UP");

        // B is ABOVE A
        if (posB.y - posA.y > threshold && Mathf.Abs(posB.x - posA.x) < threshold)
            return HasOpening(a, "UP") && HasOpening(b, "DOWN");

        return false;
    }

    bool HasOpening(RotateOnClick tile, string direction)
    {
        float z = tile.GetCurrentZ();

        // Maps the faces of your elbow pipe (assumes 0 degrees opens UP and RIGHT)
        switch (direction)
        {
            case "UP":
                return (z == 0f || z == 90f); 
            case "RIGHT":
                return (z == 0f || z == 270f);
            case "DOWN":
                return (z == 180f || z == 270f);
            case "LEFT":
                return (z == 90f || z == 180f);
            default:
                return false;
        }
    }

    // This function will be called by your Next Level Button
    public void LoadNextLevel()
    {
        if (nextLevelPanel != null)
        {
            // 1. Hide the win panel so it isn't open on the next level
            if (winPanel != null) winPanel.SetActive(false);

            // 2. Turn off the current level layout
            if (currentLevelPanel != null) currentLevelPanel.SetActive(false);

            // 3. Turn on the next level layout
            nextLevelPanel.SetActive(true);

            // 4. Automatically find and setup the new level's components
            PuzzleManager2x2 nextManager = nextLevelPanel.GetComponent<PuzzleManager2x2>();
            if (nextManager != null)
            {
                nextManager.UpdatePuzzleState();
            }
        }
        else
        {
            Debug.Log("Congratulations!");
            // Whole network puzzle done — tell the main game (opens the door, etc.)
            onAllSolved?.Invoke();
        }
    }
}