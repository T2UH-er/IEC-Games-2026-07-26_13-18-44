using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "BlockGame/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public ItemData[] items;   // độ dài mảng chính là k

    public ItemData GetRandomItem()
    {
        return items[Random.Range(0, items.Length)];
    }
    public ItemData GetItemInPosition(int x)
    {
        if(x>=items.Length)
        {
            return null;
        }
        else
        {
            return items[x];
        }
    } 
}