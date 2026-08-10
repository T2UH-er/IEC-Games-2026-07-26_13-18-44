using UnityEngine;
public class BlockPiece1 : MonoBehaviour
{
    public BlockShapeData shapeData;
    public ItemData1[] itemPerCell;       
    public GameObject cellIconPrefab;     
    public int slotIndex;


    private Camera cam;
     public void Initialize(BlockShapeData shape, ItemDatabase1 itemDb, int slot)
    {
        shapeData = shape;
        slotIndex = slot;
        cam = Camera.main;

        
        itemPerCell = new ItemData1[shape.CellCount];
        ItemData1 randomItem = itemDb.GetRandomItem();
        for (int i = 0; i < shape.CellCount; i++)
        {
            itemPerCell[i] = randomItem;
        }
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
        float cs = (GridManager1.Instance != null) ? GridManager1.Instance.cellSize : 1f;

        if (shapeData != null && shapeData.dishSprite != null)
        {
            GameObject dishObj = new GameObject("DishBackground");
            dishObj.transform.SetParent(transform, false);
            Vector2 center = shapeData.GetCenterOffset();
            dishObj.transform.localPosition = new Vector3(center.x * cs*5f, center.y * cs*5f, 0f) + shapeData.dishOffset;

            SpriteRenderer dishSr = dishObj.AddComponent<SpriteRenderer>();
            dishSr.sprite = shapeData.dishSprite;
            dishSr.sortingOrder = 5;
        }

        for (int i = 0; i < shapeData.cells.Length; i++)
        {
            Vector2Int offset = shapeData.cells[i];
            GameObject icon = Instantiate(cellIconPrefab, transform);
            icon.transform.localPosition = new Vector3(offset.x * cs, offset.y * cs, 0f);
            SpriteRenderer[] srs = icon.GetComponentsInChildren<SpriteRenderer>();
            for (int j = 0; j < srs.Length; j++)
            {
                srs[j].sortingOrder += 20; 
            }
            if (srs.Length > 0 && itemPerCell != null && i < itemPerCell.Length && itemPerCell[i] != null)
            {
                SpriteRenderer targetSr = srs.Length > 1 ? srs[srs.Length - 1] : srs[0];
                if (targetSr != null && itemPerCell[i].icon != null)
                {
                    targetSr.sprite = itemPerCell[i].icon;
                }
            }
        }
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = -cam.transform.position.z;
        return cam.ScreenToWorldPoint(mouseScreen);
    }

    public bool TryPlace()
    {
        Vector2Int originCell = GridManager1.Instance.WorldToCell(transform.position);

        if (GridManager1.Instance.CanPlace(shapeData, originCell))
        {
            GridManager1.Instance.PlaceBlock(shapeData, originCell, itemPerCell, cellIconPrefab);
            if (BlockSpawner1.Instance != null&&slotIndex>=0) BlockSpawner1.Instance.OnPiecePlaced(slotIndex);
            Destroy(gameObject);
            return true;
        }
        return false;
    }
}