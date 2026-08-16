using System.Collections.Generic;
using UnityEngine;

public class GridManager1 : MonoBehaviour
{
    // Tong so luong tung loai vi hien co tren toan ban choi
    public Dictionary<string, int> totalFlavorCounts = new Dictionary<string, int>()
    {
        { "sour", 0 }, { "spicy", 0 }, { "salty", 0 }, { "sweet", 0 },
        { "bitter", 0 }, { "umami", 0 }, { "buttery", 0 }
    };

    public static GridManager1 Instance { get; private set; }
    public int width = 4;
    public int height = 4;
    public float cellSize = 7.2f;

    [Header("Visual Scale Settings")]
    [Tooltip("Tỷ lệ kích thước món ăn khi đặt vào bàn cờ (1.0 = vừa khít ô như lúc đang kéo, 0.85 = lọt lòng bên trong ô)")]
    public float placedIconScale = 1f;
    public Transform gridOrigin;

    // Lưu thông số gốc ban đầu từ Scene làm chuẩn (mặc định 4x4, cellSize = 7.2)
    private float baseCellSize = -1f;
    private int baseWidth = 4;
    private int baseHeight = 4;
    private Vector3 initialCellSlotPrefabScale = Vector3.one;

    /// <summary>Tỷ lệ thu phóng hiện tại so với kích thước chuẩn 4x4.</summary>
    public float ScaleRatio => (baseCellSize > 0f && cellSize > 0f) ? (cellSize / baseCellSize) : 1f;

    public GameObject cellSlotPrefab;

    private ItemData1[,] cellItems;
    private GameObject[,] cellVisuals;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (baseCellSize <= 0f) baseCellSize = cellSize;
        if (baseWidth <= 0) baseWidth = width > 0 ? width : 4;
        if (baseHeight <= 0) baseHeight = height > 0 ? height : 4;
        if (cellSlotPrefab != null) initialCellSlotPrefabScale = cellSlotPrefab.transform.localScale;

        cellItems = new ItemData1[width, height];
        cellVisuals = new GameObject[width, height];
    }

    void Start() => RebuildVisualGrid();

    public void RebuildVisualGrid()
    {
        // Xoa cell slot cu
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        if (cellSlotPrefab == null) return;

        float ratio = ScaleRatio;
        Vector3 slotScale = initialCellSlotPrefabScale * ratio;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject slot = Instantiate(cellSlotPrefab, CellToWorld(x, y), Quaternion.identity, transform);
                slot.transform.localScale = slotScale;
            }
        }
    }

    /// <summary>
    /// Resize Grid sang kich thuoc moi (5x5, 6x6...), tu dong scale cellSize va cac o vuong vua khit khung ban co.
    /// </summary>
    public void ResizeGrid(int newWidth, int newHeight)
    {
        if (baseCellSize <= 0f) baseCellSize = cellSize > 0f ? cellSize : 7.2f;
        if (baseWidth <= 0) baseWidth = 4;
        if (baseHeight <= 0) baseHeight = 4;
        if (cellSlotPrefab != null && initialCellSlotPrefabScale == Vector3.one)
            initialCellSlotPrefabScale = cellSlotPrefab.transform.localScale;

        width = newWidth > 0 ? newWidth : 4;
        height = newHeight > 0 ? newHeight : 4;

        // Tong chieu rong ban co goc = baseWidth * baseCellSize (vd: 4 * 7.2 = 28.8)
        float totalBaseGridSize = baseWidth * baseCellSize;
        cellSize = totalBaseGridSize / Mathf.Max(width, height);

        // Reset data arrays
        cellItems = new ItemData1[width, height];
        cellVisuals = new GameObject[width, height];

        RebuildVisualGrid();
    }

    public Vector3 CellToWorld(int x, int y)
    {
        Vector3 origin = gridOrigin != null ? gridOrigin.position : Vector3.zero;
        return origin + new Vector3(x * cellSize, y * cellSize, 0f);
    }

    public Vector2Int WorldToCell(Vector3 worldPos)
    {
        Vector3 origin = gridOrigin != null ? gridOrigin.position : Vector3.zero;
        Vector3 local = worldPos - origin;
        return new Vector2Int(Mathf.RoundToInt(local.x / cellSize), Mathf.RoundToInt(local.y / cellSize));
    }

    public bool IsInsideGrid(Vector2Int cell) =>
        cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;

    public bool IsCellEmpty(Vector2Int cell) =>
        IsInsideGrid(cell) && cellItems[cell.x, cell.y] == null;

    public bool CanPlace(BlockShapeData shape, Vector2Int originCell)
    {
        foreach (var offset in shape.cells)
            if (!IsCellEmpty(originCell + offset)) return false;
        return true;
    }

    /// <summary>
    /// Dat khoi vao Grid va cap nhat totalFlavorCounts.
    /// Viec dem luot di do GameManager1 quan ly.
    /// </summary>
    public void PlaceBlock(BlockShapeData shape, Vector2Int originCell, ItemData1[] itemPerCell, GameObject iconPrefab)
    {
        // Cap nhat tong vi
        if (itemPerCell != null && itemPerCell.Length > 0 && itemPerCell[0]?.flavorCounts != null)
        {
            foreach (var flavor in itemPerCell[0].flavorCounts)
            {
                if (totalFlavorCounts.ContainsKey(flavor.flavorName))
                    totalFlavorCounts[flavor.flavorName] += flavor.count;
                else
                    totalFlavorCounts[flavor.flavorName] = flavor.count;
            }
        }

        // Ghi du lieu va tao visual
        for (int i = 0; i < shape.cells.Length; i++)
        {
            Vector2Int cell = originCell + shape.cells[i];
            if (!IsInsideGrid(cell)) continue;

            cellItems[cell.x, cell.y] = itemPerCell != null && i < itemPerCell.Length ? itemPerCell[i] : null;

            GameObject icon = null;
            if (iconPrefab != null)
            {
                icon = Instantiate(iconPrefab, CellToWorld(cell.x, cell.y), Quaternion.identity, transform);
                icon.transform.localScale = Vector3.one;

                SpriteRenderer[] srs = icon.GetComponentsInChildren<SpriteRenderer>();
                for (int j = 0; j < srs.Length; j++) srs[j].sortingOrder += 10;

                if (srs.Length > 0 && itemPerCell != null && i < itemPerCell.Length && itemPerCell[i] != null)
                {
                    SpriteRenderer targetSr = srs.Length > 1 ? srs[srs.Length - 1] : srs[0];
                    if (targetSr != null && itemPerCell[i].icon != null)
                    {
                        targetSr.sprite = itemPerCell[i].icon;
                        if (targetSr.sprite.bounds.size.x > 0)
                        {
                            float spriteSize = Mathf.Max(targetSr.sprite.bounds.size.x, targetSr.sprite.bounds.size.y);
                            targetSr.transform.localScale = Vector3.one * ((cellSize / spriteSize) * placedIconScale);
                        }
                    }
                }

                PlacedBlockInfo1 info = icon.AddComponent<PlacedBlockInfo1>();
                info.shapeData = shape;
                info.originCell = originCell;
                info.itemPerCell = itemPerCell;
            }

            cellVisuals[cell.x, cell.y] = icon;
        }
    }

    public void RemovePlacedBlock(BlockShapeData shape, Vector2Int originCell)
    {
        // Tru vi cua khoi 1 lan duy nhat
        ItemData1 firstItem = null;
        for (int i = 0; i < shape.cells.Length; i++)
        {
            Vector2Int cell = originCell + shape.cells[i];
            if (!IsInsideGrid(cell)) continue;
            if (cellItems[cell.x, cell.y] != null) { firstItem = cellItems[cell.x, cell.y]; break; }
        }

        if (firstItem?.flavorCounts != null)
        {
            foreach (var flavor in firstItem.flavorCounts)
            {
                if (totalFlavorCounts.ContainsKey(flavor.flavorName))
                    totalFlavorCounts[flavor.flavorName] = Mathf.Max(0, totalFlavorCounts[flavor.flavorName] - flavor.count);
            }
        }

        for (int i = 0; i < shape.cells.Length; i++)
            ClearCell((originCell + shape.cells[i]).x, (originCell + shape.cells[i]).y);
    }

    public ItemData1 GetItemAt(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return null;
        return cellItems[x, y];
    }

    public PlacedBlockInfo1 GetPlacedBlockInfoAt(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return null;
        return cellVisuals[x, y] != null ? cellVisuals[x, y].GetComponent<PlacedBlockInfo1>() : null;
    }

    public bool IsRowFull(int y)
    {
        for (int x = 0; x < width; x++) if (cellItems[x, y] == null) return false;
        return true;
    }

    public bool IsColFull(int x)
    {
        for (int y = 0; y < height; y++) if (cellItems[x, y] == null) return false;
        return true;
    }

    public void ClearRow(int y) { for (int x = 0; x < width; x++) ClearCell(x, y); }
    public void ClearCol(int x) { for (int y = 0; y < height; y++) ClearCell(x, y); }

    /// <summary>
    /// Xóa toàn bộ các khối trên bàn cờ và reset lại tổng vị về 0.
    /// </summary>
    public void ClearAllGrid()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                ClearCell(x, y);

        // Quét sạch tất cả PlacedBlockInfo1 còn sót lại trong Scene
        PlacedBlockInfo1[] allPlaced = FindObjectsOfType<PlacedBlockInfo1>();
        foreach (var p in allPlaced)
        {
            if (p != null) Destroy(p.gameObject);
        }

        totalFlavorCounts["sour"] = 0;
        totalFlavorCounts["spicy"] = 0;
        totalFlavorCounts["salty"] = 0;
        totalFlavorCounts["sweet"] = 0;
        totalFlavorCounts["bitter"] = 0;
        totalFlavorCounts["umami"] = 0;
        totalFlavorCounts["buttery"] = 0;
    }

    void ClearCell(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return;
        if (cellVisuals[x, y] != null) Destroy(cellVisuals[x, y]);
        cellVisuals[x, y] = null;
        cellItems[x, y] = null;
    }

    public bool HasAnyValidMove(BlockShapeData[] currentShapes)
    {
        foreach (var shape in currentShapes)
        {
            if (shape == null) continue;
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (CanPlace(shape, new Vector2Int(x, y))) return true;
        }
        return false;
    }
}
