using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "BlockGame/Item Database")]
public class ItemDatabase1 : ScriptableObject
{
    public ItemData1[] items;   // độ dài mảng chính là k

    public ItemData1 GetRandomItem()
    {
        return items[Random.Range(0, items.Length)];
    }
    public ItemData1 GetItemInPosition(int x)
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