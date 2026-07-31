using UnityEngine;
public class BlockPiece : MonoBehaviour
{
    public BlockShapeData shapeData;
    public ItemData[] itemPerCell;       
    public GameObject cellIconPrefab;     
    public int slotIndex;


    private Camera cam;
     public void Initialize(BlockShapeData shape, ItemDatabase itemDb, int slot)
    {
        shapeData = shape;
        slotIndex = slot;
        cam = Camera.main;

        itemPerCell = new ItemData[shape.CellCount];
        for (int i = 0; i < shape.CellCount; i++)
            itemPerCell[i] = itemDb.GetRandomItem();  
        BuildVisual();
    }
    
    public void InitializeCustom(BlockShapeData shape, ItemData[] items, int slot)
    {
        shapeData = shape;
        slotIndex = slot;
        itemPerCell = items;

        BuildVisual();
    }

    void BuildVisual()
    {
        for (int i = 0; i < shapeData.cells.Length; i++)
        {
            Vector2Int offset = shapeData.cells[i];
            GameObject icon = Instantiate(cellIconPrefab, transform);
            float cs = (GridManager.Instance != null) ? GridManager.Instance.cellSize : 1f;
            icon.transform.localPosition = new Vector3(offset.x * cs, offset.y * cs, 0f);
            SpriteRenderer[] srs = icon.GetComponentsInChildren<SpriteRenderer>();
            for (int j = 0; j < srs.Length; j++)
            {
                srs[j].sortingOrder += 20; 
            }
            var sr = icon.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.sprite = itemPerCell[i].icon;
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
        Vector2Int originCell = GridManager.Instance.WorldToCell(transform.position);

        if (GridManager.Instance.CanPlace(shapeData, originCell))
        {
            GridManager.Instance.PlaceBlock(shapeData, originCell, itemPerCell, cellIconPrefab);
            if (BlockSpawner.Instance != null&&slotIndex>=0) BlockSpawner.Instance.OnPiecePlaced(slotIndex);
            Destroy(gameObject);
            return true;
        }
        return false;
    }
}