using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner1 : MonoBehaviour
{
    public int levelNumber = 0;
    public static BlockSpawner1 Instance { get; private set; }

    public GameObject blockPiecePrefab;
    public List<Transform> spawnSlots = new List<Transform>();
    public LevelConfig[] levelConfigs;

    public Transform trayContainer;
    private Vector3 initialTrayPosition;
    public Vector3 trayBlockScale = new Vector3(0.6f, 0.6f, 0.6f);
    public int totalBlocksCount = 10;
    public float slotSpacing = 20f;

    [SerializeField] private BlockPiece1[] currentPieces;
    private Vector3[] slotPositions;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (trayContainer != null)
            initialTrayPosition = trayContainer.localPosition;

        currentPieces = new BlockPiece1[totalBlocksCount];
        slotPositions = new Vector3[totalBlocksCount];
        for (int i = 0; i < totalBlocksCount; i++)
            slotPositions[i] = GetSlotPosition(i);
    }

    public Vector3 GetSlotPosition(int slotIndex)
    {
        float offsetX = (slotIndex - (totalBlocksCount / 2f) + 0.5f) * slotSpacing;
        return new Vector3(offsetX, 0f, 0f);
    }

    private void Start()
    {
        ConfigureFlavor();
        SpawnAllSlotsInLevelConfig(levelConfigs);
    }

    public void SpawnAllSlotsInLevelConfig(LevelConfig[] configs)
    {
        if (configs == null || configs.Length == 0)
        {
            configs = Resources.FindObjectsOfTypeAll<LevelConfig>();
        }
        if (configs == null) return;

        foreach (var levelConfig in configs)
        {
            if (levelConfig == null || levelConfig.levelNumber != levelNumber) continue;

            // ─── Cách Mới: Sinh từ danh sách trayBlocks trong 1 file LevelConfig ───
            if (levelConfig.trayBlocks != null && levelConfig.trayBlocks.Count > 0)
            {
                foreach (var blockCfg in levelConfig.trayBlocks)
                {
                    if (blockCfg == null || blockCfg.blockConfig == null || blockCfg.blockConfig.itemPerCell == null)
                        continue;

                    SpawnSinglePiece(blockCfg.blockConfig, blockCfg.slotIndex, blockCfg.flavorCounts);
                }
            }
            // ─── Tương thích ngược: Sinh từ asset đơn lẻ cũ ───────────────────────
            else if (levelConfig.blockConfig != null && levelConfig.blockConfig.itemPerCell != null)
            {
                SpawnSinglePiece(levelConfig.blockConfig, levelConfig.slotIndex, levelConfig.flavorCounts);
            }
        }
    }

    private void SpawnSinglePiece(BlockConfig blockConfig, int slotIndex, List<FlavorData> flavors)
    {
        if (slotIndex < 0 || slotIndex >= totalBlocksCount) return;

        // Clone ItemData để không ghi đè asset gốc
        ItemData1[] clonedItems = new ItemData1[blockConfig.itemPerCell.Length];
        for (int i = 0; i < blockConfig.itemPerCell.Length; i++)
        {
            if (blockConfig.itemPerCell[i] != null)
            {
                clonedItems[i] = Instantiate(blockConfig.itemPerCell[i]);
                if (flavors != null)
                {
                    clonedItems[i].flavorCounts = new List<FlavorData>(flavors);
                }
            }
        }

        Vector3 localSpawnPos = GetSlotPosition(slotIndex);
        slotPositions[slotIndex] = localSpawnPos;

        GameObject pieceObj = Instantiate(blockPiecePrefab, trayContainer);
        pieceObj.transform.localPosition = localSpawnPos;
        pieceObj.transform.localScale = trayBlockScale;

        BlockPiece1 piece = pieceObj.GetComponent<BlockPiece1>();
        piece.InitializeCustom(blockConfig.shapeData, clonedItems, slotIndex);

        // Hiển thị Bubble vị trên món ăn
        if (GameManager1.Instance != null)
            GameManager1.Instance.SetupPieceBubble(piece);

        currentPieces[slotIndex] = piece;
    }

    /// <summary>
    /// Xóa toàn bộ các khối món ăn đang có trên khay và trong scene.
    /// </summary>
    public void ClearAllPieces()
    {
        // 1. Xóa toàn bộ GameObject con trong trayContainer
        if (trayContainer != null)
        {
            for (int i = trayContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(trayContainer.GetChild(i).gameObject);
            }
        }

        // 2. Quét sạch tất cả BlockPiece1 còn sót lại trong Scene
        BlockPiece1[] allPieces = FindObjectsOfType<BlockPiece1>();
        foreach (var piece in allPieces)
        {
            if (piece != null)
                Destroy(piece.gameObject);
        }

        // 3. Reset mảng lưu trữ
        if (currentPieces != null)
        {
            for (int i = 0; i < currentPieces.Length; i++)
                currentPieces[i] = null;
        }
    }

    public void ConfigureFlavor()
    {
        // Đã được xử lý tự động và an toàn khi clone item trong SpawnSinglePiece
    }

    public void OnPiecePlaced(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < currentPieces.Length)
            currentPieces[slotIndex] = null;

        bool allEmpty = true;
        for (int i = 0; i < currentPieces.Length; i++)
            if (currentPieces[i] != null) { allEmpty = false; break; }

        if (allEmpty && trayContainer != null)
            trayContainer.localPosition = initialTrayPosition;
    }

    public bool TryReturnToTray(BlockPiece1 piece)
    {
        int emptySlot = -1;
        for (int i = 0; i < currentPieces.Length; i++)
            if (currentPieces[i] == null) { emptySlot = i; break; }

        if (emptySlot == -1)
        {
            Debug.Log("No Place Left on Tray");
            return false;
        }

        piece.transform.SetParent(trayContainer);
        piece.transform.localPosition = GetSlotPosition(emptySlot);
        piece.transform.localScale = trayBlockScale;
        piece.slotIndex = emptySlot;
        currentPieces[emptySlot] = piece;
        Physics2D.SyncTransforms();
        return true;
    }
}
