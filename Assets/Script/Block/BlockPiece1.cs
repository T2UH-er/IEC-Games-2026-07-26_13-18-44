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

    public float dishScale = 1f;
    public float iconScale = 1f;
    public float shapeCellSpacing = 1.0f;
    public bool showDishBackground = false;

    [Header("Bubble Settings")]
    [Tooltip("Độ lệch vị trí Bubble so với món ăn (chỉnh tự do trong Inspector)")]
    public Vector3 bubbleOffset = new Vector3(0f, -1.2f, 0f);

    [Tooltip("Tỷ lệ scale của Bubble trên món ăn")]
    public float bubbleScale = 1.2f;

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

    void BuildVisual()
    {
        float gSize = GridManager1.Instance != null ? GridManager1.Instance.cellSize : 1f;
        float cs = gSize * shapeCellSpacing;

        if (showDishBackground && shapeData != null && shapeData.dishSprite != null)
        {
            GameObject dishObj = new GameObject("DishBackground");
            dishObj.transform.SetParent(transform, false);
            Vector2 center = shapeData.GetCenterOffset();
            dishObj.transform.localPosition = new Vector3(center.x * cs, center.y * cs, 0f) + shapeData.dishOffset;
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
            icon.transform.localPosition = new Vector3(offset.x * cs, offset.y * cs, 0f);
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

            bubble.transform.localPosition = new Vector3(center.x * cs, baseY * cs, 0f) + bubbleOffset;
            bubble.transform.localScale = Vector3.one * bubbleScale;
        }
    }
}
