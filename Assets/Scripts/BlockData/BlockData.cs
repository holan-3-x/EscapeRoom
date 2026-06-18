using UnityEngine;

// Crea asset: tasto destro in Project > Create > BlockPuzzle > BlockData
[CreateAssetMenu(menuName = "BlockPuzzle/BlockData", fileName = "NewBlock")]
public class BlockData : ScriptableObject
{
    public enum BlockType { MoveUp, MoveDown, MoveLeft, MoveRight, Loop }

    [Header("Tipo di blocco")]
    public BlockType blockType;

    [Header("Parametro (passi o ripetizioni)")]
    [Range(1, 10)]
    public int parameter = 1;

    [Header("Visuale")]
    public string label;          // Es. "Avanti 3", "Loop x2"
    public Color blockColor = Color.white;
    public Sprite blockIcon;
}
