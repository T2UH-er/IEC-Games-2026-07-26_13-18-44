using System.Collections.Generic;
using UnityEngine;
public class GridManager1 : MonoBehaviour
{
    // Dùng Dictionary để lưu số lượng từng loại vị hiện có trên toàn bàn chơi
    public Dictionary<string, int> totalFlavorCounts = new Dictionary<string, int>()
    {
        { "sour", 0 },
        { "spicy", 0 },
        { "salty", 0 },
        { "sweet", 0 },
        { "bitter", 0 },
        { "umami", 0 },
        { "buttery", 0 }
    };

    public static GridManager1 Instance {get;private set;}
    public int width = 4;
    public int height = 4;
    public float cellSize = 1f;
    public float placedIconScale = 1f;
    public Transform gridOrigin;          

    public GameObject cellSlotPrefab;    

    private ItemData1[,] cellItems;       
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
        cellItems = new ItemData1[width, height];
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
    public void PlaceBlock(BlockShapeData shape, Vector2Int originCell, ItemData1[] itemPerCell, GameObject iconPrefab)
    {
        // Cộng bộ vị của món ăn 1 lần duy nhất cho toàn bộ block
        if (itemPerCell != null && itemPerCell.Length > 0 && itemPerCell[0] != null && itemPerCell[0].flavorCounts != null)
        {
            foreach (var flavor in itemPerCell[0].flavorCounts)
            {
                if (totalFlavorCounts.ContainsKey(flavor.flavorName))
                {
                    totalFlavorCounts[flavor.flavorName] += flavor.count;
                }
                else
                {
                    totalFlavorCounts[flavor.flavorName] = flavor.count;
                }
            }
        }

        Debug.Log("Total Flavor Counts: ");
        Debug.Log(
                totalFlavorCounts["sour"].ToString() + " sours, " +
                totalFlavorCounts["spicy"].ToString() + " spicies, " +
                totalFlavorCounts["salty"].ToString() + " salties, " +
                totalFlavorCounts["sweet"].ToString() + " sweets, " +
                totalFlavorCounts["bitter"].ToString() + " bitters, " +
                totalFlavorCounts["umami"].ToString() + " umamis, " +
                totalFlavorCounts["buttery"].ToString() + " butteries."
            );

        for (int i = 0; i < shape.cells.Length; i++)
        {
            Vector2Int cell = originCell + shape.cells[i];
            if (!IsInsideGrid(cell)) continue;
            cellItems[cell.x, cell.y] = itemPerCell[i];

            GameObject icon = null;
            if (iconPrefab != null)
            {
                icon = Instantiate(iconPrefab, CellToWorld(cell.x, cell.y), Quaternion.identity, transform);
                icon.transform.localScale = Vector3.one;

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
                        if (targetSr.sprite != null && targetSr.sprite.bounds.size.x > 0)
                        {
                            float spriteSize = Mathf.Max(targetSr.sprite.bounds.size.x, targetSr.sprite.bounds.size.y);
                            float fitScale = cellSize / spriteSize;
                            targetSr.transform.localScale = new Vector3(fitScale, fitScale, 1f);
                        }
                    }
                }

                PlacedBlockInfo1 info = icon.AddComponent<PlacedBlockInfo1>();
                info.shapeData = shape;
                info.originCell = originCell;
                info.itemPerCell = itemPerCell;
            }

            cellVisuals[cell.x, cell.y] = icon;
        }
        if (ScoringSystem1.Instance != null)
        {
            ScoringSystem1.Instance.availableMoves--;
        }
    }

    public void RemovePlacedBlock(BlockShapeData shape, Vector2Int originCell)
    {
        // Trừ bộ vị của món ăn 1 lần duy nhất cho toàn bộ block
        ItemData1 firstItem = null;
        for (int i = 0; i < shape.cells.Length; i++)
        {
            Vector2Int cell = originCell + shape.cells[i];
            if (cellItems[cell.x, cell.y] != null)
            {
                firstItem = cellItems[cell.x, cell.y];
                break;
            }
        }

        if (firstItem != null && firstItem.flavorCounts != null)
        {
            foreach (var flavor in firstItem.flavorCounts)
            {
                if (totalFlavorCounts.ContainsKey(flavor.flavorName))
                {
                    totalFlavorCounts[flavor.flavorName] -= flavor.count;
                    if (totalFlavorCounts[flavor.flavorName] < 0)
                    {
                        totalFlavorCounts[flavor.flavorName] = 0; // Ensure it doesn't go negative
                    }
                }
            }
        }

        for (int i = 0; i < shape.cells.Length; i++)
        {
            Vector2Int cell = originCell + shape.cells[i];
            ClearCell(cell.x, cell.y);
        }
    }

    public ItemData1 GetItemAt(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return null;
        return cellItems[x, y];
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
        if (x < 0 || x >= width || y < 0 || y >= height) return;
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