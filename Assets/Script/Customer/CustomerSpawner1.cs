using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CustomerSpawner1 — Tự động sinh danh sách Khách hàng từ LevelConfig và dàn đều trên 1 hàng ngang.
/// Hàng khách hàng lấy tọa độ Y từ GameObject Anchor (customerRowAnchor).
/// </summary>
public class CustomerSpawner1 : MonoBehaviour
{
    public static CustomerSpawner1 Instance { get; private set; }

    [Header("Level Settings")]
    public int levelNumber = 0;

    [Header("Prefabs & Anchors")]
    [Tooltip("Prefab của Khách Hàng (có component Customer1)")]
    public GameObject customerPrefab;

    [Tooltip("Kéo GameObject vào đây để xác định tọa độ Y của hàng khách hàng")]
    public Transform customerRowAnchor;

    [Tooltip("Container chứa các Khách hàng sinh ra (để null sẽ lấy transform này)")]
    public Transform customerContainer;

    [Header("Layout Multi-Row (Grid 2x2)")]
    [Tooltip("Khoảng cách theo trục dọc giữa các hàng khi có từ 4 khách trở lên")]
    public float verticalRowSpacing = 2.4f;

    [Tooltip("Khoảng cách theo trục ngang giữa các cột khi ở dạng Grid 2x2. Nếu > 0 sẽ áp dụng khoảng cách này; nếu = 0 sẽ tự động chia theo độ rộng bàn cờ.")]
    public float horizontalColumnSpacing = 4.2f;

    [Header("Cấu Hình Level (Chứa Khách Hàng)")]
    public LevelConfig[] levelConfigs;

    public List<Customer1> currentCustomers = new List<Customer1>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (customerContainer == null) customerContainer = this.transform;
    }

    private void Start()
    {
        // 1. Tự động đồng bộ levelConfigs và levelNumber từ BlockSpawner1 nếu chưa gán
        if (BlockSpawner1.Instance != null)
        {
            if (levelConfigs == null || levelConfigs.Length == 0)
            {
                levelConfigs = BlockSpawner1.Instance.levelConfigs;
            }
            levelNumber = BlockSpawner1.Instance.levelNumber;
        }

        SpawnCustomersForLevel(levelNumber);
    }

    /// <summary>
    /// Sinh và dàn đều các khách hàng theo cấu hình LevelConfig.
    /// Nếu có <= 3 khách: Dàn trên 1 hàng ngang.
    /// Nếu có 4 khách trở lên: Tự động chia thành lưới 2x2 (2 hàng x 2 cột).
    /// </summary>
    public void SpawnCustomersForLevel(int targetLevel)
    {
        ClearCurrentCustomers();

        if (customerPrefab == null)
        {
            Debug.LogWarning("[CustomerSpawner1] customerPrefab chưa được gán trong Inspector!");
            return;
        }

        if (levelConfigs == null || levelConfigs.Length == 0)
        {
            levelConfigs = Resources.FindObjectsOfTypeAll<LevelConfig>();
        }

        if (levelConfigs == null || levelConfigs.Length == 0)
        {
            Debug.LogWarning("[CustomerSpawner1] levelConfigs chưa có dữ liệu!");
            return;
        }

        // Tìm tất cả customer config của level tương ứng
        List<CustomerConfig> configs = new List<CustomerConfig>();
        foreach (var lc in levelConfigs)
        {
            if (lc != null && lc.levelNumber == targetLevel && lc.customers != null)
            {
                configs.AddRange(lc.customers);
            }
        }

        if (configs.Count == 0)
        {
            Debug.LogWarning($"[CustomerSpawner1] Không tìm thấy khách hàng nào được cấu hình trong LevelConfig của Level {targetLevel}");
            return;
        }

        int customerCount = configs.Count;

        // 2. Tính toán phạm vi chiều ngang của hàng khách hàng theo Grid
        GridManager1 grid = GridManager1.Instance;
        float totalWidth = 6f;
        float startX = 0f;
        float centerX = 0f;

        if (grid != null)
        {
            Vector3 leftCellPos = grid.CellToWorld(0, 0);
            Vector3 rightCellPos = grid.CellToWorld(grid.width - 1, 0);

            float minX = leftCellPos.x - (grid.cellSize / 2f);
            float maxX = rightCellPos.x + (grid.cellSize / 2f);

            totalWidth = maxX - minX;
            startX = minX;
            centerX = (leftCellPos.x + rightCellPos.x) * 0.5f;
        }
        else if (customerRowAnchor != null)
        {
            startX = customerRowAnchor.position.x - (totalWidth / 2f);
            centerX = customerRowAnchor.position.x;
        }

        // 3. Xác định tọa độ Y từ customerRowAnchor
        float posY = 0f;
        if (customerRowAnchor != null)
        {
            posY = customerRowAnchor.position.y;
        }
        else if (grid != null)
        {
            Vector3 topCellPos = grid.CellToWorld(0, grid.height - 1);
            posY = topCellPos.y + grid.cellSize + 1.5f;
        }

        // 4. Xác định cấu trúc hàng và cột (Grid Layout)
        int cols;
        int rows;
        if (customerCount <= 3)
        {
            cols = customerCount;
            rows = 1;
        }
        else if (customerCount == 4)
        {
            // 4 khách: Hiển thị dạng Grid 2x2 (Khách 1,2 ở hàng trên; Khách 3,4 ở hàng dưới)
            cols = 2;
            rows = 2;
        }
        else
        {
            // Từ 5 khách trở lên: Tự động chia thành 2 hàng
            cols = Mathf.CeilToInt(customerCount / 2f);
            rows = 2;
        }

        float stepWidth = totalWidth / cols;

        for (int i = 0; i < customerCount; i++)
        {
            int rowIndex = i / cols; // 0 = Hàng trên, 1 = Hàng dưới
            int colIndex = i % cols; // 0 = Cột trái, 1 = Cột phải

            int itemsInThisRow = (rowIndex == rows - 1) ? (customerCount - rowIndex * cols) : cols;

            // Tính tọa độ X: Dùng horizontalColumnSpacing nếu được cấu hình > 0, ngược lại chia đều theo bàn cờ
            float posX;
            if (rows > 1 && horizontalColumnSpacing > 0f)
            {
                posX = centerX + (colIndex - (itemsInThisRow - 1) * 0.5f) * horizontalColumnSpacing;
            }
            else
            {
                float rowStartX = startX;
                if (itemsInThisRow < cols)
                {
                    rowStartX = startX + (cols - itemsInThisRow) * 0.5f * stepWidth;
                }
                posX = rowStartX + (colIndex + 0.5f) * stepWidth;
            }

            // Tọa độ Y: Hàng 0 ở trên, Hàng 1 ở dưới
            float currentPosY = posY;
            if (rows > 1)
            {
                currentPosY = posY + ((rows - 1) * 0.5f - rowIndex) * verticalRowSpacing;
            }

            Vector3 spawnPos = new Vector3(posX, currentPosY, 0f);

            GameObject customerObj = Instantiate(customerPrefab, spawnPos, Quaternion.identity, customerContainer);
            Customer1 customer = customerObj.GetComponent<Customer1>();

            if (customer != null)
            {
                customer.Initialize(configs[i]);
                currentCustomers.Add(customer);
            }
            else
            {
                Debug.LogError("[CustomerSpawner1] Prefab không có component Customer1!");
            }
        }

        // 5. Cập nhật danh sách cho ScoringSystem1
        if (ScoringSystem1.Instance != null)
        {
            ScoringSystem1.Instance.InitializeCustomers(currentCustomers);
        }
    }

    public void ClearCurrentCustomers()
    {
        foreach (var c in currentCustomers)
        {
            if (c != null) Destroy(c.gameObject);
        }
        currentCustomers.Clear();

        if (customerContainer != null)
        {
            for (int i = customerContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(customerContainer.GetChild(i).gameObject);
            }
        }

        Customer1[] allCust = FindObjectsOfType<Customer1>();
        foreach (var c in allCust)
        {
            if (c != null) Destroy(c.gameObject);
        }
    }
}
