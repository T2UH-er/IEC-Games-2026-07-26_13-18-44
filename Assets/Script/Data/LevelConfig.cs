using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "LevelConfig/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    public int levelNumber;
    public BlockConfig blockConfig;
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

    public int slotIndex = 0;
}
