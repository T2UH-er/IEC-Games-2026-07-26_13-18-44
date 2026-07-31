using UnityEngine;

[CreateAssetMenu(fileName = "NewBlockShape", menuName = "BlockGame/Block Shape")]
public class BlockShapeData : ScriptableObject
{
    public Vector2Int [] cells;
    public int CellCount => cells.Length;
}