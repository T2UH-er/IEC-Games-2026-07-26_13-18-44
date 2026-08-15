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

    [Header("Cấu Hình Level (Chứa Khách Hàng)")]
    public LevelConfig[] levelConfigs;

    private List<Customer1> currentCustomers = new List<Customer1>();

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

        if (grid != null)
        {
            Vector3 leftCellPos = grid.CellToWorld(0, 0);
            Vector3 rightCellPos = grid.CellToWorld(grid.width - 1, 0);

            float minX = leftCellPos.x - (grid.cellSize / 2f);
            float maxX = rightCellPos.x + (grid.cellSize / 2f);

            totalWidth = maxX - minX;
            startX = minX;
        }
        else if (customerRowAnchor != null)
        {
            startX = customerRowAnchor.position.x - (totalWidth / 2f);
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

        // 4. Dàn đều N khách hàng trên hàng ngang: mỗi khách chiếm 1/N chiều rộng
        float stepWidth = totalWidth / customerCount;

        for (int i = 0; i < customerCount; i++)
        {
            float posX = startX + (i + 0.5f) * stepWidth;
            Vector3 spawnPos = new Vector3(posX, posY, 0f);

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
