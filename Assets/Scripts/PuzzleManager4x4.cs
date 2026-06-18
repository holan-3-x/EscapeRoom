using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class PuzzleManager_Lvl2 : MonoBehaviour
{
    [Header("Fires when this (final) network level is solved (wire to MinigameLauncher.CompleteMinigame)")]
    public UnityEvent onAllSolved;

    public enum PipeType { Elbow, Straight, Tee, Cross }

    [System.Serializable]
    public class TileData
    {
        public RotateOnClick_Lvl2 tileScript;
        public PipeType pipeShape;
    }

    [Header("Grid Configuration (4x4)")]
    public List<TileData> allTiles = new List<TileData>();
    
    [Header("End Devices (2 PCs)")]
    public Image pcImage1;
    public Image pcImage2;

    [Header("Sprites to Apply (PCs Only)")]
    public Sprite pcConnectedSprite;
    public Sprite pcDisconnectedSprite;

    [Header("UI Panels")]
    public GameObject finalPanel;

    [Header("Level Flow")]
    public GameObject currentLevelPanel;
    public GameObject nextLevelPanel;

    private bool levelSolved = false;

    void Start()
    {
        UpdatePuzzleState();
    }

    public void RegisterClick()
    {
        if (levelSolved) return;
        UpdatePuzzleState();
    }

    public void UpdatePuzzleState()
    {
        if (allTiles == null || allTiles.Count < 16) return;

        // 1. Reset all tiles and PCs to their unlit/disconnected states
        foreach (var data in allTiles)
        {
            if (data.tileScript != null)
            {
                data.tileScript.GetComponent<Image>().sprite = data.tileScript.unlitSprite;
            }
        }
        if (pcImage1 != null) pcImage1.sprite = pcDisconnectedSprite;
        if (pcImage2 != null) pcImage2.sprite = pcDisconnectedSprite;

        HashSet<RotateOnClick_Lvl2> poweredTiles = new HashSet<RotateOnClick_Lvl2>();

        // SERVER CONNECTION CHECK: Power ONLY flows into the system if entry tiles point UP.
        // Tile 2 (index 1) verification
        if (allTiles[1].tileScript != null && HasOpening(allTiles[1], "UP"))
        {
            TraceFlow(1, poweredTiles);
        }
        
        // Tile 3 (index 2) verification
        if (allTiles[2].tileScript != null && HasOpening(allTiles[2], "UP"))
        {
            TraceFlow(2, poweredTiles);
        }

        // Update visual sprites only for tiles that successfully received power pathing
        foreach (var data in allTiles)
        {
            if (data.tileScript != null && poweredTiles.Contains(data.tileScript))
            {
                data.tileScript.GetComponent<Image>().sprite = data.tileScript.litSprite;
            }
        }

        // 4. End Point Verification
        bool pc1Connected = false;
        bool pc2Connected = false;

        // Check PC 1: Positioned under Tile 13 (List Index 12)
        if (allTiles[12].tileScript != null && poweredTiles.Contains(allTiles[12].tileScript) && HasOpening(allTiles[12], "DOWN"))
        {
            if (pcImage1 != null) 
            { 
                pcImage1.sprite = pcConnectedSprite; 
                pc1Connected = true; 
            }
        }

        // Check PC 2: Positioned under Tile 16 (List Index 15)
        if (allTiles[15].tileScript != null && poweredTiles.Contains(allTiles[15].tileScript) && HasOpening(allTiles[15], "DOWN"))
        {
            if (pcImage2 != null) 
            { 
                pcImage2.sprite = pcConnectedSprite; 
                pc2Connected = true; 
            }
        }

        // Trigger Completion
        if (pc1Connected && pc2Connected && !levelSolved)
        {
            levelSolved = true;
            if (finalPanel != null) finalPanel.SetActive(true);
            // Network fully connected — tell the main game (opens the Level 2 door)
            onAllSolved?.Invoke();
        }
    }

    void TraceFlow(int currentIndex, HashSet<RotateOnClick_Lvl2> visited)
    {
        TileData currentData = allTiles[currentIndex];
        if (currentData.tileScript == null || visited.Contains(currentData.tileScript)) return;
        
        visited.Add(currentData.tileScript);

        int row = currentIndex / 4;
        int col = currentIndex % 4;

        // Check Up Neighbor
        if (row > 0) CheckAndTrace(currentIndex, currentIndex - 4, "UP", "DOWN", visited);
        // Check Down Neighbor
        if (row < 3) CheckAndTrace(currentIndex, currentIndex + 4, "DOWN", "UP", visited);
        // Check Left Neighbor
        if (col > 0) CheckAndTrace(currentIndex, currentIndex - 1, "LEFT", "RIGHT", visited);
        // Check Right Neighbor
        if (col < 3) CheckAndTrace(currentIndex, currentIndex + 1, "RIGHT", "LEFT", visited);
    }

    void CheckAndTrace(int currentIdx, int neighborIdx, string outDir, string inDir, HashSet<RotateOnClick_Lvl2> visited)
    {
        TileData current = allTiles[currentIdx];
        TileData neighbor = allTiles[neighborIdx];

        if (neighbor.tileScript == null || visited.Contains(neighbor.tileScript)) return;

        if (HasOpening(current, outDir) && HasOpening(neighbor, inDir))
        {
            TraceFlow(neighborIdx, visited);
        }
    }

    bool HasOpening(TileData data, string direction)
    {
        float z = Mathf.Abs(data.tileScript.GetCurrentZ());
        
        // Fix float precision variations on UI layouts
        if (z > 345f || z < 15f) z = 0f;
        else if (z > 75f && z < 105f) z = 90f;
        else if (z > 165f && z < 195f) z = 180f;
        else if (z > 255f && z < 285f) z = 270f;

        // STRAIGHT LINES
        if (data.pipeShape == PipeType.Straight)
        {
            if (z == 0f || z == 180f) return (direction == "UP" || direction == "DOWN");
            if (z == 90f || z == 270f) return (direction == "LEFT" || direction == "RIGHT");
        }

        // T-JUNCTIONS
        if (data.pipeShape == PipeType.Tee)
        {
            switch (direction)
            {
                case "UP": return (z == 0f || z == 90f || z == 270f);
                case "RIGHT": return (z == 0f || z == 90f || z == 180f);
                case "DOWN": return (z == 90f || z == 180f || z == 270f);
                case "LEFT": return (z == 0f || z == 180f || z == 270f);
            }
        }

        // CROSS JUNCTIONS
        if (data.pipeShape == PipeType.Cross) return true;

        // ELBOW JUNCTIONS
        switch (direction)
        {
            case "UP": return (z == 0f || z == 90f);
            case "RIGHT": return (z == 0f || z == 270f);
            case "DOWN": return (z == 180f || z == 270f);
            case "LEFT": return (z == 90f || z == 180f);
            default: return false;
        }
    }

    public void CloseLevelPanel()
    {
        if (finalPanel != null) finalPanel.SetActive(false);
        if (currentLevelPanel != null) currentLevelPanel.SetActive(false);
       
        //UNFREEZE THE GAME (Resumes everything back to normal)
        Time.timeScale = 1f;
    }
}