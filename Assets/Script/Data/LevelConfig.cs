using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cấu hình dữ liệu cho 1 Khách hàng trong Level.
/// </summary>
[System.Serializable]
public class CustomerConfig
{
    [Header("Avatar / Icon Khách Hàng")]
    public Sprite customerSprite;

    [Header("Yêu Cầu Vị Món Ăn")]
    public List<FlavorData> requirement = new List<FlavorData>()
    {
        new FlavorData { flavorName = "sour", count = 0 },
        new FlavorData { flavorName = "spicy", count = 0 },
        new FlavorData { flavorName = "salty", count = 0 },
        new FlavorData { flavorName = "sweet", count = 0 },
        new FlavorData { flavorName = "bitter", count = 0 },
        new FlavorData { flavorName = "umami", count = 0 },
        new FlavorData { flavorName = "buttery", count = 0 }
    };

    [Header("Vùng Ảnh Hưởng (Tọa độ ô trên Grid)")]
    public List<Vector2Int> affectedZone = new List<Vector2Int>();

    [Header("Màu Vùng Ảnh Hưởng Của Khách Này")]
    public Color zoneColor = new Color(0.2f, 0.6f, 1.0f, 0.35f);
}

/// <summary>
/// Cấu hình dữ liệu cho 1 Khối món ăn sẽ sinh ra trên Khay (Tray).
/// </summary>
[System.Serializable]
public class LevelBlockSpawnConfig
{
    [Tooltip("Vị trí ô trên Khay (0, 1, 2, 3...)")]
    public int slotIndex = 0;

    [Tooltip("Cấu hình hình dạng khối (Shape) và loại món ăn")]
    public BlockConfig blockConfig;

    [Tooltip("Danh sách vị của khối món ăn này")]
    public List<FlavorData> flavorCounts = new List<FlavorData>()
    {
        new FlavorData { flavorName = "sour", count = 0 },
        new FlavorData { flavorName = "spicy", count = 0 },
        new FlavorData { flavorName = "salty", count = 0 },
        new FlavorData { flavorName = "sweet", count = 0 },
        new FlavorData { flavorName = "bitter", count = 0 },
        new FlavorData { flavorName = "umami", count = 0 },
        new FlavorData { flavorName = "buttery", count = 0 }
    };
}

/// <summary>
/// LevelConfig — ScriptableObject quản lý toàn bộ dữ liệu của 1 Level:
/// Gồm danh sách Khách Hàng (customers) và danh sách Khối Món Ăn trên khay (trayBlocks).
/// </summary>
[CreateAssetMenu(fileName = "LevelConfig", menuName = "LevelConfig/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    [Header("Thong Tin Level")]
    public int levelNumber = 0;

    [Header("Kich Thuoc Grid")]
    [Tooltip("So cot cua ban co (vd: 4 = 4x4, 5 = 5x5). De 0 se giu nguyen kich thuoc cu.")]
    public int gridWidth = 4;
    [Tooltip("So hang cua ban co (vd: 4 = 4x4, 5 = 5x5). De 0 se giu nguyen kich thuoc cu.")]
    public int gridHeight = 4;

    [Header("1. Danh Sách Khách Hàng Trong Level")]
    public List<CustomerConfig> customers = new List<CustomerConfig>();

    [Header("2. Danh Sách Khối Món Ăn Xuất Hiện Trên Khay")]
    public List<LevelBlockSpawnConfig> trayBlocks = new List<LevelBlockSpawnConfig>();

    [Header("3. Các Ô Bị Chặn (Chốt Chặn)")]
    [Tooltip("Danh sách tọa độ (x,y) của các ô bị chặn (màu đen). Ví dụ: x=1, y=2")]
    public List<Vector2Int> blockedCells = new List<Vector2Int>();

    // Giữ trường đơn lẻ để tương thích ngược nếu còn asset cũ
    [HideInInspector] public BlockConfig blockConfig;
    [HideInInspector] public List<FlavorData> flavorCounts;
    [HideInInspector] public int slotIndex = 0;
}
