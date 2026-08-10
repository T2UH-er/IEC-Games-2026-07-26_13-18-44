using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BlockSpawner1: MonoBehaviour
{
    public int levelNumber = 0;

    public static BlockSpawner1 Instance { get; private set; }

    public GameObject blockPiecePrefab;
    public List<Transform> spawnSlots = new List<Transform> ();

    public LevelConfig[] levelConfigs;

    public Transform trayContainer;     
    public Vector3 trayBlockScale = new Vector3(0.6f, 0.6f, 0.6f);
    public int totalBlocksCount = 10;      
    public float slotSpacing = 10f; 
    [SerializeField] private BlockPiece1[] currentPieces;
    private Vector3[] slotPositions;  

    private void Awake()
    {
        if(Instance == null){
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
        currentPieces = new BlockPiece1[totalBlocksCount];
        slotPositions = new Vector3[totalBlocksCount];
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
            Debug.Log("ItemPerCell loaded");
            currentPieces[levelConfig.slotIndex] = piece;
        }
    }

    public void ConfigureFlavor()
    {
        // Check if the level number exists in the levelConfigs array
        foreach (var levelConfig in levelConfigs)
        {
            if (levelConfig.levelNumber != levelNumber) return;
        }

        // Assign the flavorCounts from the levelConfig to each ItemData1 in the blockConfig's itemData array
        foreach (var levelConfig in levelConfigs)
        {
            foreach (ItemData1 itemdata in levelConfig.blockConfig.itemPerCell)
            {
                itemdata.flavorCounts = levelConfig.flavorCounts;
            }
        }
    }

    public void OnPiecePlaced(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < currentPieces.Length)
        {
            currentPieces[slotIndex] = null;
        }
        Debug.Log("idx"+ slotIndex);
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
            trayContainer.localPosition = new Vector3(0f, -4.5f, 0f);
        }
    }
    public bool TryReturnToTray(BlockPiece1 piece)
    {
        int emptySlot = -1;
        for (int i = 0; i < currentPieces.Length; i++)
        {
            //Debug.Log("idx"+ i + " "+ currentPieces[i]);
            if (currentPieces[i] == null)
            {
                emptySlot = i;
                break;
            }
        }

        if (emptySlot != -1)
        {
            piece.transform.SetParent(trayContainer);
            piece.transform.localPosition = slotPositions[emptySlot];
            piece.transform.localScale = trayBlockScale;
            piece.slotIndex = emptySlot;
            currentPieces[emptySlot] = piece;
            return true;
        }
        Debug.Log("No Place");
        return false;
    }
}
