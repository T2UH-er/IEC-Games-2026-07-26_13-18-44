using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class LevelSolver : MonoBehaviour
{
    [MenuItem("Tools/Solve Level 14")]
    public static void Solve()
    {
        string path = "Assets/Items/LevelConfigs/pa/Lv14.asset";
        LevelConfig level = AssetDatabase.LoadAssetAtPath<LevelConfig>(path);
        if (level == null)
        {
            Debug.LogError("Could not find Level 14 at " + path);
            return;
        }

        Debug.Log($"Solving Level 14... Grid: {level.gridWidth}x{level.gridHeight}");

        // Gather grid info
        List<Vector2Int> validCells = new List<Vector2Int>();
        for (int x = 0; x < level.gridWidth; x++)
        {
            for (int y = 0; y < level.gridHeight; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (!level.blockedCells.Contains(pos))
                {
                    validCells.Add(pos);
                }
            }
        }

        // Get blocks
        List<BlockConfig> blocks = new List<BlockConfig>();
        foreach (var tb in level.trayBlocks)
        {
            blocks.Add(tb.blockConfig);
        }

        // Backtracking to find placement
        List<Placement> placements = new List<Placement>();
        if (SolveRecursive(level, blocks, 0, new Dictionary<Vector2Int, CellInfo>(), validCells, placements))
        {
            Debug.Log("<color=green>SOLUTION FOUND!</color>");
            foreach (var p in placements)
            {
                Debug.Log($"Block '{p.block.name}' placed at {p.origin} (Rotated {p.rotation} times). Cells: {string.Join(", ", p.cells)}");
            }
        }
        else
        {
            Debug.LogError("No solution found.");
        }
    }

    struct Placement
    {
        public BlockConfig block;
        public Vector2Int origin;
        public int rotation;
        public List<Vector2Int> cells;
    }

    struct CellInfo
    {
        public BlockConfig block;
        public ItemData1 item;
    }

    static bool SolveRecursive(LevelConfig level, List<BlockConfig> blocks, int blockIndex, Dictionary<Vector2Int, CellInfo> grid, List<Vector2Int> validCells, List<Placement> outPlacements)
    {
        if (blockIndex >= blocks.Count)
        {
            // All blocks placed. Check customers.
            return CheckCustomers(level, grid);
        }

        BlockConfig currentBlock = blocks[blockIndex];

        // Try every valid cell as origin
        foreach (Vector2Int origin in validCells)
        {
            // Try all 4 rotations
            for (int r = 0; r < 4; r++)
            {
                List<Vector2Int> occupiedCells = new List<Vector2Int>();
                bool validPlacement = true;

                for (int i = 0; i < currentBlock.shapeData.cells.Length; i++)
                {
                    Vector2Int offset = currentBlock.shapeData.cells[i];
                    
                    // Apply rotation
                    int rotatedX = offset.x;
                    int rotatedY = offset.y;
                    for (int step = 0; step < r; step++)
                    {
                        int tempX = rotatedX;
                        rotatedX = rotatedY;
                        rotatedY = -tempX;
                    }
                    Vector2Int finalOffset = new Vector2Int(rotatedX, rotatedY);
                    Vector2Int targetPos = origin + finalOffset;

                    if (!validCells.Contains(targetPos) || grid.ContainsKey(targetPos))
                    {
                        validPlacement = false;
                        break;
                    }
                    occupiedCells.Add(targetPos);
                }

                if (validPlacement)
                {
                    // Place block
                    for (int i = 0; i < occupiedCells.Count; i++)
                    {
                        grid[occupiedCells[i]] = new CellInfo { block = currentBlock, item = currentBlock.itemPerCell[i] };
                    }
                    outPlacements.Add(new Placement { block = currentBlock, origin = origin, rotation = r, cells = occupiedCells });

                    // Recurse
                    if (SolveRecursive(level, blocks, blockIndex + 1, grid, validCells, outPlacements))
                    {
                        return true;
                    }

                    // Backtrack
                    outPlacements.RemoveAt(outPlacements.Count - 1);
                    foreach (var c in occupiedCells)
                    {
                        grid.Remove(c);
                    }
                }
            }
        }

        return false;
    }

    static bool CheckCustomers(LevelConfig level, Dictionary<Vector2Int, CellInfo> grid)
    {
        foreach (var customer in level.customers)
        {
            Dictionary<string, int> currentFlavors = new Dictionary<string, int>();
            HashSet<BlockConfig> processedBlocks = new HashSet<BlockConfig>();

            foreach (var zone in customer.affectedZone)
            {
                if (grid.TryGetValue(zone, out CellInfo cellInfo))
                {
                    if (cellInfo.block != null && !processedBlocks.Contains(cellInfo.block))
                    {
                        processedBlocks.Add(cellInfo.block);
                        var tb = level.trayBlocks.FirstOrDefault(b => b.blockConfig == cellInfo.block);
                        if (tb != null && tb.flavorCounts != null)
                        {
                            foreach (var fv in tb.flavorCounts)
                            {
                                if (!currentFlavors.ContainsKey(fv.flavorName))
                                    currentFlavors[fv.flavorName] = 0;
                                currentFlavors[fv.flavorName] += fv.count;
                            }
                        }
                    }
                }
            }

            // Check against requirement (>= requirement count)
            foreach (var req in customer.requirement)
            {
                int currentFlv = currentFlavors.ContainsKey(req.flavorName) ? currentFlavors[req.flavorName] : 0;
                if (currentFlv < req.count)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
