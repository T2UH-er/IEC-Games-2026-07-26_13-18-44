using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GameManager1 — Trung tâm điều phối toàn bộ luồng game.
/// Chịu trách nhiệm: kéo thả khối, đếm lượt đi, và xây dựng Bubble Sprite.
/// InputManager1 chỉ detect input rồi ủy quyền cho class này.
/// </summary>
public class GameManager1 : MonoBehaviour
{
    public static GameManager1 Instance { get; private set; }

    [Header("Block Piece Prefab")]
    public GameObject blockPiecePrefab;

    [Header("Drag Settings")]
    public float dragScaleMultiplier = 1.0f;

    // ─── Trạng thái kéo thả ───────────────────────────────────────────────
    public BlockPiece1 HeldPiece { get; private set; }

    private bool isFromGrid = false;
    private Vector2Int originalOriginCell;
    private ItemData1[] originalItems;
    private BlockShapeData originalShape;
    private Vector3 startWorldPosition;

    [Header("Level Flow")]
    public int currentLevelIndex = 0;

    // ─── Sprite Resources ─────────────────────────────────────────────────
    private SpriteFlavor spriteFlavor;
    private SpriteNumber spriteNumber;

    // ─── Unity Lifecycle ──────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        spriteFlavor = Resources.Load<SpriteFlavor>("SpriteFlavor");
        if (spriteFlavor == null) Debug.LogError("[GameManager1] Không tìm thấy SpriteFlavor.asset trong Resources/");

        spriteNumber = Resources.Load<SpriteNumber>("SpriteNumber");
        if (spriteNumber == null) Debug.LogError("[GameManager1] Không tìm thấy SpriteNumber.asset trong Resources/");
    }

    // ─── Level Management API ─────────────────────────────────────────────

    /// <summary>
    /// Chuyển sang level tiếp theo: Tăng level hiện tại lên 1, dọn bàn cờ và spawn lại toàn bộ dữ liệu mới.
    /// Có thể gán trực tiếp vào Button OnClick() trong Unity UI.
    /// </summary>
    public void NextLevel()
    {
        LoadLevel(currentLevelIndex + 1);
    }

    /// <summary>
    /// Nạp dữ liệu level theo index (levelNumber).
    /// </summary>
    public void LoadLevel(int targetLevel)
    {
        currentLevelIndex = targetLevel;
        Debug.Log($"[GameManager1] === Đang chuyển sang Level {targetLevel} ===");

        // 1. Dọn sạch các khối đã đặt trên Grid và reset tổng vị
        if (GridManager1.Instance != null)
        {
            GridManager1.Instance.ClearAllGrid();
        }

        // 2. Dọn và sinh lại các món ăn trên khay theo Level mới
        if (BlockSpawner1.Instance != null)
        {
            BlockSpawner1.Instance.levelNumber = targetLevel;
            BlockSpawner1.Instance.ClearAllPieces();
            BlockSpawner1.Instance.SpawnAllSlotsInLevelConfig(BlockSpawner1.Instance.levelConfigs);
        }

        // 3. Dọn và sinh lại danh sách Khách hàng theo Level mới
        if (CustomerSpawner1.Instance != null)
        {
            CustomerSpawner1.Instance.levelNumber = targetLevel;
            CustomerSpawner1.Instance.SpawnCustomersForLevel(targetLevel);
        }

        // 4. Reset điểm số, lượt đi và ẩn bảng thông báo kết quả
        if (ScoringSystem1.Instance != null)
        {
            ScoringSystem1.Instance.ResetGameStatus();
        }
    }

    // ─── API cho InputManager1 ────────────────────────────────────────────

    /// <summary>Bốc khối đang nằm trên Khay.</summary>
    public void BeginPickFromTray(BlockPiece1 piece)
    {
        HeldPiece = piece;
        startWorldPosition = piece.transform.position;
        isFromGrid = false;
        HeldPiece.transform.localScale = Vector3.one * dragScaleMultiplier;
    }

    /// <summary>Bốc khối đang nằm trên Grid.</summary>
    public void BeginPickFromGrid(PlacedBlockInfo1 info)
    {
        originalShape = info.shapeData;
        originalOriginCell = info.originCell;
        originalItems = info.itemPerCell;

        startWorldPosition = GridManager1.Instance.CellToWorld(originalOriginCell.x, originalOriginCell.y);

        // Xóa khỏi Grid (tự trừ flavor trong totalFlavorCounts)
        GridManager1.Instance.RemovePlacedBlock(originalShape, originalOriginCell);

        // Sinh khối tạm để kéo
        GameObject pieceObj = Instantiate(blockPiecePrefab, startWorldPosition, Quaternion.identity);
        HeldPiece = pieceObj.GetComponent<BlockPiece1>();
        HeldPiece.InitializeCustom(originalShape, originalItems, -1);
        SetupPieceBubble(HeldPiece);

        isFromGrid = true;
        HeldPiece.transform.localScale = Vector3.one * dragScaleMultiplier;
    }

    /// <summary>Cập nhật vị trí khối đang kéo mỗi frame.</summary>
    public void UpdateDrag(Vector3 targetWorldPos)
    {
        if (HeldPiece == null) return;
        HeldPiece.transform.position = targetWorldPos;
        Physics2D.SyncTransforms();
    }

    /// <summary>Xử lý thả khối — quyết định đặt, trả về, hay hủy.</summary>
    public void EndDrag(Vector3 releaseWorldPos)
    {
        if (HeldPiece == null) return;

        Vector2Int targetCell = GridManager1.Instance.WorldToCell(HeldPiece.transform.position);

        if (GridManager1.Instance.CanPlace(HeldPiece.shapeData, targetCell))
        {
            // ── Đặt hợp lệ vào Grid ──────────────────────────────────────
            bool isNewPosition = !isFromGrid || (targetCell != originalOriginCell);

            GridManager1.Instance.PlaceBlock(
                HeldPiece.shapeData, targetCell, HeldPiece.itemPerCell, HeldPiece.cellIconPrefab);

            if (isNewPosition && ScoringSystem1.Instance != null)
                ScoringSystem1.Instance.availableMoves--;

            // Thông báo cho Spawner nếu khối đến từ khay
            if (HeldPiece.slotIndex >= 0 && BlockSpawner1.Instance != null)
                BlockSpawner1.Instance.OnPiecePlaced(HeldPiece.slotIndex);

            Destroy(HeldPiece.gameObject);

            // Kiểm tra win/lose sau khi đặt khối
            if (ScoringSystem1.Instance != null) ScoringSystem1.Instance.NotifyGridChanged();
        }
        else if (isFromGrid && releaseWorldPos.y < -2.0f)
        {
            // ── Thả vào vùng Khay (y thấp hơn -2) ───────────────────────
            bool returnedToTray = BlockSpawner1.Instance != null
                && BlockSpawner1.Instance.TryReturnToTray(HeldPiece);

            if (returnedToTray)
            {
                // Thành công → tính 1 lượt di chuyển
                if (ScoringSystem1.Instance != null)
                    ScoringSystem1.Instance.availableMoves--;
                // Grid đã thay đổi (bỏ khối đi): kiểm tra lại
                if (ScoringSystem1.Instance != null) ScoringSystem1.Instance.NotifyGridChanged();
            }
            else
            {
                // Khay đầy → hoàn trả về Grid cũ, không tính lượt
                GridManager1.Instance.PlaceBlock(
                    HeldPiece.shapeData, originalOriginCell, originalItems, HeldPiece.cellIconPrefab);
                Destroy(HeldPiece.gameObject);
                if (ScoringSystem1.Instance != null) ScoringSystem1.Instance.NotifyGridChanged();
            }
        }
        else if (isFromGrid)
        {
            // ── Tha khong hop le tren Grid → tra ve vi tri cu, khong tinh luot ──
            GridManager1.Instance.PlaceBlock(
                HeldPiece.shapeData, originalOriginCell, originalItems, HeldPiece.cellIconPrefab);
            Destroy(HeldPiece.gameObject);
            if (ScoringSystem1.Instance != null) ScoringSystem1.Instance.NotifyGridChanged();
        }
        else
        {
            // ── Từ Khay, thả không hợp lệ → trả về Khay ─────────────────
            HeldPiece.transform.position = startWorldPosition;
            HeldPiece.transform.localScale = BlockSpawner1.Instance != null
                ? BlockSpawner1.Instance.trayBlockScale
                : new Vector3(0.6f, 0.6f, 0.6f);
        }

        HeldPiece = null;
        isFromGrid = false;
    }

    // ─── Bubble Helpers (dùng chung cho Spawner, Customer) ───────────────

    /// <summary>Xây danh sách Sprite (số + vị) từ ItemData1. Tối đa 4 sprite (lưới 2×2).</summary>
    public List<Sprite> BuildBubbleSprites(ItemData1 item)
    {
        var sprites = new List<Sprite>();
        if (item == null || item.flavorCounts == null) return sprites;

        string[] flavorNames = { "sour", "spicy", "salty", "sweet", "bitter", "umami", "buttery" };
        foreach (string flv in flavorNames)
        {
            int count = item.GetFlavorCount(flv);
            if (count <= 0) continue;
            if (sprites.Count + 2 > 4) break;
            // Them Icon Vi truoc (cot trai), Icon So sau (cot phai)
            if (spriteFlavor != null) sprites.Add(spriteFlavor.GetSprite(flv));
            if (spriteNumber != null) sprites.Add(spriteNumber.GetSprite(count));
        }
        return sprites;
    }

    /// <summary>Xây danh sách Sprite từ List&lt;FlavorData&gt; (dùng cho Customer1).</summary>
    public List<Sprite> BuildBubbleSpritesFromRequirement(List<FlavorData> requirements)
    {
        var sprites = new List<Sprite>();
        if (requirements == null) return sprites;

        string[] flavorNames = { "sour", "spicy", "salty", "sweet", "bitter", "umami", "buttery" };
        foreach (string flv in flavorNames)
        {
            var fd = requirements.Find(f => f.flavorName == flv);
            if (fd.count <= 0) continue;
            if (sprites.Count + 2 > 4) break;
            // Them Icon Vi truoc (cot trai), Icon So sau (cot phai)
            if (spriteFlavor != null) sprites.Add(spriteFlavor.GetSprite(flv));
            if (spriteNumber != null) sprites.Add(spriteNumber.GetSprite(fd.count));
        }
        return sprites;
    }

    /// <summary>Tìm Bubble trên khối và setup sprite vị.</summary>
    public void SetupPieceBubble(BlockPiece1 piece)
    {
        if (piece == null || piece.itemPerCell == null || piece.itemPerCell.Length == 0) return;
        Bubble bubble = piece.GetComponentInChildren<Bubble>();
        if (bubble == null) return;
        bubble.SetupBubble(BuildBubbleSprites(piece.itemPerCell[0]));
    }
}
