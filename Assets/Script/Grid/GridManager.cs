using UnityEngine;
public class GridManager : MonoBehaviour
{
    public static GridManager Instance {get;private set;}
    public int width = 8;
    public int height = 8;
    public float cellSize = 1f;
    public Transform gridOrigin;          

    public GameObject cellSlotPrefab;    

    private ItemData[,] cellItems;       
    private GameObject[,] cellVisuals; 
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); 
        }
        cellItems = new ItemData[width, height];
        cellVisuals = new GameObject[width, height];
    }
    void Start()
    {
        BuildVisualGrid();
    }
    void BuildVisualGrid()
    {
        if (cellSlotPrefab == null) return;
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++) {
                Instantiate(cellSlotPrefab, CellToWorld(x, y), Quaternion.identity, transform);
            }
    }
    public Vector3 CellToWorld(int x,int y)
    {
        Vector3 origin;
        if(gridOrigin != null){
            origin = gridOrigin.position;
        }
        else
        {
            origin = Vector3.zero;
        }
        return origin + new Vector3(x * cellSize, y * cellSize, 0f);
    }
    public Vector2Int WorldToCell(Vector3 worldPos)
    {
        Vector3 origin;
        if(gridOrigin != null){
            origin = gridOrigin.position;
        }
        else
        {
            origin = Vector3.zero;
        }
        Vector3 local = worldPos - origin;
        int x = Mathf.RoundToInt(local.x / cellSize);
        int y = Mathf.RoundToInt(local.y / cellSize);
        return new Vector2Int(x, y);
    }
    public bool IsInsideGrid(Vector2Int cell)
    {
        return cell.x >= 0 &&
            cell.x < width &&
            cell.y >= 0 &&
            cell.y < height;
    }
    public bool IsCellEmpty (Vector2Int cell)
    {
        return IsInsideGrid(cell)&&cellItems[cell.x,cell.y]==null;
    }
    public bool CanPlace(BlockShapeData shape, Vector2Int originCell)
    {
        foreach (var offset in shape.cells)
            if (!IsCellEmpty(originCell + offset)) return false;
        return true;
    }
    public void PlaceBlock(BlockShapeData shape, Vector2Int originCell, ItemData[] itemPerCell, GameObject iconPrefab)
    {
        for (int i = 0; i < shape.cells.Length; i++)
        {
            Vector2Int cell = originCell + shape.cells[i];
            cellItems[cell.x, cell.y] = itemPerCell[i];

            GameObject icon = Instantiate(iconPrefab, CellToWorld(cell.x, cell.y), Quaternion.identity, transform);

            SpriteRenderer[] srs = icon.GetComponentsInChildren<SpriteRenderer>();
            for (int j = 0; j < srs.Length; j++)
            {
                srs[j].sortingOrder += 10;
            }

            if (srs.Length > 0 && itemPerCell != null && i < itemPerCell.Length && itemPerCell[i] != null)
            {
                SpriteRenderer targetSr = srs.Length > 1 ? srs[srs.Length - 1] : srs[0];
                if (targetSr != null && itemPerCell[i].icon != null)
                {
                    targetSr.sprite = itemPerCell[i].icon;
                }
            }

            cellVisuals[cell.x, cell.y] = icon;
            PlacedBlockInfo info = icon.AddComponent<PlacedBlockInfo>();
            info.shapeData = shape;
            info.originCell = originCell;
            info.itemPerCell = itemPerCell;
        }
    }
    public void RemovePlacedBlock(BlockShapeData shape, Vector2Int originCell)
    {
        for (int i = 0; i < shape.cells.Length; i++)
        {
            Vector2Int cell = originCell + shape.cells[i];
            ClearCell(cell.x, cell.y);
        }
    }

    public ItemData GetItemAt(int x, int y)
    {
        return cellItems[x,y];
    }
    //logic for destroy block can be remove if not be used
    public bool IsRowFull(int y)
    {
        for (int x = 0; x < width; x++)
            if (cellItems[x, y] == null) return false;
        return true;
    }
    public bool IsColFull(int x)
    {
        for (int y = 0; y < height; y++)
            if (cellItems[x, y] == null) return false;
        return true;
    }
    public void ClearRow(int y) 
    { 
        for (int x = 0; x < width; x++) 
        {
            ClearCell(x, y); 
        }
    }
    public void ClearCol(int x) { 
        for (int y = 0; y < height; y++) 
        {
            ClearCell(x, y); 
        }
    }
    void ClearCell(int x, int y)
    {
        if (cellVisuals[x, y] != null) Destroy(cellVisuals[x, y]);
        cellVisuals[x, y] = null;
        cellItems[x, y] = null;
    }
    public bool HasAnyValidMove(BlockShapeData[] currentShapes)
    {
        foreach (var shape in currentShapes)
        {
            if (shape == null) continue;
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (CanPlace(shape, new Vector2Int(x, y))) return true;
        }
        return false;
    }
}