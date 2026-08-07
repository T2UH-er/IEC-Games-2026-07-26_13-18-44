using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    public static BlockSpawner Instance { get; private set; }

    public BlockShapeData[] allShapes;   
    public ItemDatabase itemDatabase;        
    public GameObject blockPiecePrefab;     
    public List<Transform> spawnSlots = new List<Transform> ();        

    public Transform trayContainer;     
    public Vector3 trayBlockScale = new Vector3(0.6f, 0.6f, 0.6f);
    public int totalBlocksCount = 10;      
    public float slotSpacing = 10f; 
    private BlockPiece[] currentPieces;
    private Vector3[] slotPositions;  

    private void Awake()
    {
        if(Instance == null){
            Instance = this;
        }
        else {
            Destroy(gameObject);
        }
        currentPieces = new BlockPiece[totalBlocksCount];
        slotPositions = new Vector3[totalBlocksCount];
    }

    private void Start()
    {
        SpawnAllSlots();
    }

    public void SpawnAllSlots()
    {
        for (int i = 0; i < totalBlocksCount; i++)
        {
            if (currentPieces[i] == null)
            {
                SpawnPieceAtSlot(i);
            }
        }
    }

    private void SpawnPieceAtSlot(int slotIndex)
    {
        if (allShapes == null || allShapes.Length == 0) return;

        BlockShapeData shape = allShapes[Random.Range(0, allShapes.Length)];

        Vector3 localSpawnPos = new Vector3((slotIndex-(totalBlocksCount/2)) * slotSpacing, 0f, 0f);
        slotPositions[slotIndex] = localSpawnPos;
        GameObject pieceObj = Instantiate(blockPiecePrefab, trayContainer);
        pieceObj.transform.localPosition = localSpawnPos;
        pieceObj.transform.localScale = trayBlockScale;
        BlockPiece piece = pieceObj.GetComponent<BlockPiece>();
        piece.Initialize(shape, itemDatabase, slotIndex);
        currentPieces[slotIndex] = piece;
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
            SpawnAllSlots();
        }
    }
    public bool TryReturnToTray(BlockPiece piece)
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
