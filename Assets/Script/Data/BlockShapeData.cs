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

    /// <summary>
    /// Tạo bản sao BlockShapeData với các ô được xoay 90 độ theo chiều kim đồng hồ quanh đúng tâm gốc (0,0).
    /// Công thức biến đổi: (x', y') = (y, -x)
    /// </summary>
    public BlockShapeData GetRotatedClockwiseShape()
    {
        BlockShapeData rotated = ScriptableObject.CreateInstance<BlockShapeData>();
        rotated.name = this.name + "_Rotated";
        rotated.dishSprite = this.dishSprite;
        rotated.dishOffset = this.dishOffset;

        if (cells == null || cells.Length == 0)
        {
            rotated.cells = new Vector2Int[0];
            return rotated;
        }

        Vector2Int[] newCells = new Vector2Int[cells.Length];

        // Xoay 90 độ theo chiều kim đồng hồ quanh đúng tâm (0,0): (x', y') = (y, -x)
        for (int i = 0; i < cells.Length; i++)
        {
            int rx = cells[i].y;
            int ry = -cells[i].x;
            newCells[i] = new Vector2Int(rx, ry);
        }

        rotated.cells = newCells;
        return rotated;
    }
}