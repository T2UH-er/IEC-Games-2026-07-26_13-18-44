using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public GameObject blockPiecePrefab;

    public BlockPiece selectedPiece;
    private Vector3 startPosition;
    private Vector3 dragOffset;
    private Camera mainCam;
    private bool isFromGrid = false;
    private Vector2Int originalOriginCell;
    private ItemData[] originalItems;

    private void Awake()
    {
        Instance = this;
        mainCam = Camera.main != null ? Camera.main : FindObjectOfType<Camera>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = GetMouseWorldPos();
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider != null)
            {
                BlockPiece piece = hit.collider.GetComponentInParent<BlockPiece>();
                if (piece != null)
                {
                    selectedPiece = piece;
                    startPosition = piece.transform.position;
                    dragOffset = piece.transform.position - worldPos;
                    isFromGrid = false;
                    selectedPiece.transform.localScale = Vector3.one * 1.1f;
                    return;
                }

                PlacedBlockInfo placedInfo = hit.collider.GetComponentInParent<PlacedBlockInfo>();
                if (placedInfo != null)
                {
                    BlockShapeData shape = placedInfo.shapeData;
                    originalOriginCell = placedInfo.originCell;
                    originalItems = placedInfo.itemPerCell;

                    GridManager.Instance.RemovePlacedBlock(shape, originalOriginCell);

                    GameObject pieceObj = Instantiate(blockPiecePrefab, worldPos, Quaternion.identity);
                    selectedPiece = pieceObj.GetComponent<BlockPiece>();
                    selectedPiece.InitializeCustom(shape, originalItems, -1);

                    startPosition = GridManager.Instance.CellToWorld(originalOriginCell.x, originalOriginCell.y);
                    dragOffset = selectedPiece.transform.position - worldPos;
                    isFromGrid = true;
                    selectedPiece.transform.localScale = Vector3.one * 1.1f;
                }
            }
        }

        if (selectedPiece != null && Input.GetMouseButton(0))
        {
            Vector3 worldPos = GetMouseWorldPos();
            selectedPiece.transform.position = worldPos + dragOffset;
        }
        if (selectedPiece != null && Input.GetMouseButtonUp(0))
        {
            bool placed = selectedPiece.TryPlace();
            if (!placed)
            {
                Vector3 worldPos = GetMouseWorldPos();
                if (isFromGrid && worldPos.y < -2.0f)
                {
                    bool returnedToTray = (BlockSpawner.Instance != null) && BlockSpawner.Instance.TryReturnToTray(selectedPiece);
                    if (!returnedToTray)
                    {
                        
                        GridManager.Instance.PlaceBlock(selectedPiece.shapeData, originalOriginCell, originalItems, selectedPiece.cellIconPrefab);
                        Destroy(selectedPiece.gameObject);
                    }
                } 
                else if (isFromGrid)
                {
                    GridManager.Instance.PlaceBlock(selectedPiece.shapeData, originalOriginCell, originalItems, selectedPiece.cellIconPrefab);
                    Destroy(selectedPiece.gameObject);
                }
                else
                {
                    selectedPiece.transform.position = startPosition;
                    selectedPiece.transform.localScale = (BlockSpawner.Instance != null) ? BlockSpawner.Instance.trayBlockScale : new Vector3(0.6f, 0.6f, 0.6f);
                }
            }

            selectedPiece = null;
            isFromGrid = false;
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        if (mainCam == null) mainCam = Camera.main;
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = -mainCam.transform.position.z;
        return mainCam.ScreenToWorldPoint(mouseScreen);
    }
}
