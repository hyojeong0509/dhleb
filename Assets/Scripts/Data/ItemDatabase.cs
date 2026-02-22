using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 모든 아이템 에셋을 등록해두고, 저장/로드 시 이름으로 찾기
/// Project 창에서 우클릭 → Create → Data → Item Database
/// Inspector에서 사용하는 모든 ItemData/SeedData/ToolData를 리스트에 드래그
/// </summary>
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Data/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [Header("등록할 모든 아이템 (ItemData, SeedData, ToolData)")]
    public List<ItemData> allItems = new List<ItemData>();

    private static ItemDatabase _instance;
    public static ItemDatabase Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<ItemDatabase>("ItemDatabase");
            return _instance;
        }
    }

    /// <summary>
    /// 아이템 이름으로 ItemData 찾기 (로드 시 사용)
    /// </summary>
    public ItemData GetItemByName(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return null;

        foreach (var item in allItems)
        {
            if (item != null && item.itemName == itemName)
                return item;
        }

        Debug.LogWarning($"[ItemDatabase] '{itemName}' 아이템을 찾을 수 없습니다.");
        return null;
    }
}
