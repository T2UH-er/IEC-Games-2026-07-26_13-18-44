using UnityEngine;

public class InputManager1 : MonoBehaviour
{
    public static InputManager1 Instance { get; private set; }

    public GameObject blockPiecePrefab;

    public BlockPiece1 selectedPiece;
    private Vector3 startPosition;
    private Vector3 dragOffset;
    private Camera mainCam;
    private bool isFromGrid = false;
    private Vector2Int originalOriginCell;
    private ItemData1[] originalItems;

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
                BlockPiece1 piece = hit.collider.GetComponentInParent<BlockPiece1>();
                if (piece != null)
                {
                    selectedPiece = piece;
                    startPosition = piece.transform.position;
                    dragOffset = piece.transform.position - worldPos;
                    isFromGrid = false;
                    selectedPiece.transform.localScale = Vector3.one * 1.1f;
                    return;
                }

                PlacedBlockInfo1 placedInfo = hit.collider.GetComponentInParent<PlacedBlockInfo1>();
                if (placedInfo != null)
                {
                    BlockShapeData shape = placedInfo.shapeData;
                    originalOriginCell = placedInfo.originCell;
                    originalItems = placedInfo.itemPerCell;

                    // 1. Tính toán vị trí thế giới gốc của món ăn trên Grid trước
                    startPosition = GridManager1.Instance.CellToWorld(originalOriginCell.x, originalOriginCell.y);

                    // 2. Xóa món ăn cũ khỏi Grid
                    GridManager1.Instance.RemovePlacedBlock(shape, originalOriginCell);

                    // 3. Tạo GameObject mới tại ĐÚNG startPosition (thay vì worldPos)
                    GameObject pieceObj = Instantiate(blockPiecePrefab, startPosition, Quaternion.identity);
                    selectedPiece = pieceObj.GetComponent<BlockPiece1>();
                    selectedPiece.InitializeCustom(shape, originalItems, -1);

                    // 4. Tính dragOffset chuẩn xác giữa món ăn và vị trí con trỏ chuột
                    dragOffset = startPosition - worldPos;
                    isFromGrid = true;
                    selectedPiece.transform.localScale = Vector3.one * 1.1f;
                }
            }
        }

        if (selectedPiece != null && Input.GetMouseButton(0))
        {
            Vector3 worldPos = GetMouseWorldPos();
            selectedPiece.transform.position = worldPos + dragOffset;
            // Đảm bảo Collider của Block đang kéo cũng đi theo Sprite ngay trong khung hình này
            Physics2D.SyncTransforms();
        }
        if (selectedPiece != null && Input.GetMouseButtonUp(0))
        {
            bool placed = selectedPiece.TryPlace();
            if (!placed)
            {
                Vector3 worldPos = GetMouseWorldPos();
                if (isFromGrid && worldPos.y < -2.0f)
                {
                    bool returnedToTray = (BlockSpawner1.Instance != null) && BlockSpawner1.Instance.TryReturnToTray(selectedPiece);
                    if (!returnedToTray)
                    {
                        
                        GridManager1.Instance.PlaceBlock(selectedPiece.shapeData, originalOriginCell, originalItems, selectedPiece.cellIconPrefab);
                        Destroy(selectedPiece.gameObject);
                    }
                } 
                else if (isFromGrid)
                {
                    GridManager1.Instance.PlaceBlock(selectedPiece.shapeData, originalOriginCell, originalItems, selectedPiece.cellIconPrefab);
                    Destroy(selectedPiece.gameObject);
                }
                else
                {
                    selectedPiece.transform.position = startPosition;
                    selectedPiece.transform.localScale = (BlockSpawner1.Instance != null) ? BlockSpawner1.Instance.trayBlockScale : new Vector3(0.6f, 0.6f, 0.6f);
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
