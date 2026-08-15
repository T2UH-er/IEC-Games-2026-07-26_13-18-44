using UnityEngine;

public class InputManager1 : MonoBehaviour
{
    public static InputManager1 Instance { get; private set; }

    // Tuong thich nguoc voi TrayScrollManager1 dang doc selectedPiece
    public BlockPiece1 selectedPiece => GameManager1.Instance != null ? GameManager1.Instance.HeldPiece : null;

    private Vector3 dragOffset;
    private Camera mainCam;

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
                // Uu tien: khoi dang nam tren Khay
                BlockPiece1 piece = hit.collider.GetComponentInParent<BlockPiece1>();
                if (piece != null)
                {
                    dragOffset = piece.transform.position - worldPos;
                    GameManager1.Instance.BeginPickFromTray(piece);
                    return;
                }

                // Thu hai: khoi da dat tren Grid
                PlacedBlockInfo1 info = hit.collider.GetComponentInParent<PlacedBlockInfo1>();
                if (info != null)
                {
                    GameManager1.Instance.BeginPickFromGrid(info);
                    if (GameManager1.Instance.HeldPiece != null)
                        dragOffset = GameManager1.Instance.HeldPiece.transform.position - worldPos;
                }
            }
        }

        if (GameManager1.Instance != null && GameManager1.Instance.HeldPiece != null && Input.GetMouseButton(0))
        {
            GameManager1.Instance.UpdateDrag(GetMouseWorldPos() + dragOffset);
        }

        if (GameManager1.Instance != null && GameManager1.Instance.HeldPiece != null && Input.GetMouseButtonUp(0))
        {
            GameManager1.Instance.EndDrag(GetMouseWorldPos());
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
