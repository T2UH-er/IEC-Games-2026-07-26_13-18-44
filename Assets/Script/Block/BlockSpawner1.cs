using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BlockSpawner1: MonoBehaviour
{
    public int levelNumber = 0;

    public SpriteFlavor flavor;
    public SpriteNumber number;
    public static BlockSpawner1 Instance { get; private set; }

    public GameObject blockPiecePrefab;
    public List<Transform> spawnSlots = new List<Transform> ();

    public LevelConfig[] levelConfigs;

    public Transform trayContainer;
    private Vector3 initialTrayPosition; // Biến lưu vị trí Y ban đầu của Tray
    public Vector3 trayBlockScale = new Vector3(0.6f, 0.6f, 0.6f);
    public int totalBlocksCount = 10;      
    public float slotSpacing = 20f; 
    [SerializeField] private BlockPiece1[] currentPieces;
    private Vector3[] slotPositions;

    // 2. Khởi tạo mảng slotPositions trong Awake
    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
        // 1. Lưu lại vị trí chuẩn của Tray trong Scene (ví dụ: Y = -23)
        if (trayContainer != null)
        {
            initialTrayPosition = trayContainer.localPosition;
        }
        currentPieces = new BlockPiece1[totalBlocksCount];
        slotPositions = new Vector3[totalBlocksCount];
        for (int i = 0; i < totalBlocksCount; i++)
        {
            slotPositions[i] = GetSlotPosition(i);
        }

        flavor = Resources.Load<SpriteFlavor>("SpriteFlavor");
        if (flavor == null) Debug.LogError("Không có SpriteFlavor.asset");
        number = Resources.Load<SpriteNumber>("SpriteNumber");
        if (number == null) Debug.LogError("Không có SpriteNumber.asset");
    }


    // 1. Hàm tính toán vị trí local chuẩn cho bất kỳ slotIndex nào
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

    public void SpawnAllSlotsInLevelConfig(LevelConfig[] levelConfigs)
    {
        foreach (var levelConfig in levelConfigs)
        {
            if (levelConfig == null || levelConfig.blockConfig == null || levelConfig.blockConfig.itemPerCell == null)
            {
                Debug.LogWarning("Invalid LevelConfig or BlockConfig.");
                return;
            }

            Vector3 localSpawnPos = new Vector3((levelConfig.slotIndex - (totalBlocksCount / 2)) * slotSpacing, 0f, 0f);
            slotPositions[levelConfig.slotIndex] = localSpawnPos;
            GameObject pieceObj = Instantiate(blockPiecePrefab, trayContainer);
            pieceObj.transform.localPosition = localSpawnPos;
            pieceObj.transform.localScale = trayBlockScale;
            BlockPiece1 piece = pieceObj.GetComponent<BlockPiece1>();
            piece.InitializeCustom(levelConfig.blockConfig.shapeData, levelConfig.blockConfig.itemPerCell, levelConfig.slotIndex);
            // Tạo sprite hiển thị vị cho bubble
            Bubble bubble = piece.GetComponentInChildren<Bubble>();
            List<Sprite> sprites = new List<Sprite>();
            ItemData1 itDt1 = piece.itemPerCell[0];

            // Thêm "new string[]" để sửa lỗi cú pháp
            string[] flavors = new string[] { "sour", "spicy", "salty", "sweet", "bitter", "umami", "buttery" };

            foreach (string flv in flavors)
            {
                int count = itDt1.GetFlavorCount(flv);
                if (count > 0)
                {
                    // Kiểm tra an toàn: Đảm bảo không vượt quá 4 Sprite nếu dùng cho Lưới 2x2
                    if (sprites.Count + 2 > 4)
                    {
                        Debug.LogWarning("[Bubble] Đã đạt tối đa 4 Sprite, dừng thêm vị mới!");
                        break;
                    }

                    sprites.Add(number.GetSprite(count));
                    sprites.Add(flavor.GetSprite(flv));
                }
            }

            bubble.SetupBubble(sprites);

            Debug.Log("ItemPerCell loaded");
            currentPieces[levelConfig.slotIndex] = piece;
        }
    }

    public void ConfigureFlavor()
    {
        if (levelConfigs == null) return;

        for (int i = 0; i < levelConfigs.Length; i++)
        {
            var levelConfig = levelConfigs[i];
            if (levelConfig == null || levelConfig.levelNumber != levelNumber) continue;

            if (levelConfig.blockConfig != null && levelConfig.blockConfig.itemPerCell != null)
            {
                foreach (ItemData1 itemdata in levelConfig.blockConfig.itemPerCell)
                {
                    if (itemdata != null)
                    {
                        itemdata.flavorCounts = levelConfig.flavorCounts;
                        itemdata.itemId = i.ToString();
                    }
                }
            }
        }
    }

    public void OnPiecePlaced(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < currentPieces.Length)
        {
            currentPieces[slotIndex] = null;
        }
        bool allEmpty = true;
        for (int i = 0; i < currentPieces.Length; i++)
        {
            if (currentPieces[i] != null)
            {
                allEmpty = false;
                break;
            }
        }
        if (allEmpty)
        {
            // 2. Trả trayContainer về đúng vị trí ban đầu thay vì gán -4.5f
            trayContainer.localPosition = initialTrayPosition;
        }
    }
    // 3. Hàm trả món ăn về Tray
    public bool TryReturnToTray(BlockPiece1 piece)
    {
        int emptySlot = -1;
        for (int i = 0; i < currentPieces.Length; i++)
        {
            if (currentPieces[i] == null)
            {
                emptySlot = i;
                break;
            }
        }
        if (emptySlot != -1)
        {
            piece.transform.SetParent(trayContainer);
            // Đặt vị trí local chính xác theo ô trống
            piece.transform.localPosition = GetSlotPosition(emptySlot);
            piece.transform.localScale = trayBlockScale;
            piece.slotIndex = emptySlot;
            currentPieces[emptySlot] = piece;
            // Đồng bộ lại Physics2D để Collider di chuyển theo món ăn trên Tray ngay lập tức
            Physics2D.SyncTransforms();
            return true;
        }

        Debug.Log("No Place Left on Tray");
        return false;
    }
}
