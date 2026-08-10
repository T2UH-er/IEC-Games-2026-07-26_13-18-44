using UnityEngine;

[CreateAssetMenu(fileName = "BlockConfig", menuName = "BlockGame/BlockConfig")]
public class BlockConfig : ScriptableObject
{
    public BlockShapeData shapeData;
    public ItemData1[] itemPerCell;
}
