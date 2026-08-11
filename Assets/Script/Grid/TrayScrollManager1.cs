using UnityEngine;

public class TrayScrollManager1 : MonoBehaviour
{
    public Transform trayContainer;
    public float minX = -9.6f;
    public float maxX = 0.0f; 
    public float scrollSensitivity = 3f;
    public Transform trayBoard;
    public float boardPadding = 2.0f;

    private Vector3 lastMousePos;
    private void Start()
    {
        AutoResizeTrayBoard();
        CalculateScrollBounds();
    }
    
    
    public void AutoResizeTrayBoard()
    {
        if (trayBoard == null) return;
        // 1. Tính tổng độ rộng cần thiết
        float requiredWidth = (BlockSpawner1.Instance.totalBlocksCount * BlockSpawner1.Instance.slotSpacing) + boardPadding;
        // 2. Kiểm tra nếu SpriteRenderer đang ở chế độ Sliced (9-Slice)
        SpriteRenderer sr = trayBoard.GetComponent<SpriteRenderer>();
        if (sr != null && sr.drawMode == SpriteDrawMode.Sliced)
        {
            sr.size = new Vector2(requiredWidth, sr.size.y);
        }
        else
        {
            // 3. Co giãn Scale X theo độ rộng cần thiết
            Vector3 scale = trayBoard.localScale;
            scale.x = requiredWidth / 10f; // (10f là độ rộng gốc của sprite)
            trayBoard.localScale = scale;
        }
    }
    public void CalculateScrollBounds()
    {
        if (BlockSpawner1.Instance != null)
        {
            int N = BlockSpawner1.Instance.totalBlocksCount;
            float S = BlockSpawner1.Instance.slotSpacing;
            float halfN = N / 2f;
            maxX = halfN * S;                     
            minX = -((N - 1) - halfN) * S;       
        }
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePos = Input.mousePosition;
        }

        
        if (Input.GetMouseButton(0) && InputManager1.Instance != null && InputManager1.Instance.selectedPiece == null)
        {
            Vector3 delta = Input.mousePosition - lastMousePos;


            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y) && Mathf.Abs(delta.x) > 1.5f)
            {
                float moveX = delta.x * 0.01f * scrollSensitivity;
                Vector3 newPos = trayContainer.position + new Vector3(moveX, 0, 0);
                newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
                trayContainer.position = newPos;
                // ÉP HỆ THỐNG PHYSICS2D ĐỒNG BỘ COLLIDER THEO POSITION MỚI NGAY LẬP TỨC
                Physics2D.SyncTransforms();
            }

            lastMousePos = Input.mousePosition;
        }
    }
}
