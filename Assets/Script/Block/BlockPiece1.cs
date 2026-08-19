using UnityEngine;

/// <summary>
/// BlockPiece1 — Luu data va hien thi visual cua 1 khoi mon an.
/// Khong chua game logic (TryPlace da chuyen sang GameManager1).
/// </summary>
public class BlockPiece1 : MonoBehaviour
{
    public BlockShapeData shapeData;
    public ItemData1[] itemPerCell;
    public GameObject cellIconPrefab;
    public int slotIndex;
    public float currentRotationAngle = 0f;

    public float dishScale = 1f;
    public float iconScale = 1f;
    public float shapeCellSpacing = 1.0f;
    public bool showDishBackground = false;

    [Header("Bubble Settings")]
    [Tooltip("Độ lệch vị trí Bubble so với món ăn (chỉnh tự do trong Inspector)")]
    public Vector3 bubbleOffset = new Vector3(0f, -0.7f, 0f);

    [Tooltip("Tỷ lệ scale của Bubble trên món ăn")]
    public float bubbleScale = 1.0f;

    [Tooltip("True: Căn Bubble theo cạnh đáy món ăn; False: Căn theo tâm món ăn")]
    public bool alignBubbleToBottom = true;

    public void Initialize(BlockShapeData shape, ItemDatabase1 itemDb, int slot)
    {
        shapeData = shape;
        slotIndex = slot;

        itemPerCell = new ItemData1[shape.CellCount];
        ItemData1 randomItem = itemDb.GetRandomItem();
        for (int i = 0; i < shape.CellCount; i++) itemPerCell[i] = randomItem;
        BuildVisual();
    }

    public void InitializeCustom(BlockShapeData shape, ItemData1[] items, int slot)
    {
        shapeData = shape;
        slotIndex = slot;
        itemPerCell = items;
        BuildVisual();
    }

    private bool isRotating = false;
    public bool IsRotating => isRotating;
    public static System.Action<BlockPiece1> OnPieceRotated;

    public void TriggerRotate(float duration = 0.18f)
    {
        if (isRotating || shapeData == null) return;
        OnPieceRotated?.Invoke(this);
        StartCoroutine(RotateRoutine(duration));
    }

    private System.Collections.IEnumerator RotateRoutine(float duration)
    {
        AudioManager.Instance.PlayAudio("rotate");
        isRotating = true;
        Quaternion startRot = transform.localRotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, 0f, -90f);

        Transform bubbleT = GetComponentInChildren<Bubble>()?.transform;
        Quaternion bubbleStartRot = bubbleT != null ? bubbleT.rotation : Quaternion.identity;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Ease-out cubic
            float easeT = 1f - Mathf.Pow(1f - t, 3f);
            transform.localRotation = Quaternion.Slerp(startRot, endRot, easeT);

            // Giữ Bubble luôn thẳng đứng không bị xoay lộn ngược
            if (bubbleT != null)
                bubbleT.rotation = bubbleStartRot;

            yield return null;
        }

        transform.localRotation = Quaternion.identity;
        if (bubbleT != null)
            bubbleT.rotation = Quaternion.identity;

        shapeData = shapeData.GetRotatedClockwiseShape();
        currentRotationAngle -= 90f;

        ClearVisual();
        BuildVisual();

        Physics2D.SyncTransforms();
        isRotating = false;
    }

    public void ClearVisual()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            // Giữ lại Bubble nếu có
            if (child.GetComponent<Bubble>() != null || child.name.ToLower().Contains("bubble"))
                continue;

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }

    public void BuildVisual()
    {
        float gSize = GridManager1.Instance != null ? GridManager1.Instance.baseCellSize : 7.2f;
        float cs = gSize * shapeCellSpacing;

        if (showDishBackground && shapeData != null && shapeData.dishSprite != null)
        {
            GameObject dishObj = new GameObject("DishBackground");
            dishObj.transform.SetParent(transform, false);
            Vector2 center = shapeData.GetCenterOffset();
            dishObj.transform.localPosition = new Vector3(center.x * cs, center.y * cs, 0f) + shapeData.dishOffset;
            dishObj.transform.localRotation = Quaternion.Euler(0f, 0f, currentRotationAngle);
            dishObj.transform.localScale = new Vector3(dishScale, dishScale, 1f);

            SpriteRenderer dishSr = dishObj.AddComponent<SpriteRenderer>();
            dishSr.sprite = shapeData.dishSprite;
            dishSr.sortingOrder = 5;
        }

        for (int i = 0; i < shapeData.cells.Length; i++)
        {
            Vector2Int offset = shapeData.cells[i];
            if (cellIconPrefab == null) continue;

            GameObject icon = Instantiate(cellIconPrefab, transform);
            
            // Xử lý bù trừ vị trí do pivot của sprite nằm ở Bottom-Left (0,0)
            Vector3 pivotOffset = Vector3.zero;
            int angle = Mathf.RoundToInt(currentRotationAngle) % 360;
            if (angle < 0) angle += 360;
            if (angle == 270) pivotOffset = new Vector3(0, cs, 0);       // -90 độ
            else if (angle == 180) pivotOffset = new Vector3(cs, cs, 0); // -180 độ
            else if (angle == 90) pivotOffset = new Vector3(cs, 0, 0);   // -270 độ (+90)

            icon.transform.localPosition = new Vector3(offset.x * cs, offset.y * cs, 0f) + pivotOffset;
            icon.transform.localRotation = Quaternion.Euler(0f, 0f, currentRotationAngle);
            icon.transform.localScale = Vector3.one;

            SpriteRenderer[] srs = icon.GetComponentsInChildren<SpriteRenderer>();
            for (int j = 0; j < srs.Length; j++) srs[j].sortingOrder += 20;

            if (srs.Length > 0 && itemPerCell != null && i < itemPerCell.Length && itemPerCell[i] != null)
            {
                SpriteRenderer targetSr = srs.Length > 1 ? srs[srs.Length - 1] : srs[0];
                if (targetSr != null && itemPerCell[i].icon != null)
                {
                    targetSr.sprite = itemPerCell[i].icon;
                    if (targetSr.sprite != null && targetSr.sprite.bounds.size.x > 0)
                    {
                        float spriteSize = Mathf.Max(targetSr.sprite.bounds.size.x, targetSr.sprite.bounds.size.y);
                        float fitScale = (cs / spriteSize) * iconScale;
                        targetSr.transform.localScale = new Vector3(fitScale, fitScale, 1f);
                    }
                }
            }
        }

        // Tự động căn chỉnh vị trí & scale của Bubble theo thiết lập Inspector
        Bubble bubble = GetComponentInChildren<Bubble>();
        if (bubble != null && shapeData != null)
        {
            Vector2 center = shapeData.GetCenterOffset();
            float baseY = center.y;

            if (alignBubbleToBottom && shapeData.cells != null && shapeData.cells.Length > 0)
            {
                float minY = shapeData.cells[0].y;
                for (int j = 1; j < shapeData.cells.Length; j++)
                    if (shapeData.cells[j].y < minY) minY = shapeData.cells[j].y;
                baseY = minY;
            }

            float scaleRatio = GridManager1.Instance != null ? GridManager1.Instance.ScaleRatio : 1f;
            bubble.transform.localPosition = new Vector3(center.x * cs, baseY * cs, 0f) + bubbleOffset * scaleRatio;
            bubble.transform.localScale = Vector3.one * (bubbleScale * scaleRatio);
        }
    }
}
