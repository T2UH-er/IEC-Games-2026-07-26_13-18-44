using UnityEngine;

[CreateAssetMenu(fileName = "NewBlockShape", menuName = "BlockGame/Block Shape")]
public class BlockShapeData : ScriptableObject
{
    public Vector2Int [] cells;
    public Sprite dishSprite; // Sprite món ăn gán cho shape
    public Vector3 dishOffset = Vector3.zero; // Tùy chỉnh vị trí offset cho sprite món nếu cần

    public int CellCount => cells.Length;

    public Vector2 GetCenterOffset()
    {
        if (cells == null || cells.Length == 0) return Vector2.zero;
        float minX = cells[0].x, maxX = cells[0].x;
        float minY = cells[0].y, maxY = cells[0].y;
        for (int i = 1; i < cells.Length; i++)
        {
            if (cells[i].x < minX) minX = cells[i].x;
            if (cells[i].x > maxX) maxX = cells[i].x;
            if (cells[i].y < minY) minY = cells[i].y;
            if (cells[i].y > maxY) maxY = cells[i].y;
        }
        return new Vector2((minX + maxX) * 0.5f, (minY + maxY) * 0.5f);
    }
}