using UnityEngine;
using System.Collections.Generic;

// 상점에서 판매하는 아이템 1개
[System.Serializable]
public class ShopItemEntry
{
    [Tooltip("ItemDatabase에 등록된 아이템")]
    public ItemData item;
    [Tooltip("판매 가격 (0이면 item.buyPrice 사용)")]
    public int priceOverride;
    [Tooltip("재고 (-1 = 무제한)")]
    public int stock = -1;

    public int BuyPrice => priceOverride > 0 ? priceOverride : (item != null ? item.buyPrice : 0);
}

// 상점 데이터 (ScriptableObject)
// Project 창에서 우클릭 → Create → Shop → Shop Data
[CreateAssetMenu(fileName = "NewShop", menuName = "Shop/Shop Data")]
public class ShopData : ScriptableObject
{
    [Tooltip("상점 이름 (UI 표시용)")]
    public string shopName = "상점";

    [Tooltip("판매하는 아이템 목록")]
    public List<ShopItemEntry> sellItems = new List<ShopItemEntry>();

    [Tooltip("판매 가능한 아이템 타입 (비어있으면 모든 sellPrice > 0 아이템 판매 가능)")]
    public ItemType[] sellableTypes = new ItemType[] { ItemType.Tool, ItemType.Seed, ItemType.Crop, ItemType.Material, ItemType.Consumable, ItemType.Etc };
}
