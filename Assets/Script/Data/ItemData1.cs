using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct FlavorData
{
    public string flavorName;
    public int count;
}

[CreateAssetMenu(fileName = "NewItemData", menuName = "BlockGame/Item Data 1")]
public class ItemData1 : ScriptableObject
{
    public string itemId;
    public string displayName;
    public Sprite icon;

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

    // --- CÁC HÀM TIỆN ÍCH TRUY CẬP ---

    // 1. Lấy số lượng của một vị cụ thể
    public int GetFlavorCount(string flavorName)
    {
        // Dùng Find để tìm vị tương ứng
        FlavorData data = flavorCounts.Find(f => f.flavorName.Equals(flavorName, StringComparison.OrdinalIgnoreCase));
        return data.count; // Nếu không tìm thấy struct sẽ trả về 0 mặc định
    }

    // 2. Kiểm tra xem một vị có giá trị lớn hơn 0 không (hoặc thỏa điều kiện nào đó)
    public bool HasFlavor(string flavorName)
    {
        return GetFlavorCount(flavorName) > 0;
    }

    // 3. Cập nhật số lượng của một vị
    public void SetFlavorCount(string flavorName, int newCount)
    {
        for (int i = 0; i < flavorCounts.Count; i++)
        {
            if (flavorCounts[i].flavorName.Equals(flavorName, StringComparison.OrdinalIgnoreCase))
            {
                FlavorData updated = flavorCounts[i];
                updated.count = newCount;
                flavorCounts[i] = updated;
                return;
            }
        }
    }
}