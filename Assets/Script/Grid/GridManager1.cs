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
    [HideInInspector] public int playableWidth = 4;
    [HideInInspector] public int playableHeight = 4;
    public float cellSize = 7.2f;

    [Header("Visual Scale Settings")]
    [Tooltip("Tỷ lệ kích thước món ăn khi đặt vào bàn cờ (1.0 = vừa khít ô như lúc đang kéo, 0.85 = lọt lòng bên trong ô)")]
    public float placedIconScale = 1f;
    public Transform gridOrigin;

    // Lưu thông số gốc ban đầu từ Scene làm chuẩn (mặc định 4x4, cellSize = 7.2)
    public float baseCellSize = -1f;
    private int baseWidth = 4;
    private int baseHeight = 4;
    private Vector3 initialCellSlotPrefabScale = Vector3.one;

    /// <summary>Tỷ lệ thu phóng hiện tại so với kích thước chuẩn 4x4.</summary>
    public float ScaleRatio => (baseCellSize > 0f && cellSize > 0f) ? (cellSize / baseCellSize) : 1f;

    public GameObject cellSlotPrefab;

    private ItemData1[,] cellItems;
    private GameObject[,] cellVisuals;
    public bool[,] isBlocked;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        playableWidth = width;
        playableHeight = height;
        int maxDim = Mathf.Max(width, height);
        width = maxDim;
        height = maxDim;

        cellItems = new ItemData1[width, height];
        cellVisuals = new GameObject[width, height];
        isBlocked = new bool[width, height];
    }

    void Start() => RebuildVisualGrid();

    public void SetBlockedCells(List<Vector2Int> blockedList)
    {
        isBlocked = new bool[width, height];

        // Tự động lấp đầy các ô thừa bên ngoài phạm vi chơi thành ô bị chặn
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x >= playableWidth || y >= playableHeight)
                    isBlocked[x, y] = true;
            }
        }

        if (blockedList != null)
        {
            foreach (var pos in blockedList)
            {
                if (IsInsideGrid(pos))
                    isBlocked[pos.x, pos.y] = true;
            }
        }
        RebuildVisualGrid();
    }

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

                if (isBlocked != null && isBlocked[x, y])
                {
                    SpriteRenderer sr = slot.GetComponentInChildren<SpriteRenderer>();
                    if (sr != null) sr.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Mau den xam
                }
            }
        }
    }

    /// <summary>
    /// Resize Grid sang kich thuoc moi (5x5, 6x6...), tu dong scale cellSize va cac o vuong vua khit khung ban co.
    /// Tu dong lap day cac o thua ngoai pham vi choi thanh o bi chan (Blocked Cells).
    /// </summary>
    public void ResizeGrid(int newWidth, int newHeight)
    {
        if (baseCellSize <= 0f) baseCellSize = cellSize > 0f ? cellSize : 7.2f;
        if (baseWidth <= 0) baseWidth = 4;
        if (baseHeight <= 0) baseHeight = 4;
        if (cellSlotPrefab != null && initialCellSlotPrefabScale == Vector3.one)
            initialCellSlotPrefabScale = cellSlotPrefab.transform.localScale;

        playableWidth = newWidth > 0 ? newWidth : 4;
        playableHeight = newHeight > 0 ? newHeight : 4;

        int maxDim = Mathf.Max(playableWidth, playableHeight);
        width = maxDim;
        height = maxDim;

        // Tong chieu rong ban co goc = baseWidth * baseCellSize (vd: 4 * 7.2 = 28.8)
        float totalBaseGridSize = baseWidth * baseCellSize;
        cellSize = totalBaseGridSize / maxDim;

        // Reset data arrays
        cellItems = new ItemData1[width, height];
        cellVisuals = new GameObject[width, height];
        isBlocked = new bool[width, height];

        // Tu dong chan cac o thua ben ngoai
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x >= playableWidth || y >= playableHeight)
                    isBlocked[x, y] = true;
            }
        }

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
        IsInsideGrid(cell) && cellItems[cell.x, cell.y] == null && (isBlocked == null || !isBlocked[cell.x, cell.y]);

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
    public void PlaceBlock(BlockShapeData shape, Vector2Int originCell, ItemData1[] itemPerCell, GameObject iconPrefab, float rotationAngle = 0f)
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
                // Xử lý bù trừ vị trí do pivot của sprite nằm ở Bottom-Left (0,0)
                Vector3 pivotOffset = Vector3.zero;
                int angle = Mathf.RoundToInt(rotationAngle) % 360;
                if (angle < 0) angle += 360;
                if (angle == 270) pivotOffset = new Vector3(0, cellSize, 0);       // -90 độ
                else if (angle == 180) pivotOffset = new Vector3(cellSize, cellSize, 0); // -180 độ
                else if (angle == 90) pivotOffset = new Vector3(cellSize, 0, 0);   // -270 độ (+90)

                icon = Instantiate(iconPrefab, CellToWorld(cell.x, cell.y) + pivotOffset, Quaternion.identity, transform);
                icon.transform.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);
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
                info.cellIconPrefab = iconPrefab;
                info.rotationAngle = rotationAngle;
            }

            cellVisuals[cell.x, cell.y] = icon;
        }
    }

    /// <summary>
    /// Lấy tất cả PlacedBlockInfo1 của các ô thuộc cùng 1 khối.
    /// </summary>
    public List<PlacedBlockInfo1> GetAllCellsOfBlock(Vector2Int originCell, BlockShapeData shape)
    {
        List<PlacedBlockInfo1> list = new List<PlacedBlockInfo1>();
        if (shape == null || shape.cells == null) return list;

        for (int i = 0; i < shape.cells.Length; i++)
        {
            Vector2Int cell = originCell + shape.cells[i];
            PlacedBlockInfo1 info = GetPlacedBlockInfoAt(cell.x, cell.y);
            if (info != null && info.originCell == originCell && !list.Contains(info))
            {
                list.Add(info);
            }
        }
        return list;
    }

    /// <summary>
    /// Thử xoay khối đã đặt trên Grid theo chiều kim đồng hồ.
    /// Nếu hợp lệ: xoay và cập nhật lại Grid.
    /// Nếu không hợp lệ: khôi phục vị trí cũ và chớp đỏ cảnh báo.
    /// </summary>
    public bool TryRotatePlacedBlock(PlacedBlockInfo1 blockInfo)
    {
        if (blockInfo == null || blockInfo.shapeData == null) return false;

        BlockShapeData oldShape = blockInfo.shapeData;
        Vector2Int origin = blockInfo.originCell;
        ItemData1[] items = blockInfo.itemPerCell;
        GameObject iconPrefab = blockInfo.cellIconPrefab;
        float oldRotationAngle = blockInfo.rotationAngle;
        float newRotationAngle = oldRotationAngle - 90f;

        BlockShapeData rotatedShape = oldShape.GetRotatedClockwiseShape();

        // 1. Tạm gỡ khối cũ khỏi Grid (để không tự kiểm tra va chạm với chính nó)
        RemovePlacedBlock(oldShape, origin);

        // 2. Kiểm tra xem hình dạng mới có đặt vừa không
        if (CanPlace(rotatedShape, origin))
        {
            // Đặt hình dạng mới đã xoay vào Grid
            PlaceBlock(rotatedShape, origin, items, iconPrefab, newRotationAngle);

            // Cập nhật lượt đi và thông báo cho ScoringSystem
            if (ScoringSystem1.Instance != null)
            {
                ScoringSystem1.Instance.availableMoves--;
                ScoringSystem1.Instance.NotifyGridChanged();
            }
            return true;
        }
        else
        {
            // Không đặt được -> Khôi phục lại khối ban đầu
            PlaceBlock(oldShape, origin, items, iconPrefab, oldRotationAngle);

            // Báo đỏ trên các ô của khối
            PlacedBlockInfo1 restoredInfo = GetPlacedBlockInfoAt(origin.x + oldShape.cells[0].x, origin.y + oldShape.cells[0].y);
            if (restoredInfo != null)
            {
                restoredInfo.FlashRed();
            }
            return false;
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
