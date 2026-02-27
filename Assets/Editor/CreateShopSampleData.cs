using UnityEngine;
using UnityEditor;

/// <summary>
/// 상점/아이템 샘플 데이터 생성
/// 메뉴: Tools → Create Shop Sample Data
/// </summary>
public static class CreateShopSampleData
{
    const string ItemPath = "Assets/Scripts/Data/Item";
    const string CropPath = "Assets/Scripts/Data/Crops";
    const string ShopPath = "Assets/Resources/Shop";

    [MenuItem("Tools/Create Shop Sample Data")]
    static void CreateAll()
    {
        CreateItems();
        CreateShop();
        UpdateItemDatabase();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CreateShopSampleData] 샘플 데이터 생성 완료. ItemDatabase에 새 아이템을 수동으로 추가해주세요.");
    }

    static void CreateItems()
    {
        // 토마토 (작물)
        CreateItemData("Tomato", "토마토", ItemType.Crop, "맛있는 토마토!", 60, 30);

        // 당근 (작물)
        CreateItemData("Carrot", "당근", ItemType.Crop, "싱싱한 당근!", 45, 22);

        // 나무 (재료)
        CreateItemData("Wood", "나무", ItemType.Material, "채집한 나무.", 20, 10);

        // 돌 (재료)
        CreateItemData("Stone", "돌", ItemType.Material, "채집한 돌.", 15, 7);

        // 빵 (소비품)
        CreateItemData("Bread", "빵", ItemType.Consumable, "맛있는 빵. 배고픔을 채워줍니다.", 100, 50);
    }

    static void CreateItemData(string name, string displayName, ItemType type, string desc, int buy, int sell)
    {
        string path = $"{ItemPath}/{name}.asset";
        if (AssetDatabase.LoadAssetAtPath<ItemData>(path) != null) return;

        var item = ScriptableObject.CreateInstance<ItemData>();
        item.itemName = displayName;
        item.itemType = type;
        item.description = desc;
        item.buyPrice = buy;
        item.sellPrice = sell;
        item.maxStackSize = 99;

        // 기존 스프라이트 재사용 (Strawberry 아이콘)
        var existing = AssetDatabase.LoadAssetAtPath<ItemData>($"{ItemPath}/Strawberry.asset");
        if (existing != null && existing.icon != null)
            item.icon = existing.icon;

        AssetDatabase.CreateAsset(item, path);
    }

    static void CreateShop()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Shop")) AssetDatabase.CreateFolder("Assets/Resources", "Shop");

        string path = $"{ShopPath}/Shop_General.asset";
        if (AssetDatabase.LoadAssetAtPath<ShopData>(path) != null) return;

        var shop = ScriptableObject.CreateInstance<ShopData>();
        shop.shopName = "일반 상점";

        var db = AssetDatabase.LoadAssetAtPath<ItemDatabase>("Assets/Resources/ItemDatabase.asset");
        if (db != null)
        {
            foreach (var item in db.allItems)
            {
                if (item == null) continue;
                if (item.buyPrice <= 0) continue;
                shop.sellItems.Add(new ShopItemEntry { item = item });
            }
        }

        // 수동으로 추가 (DB에 없을 수 있음)
        AddShopEntry(shop, "Assets/Scripts/Data/Item/Strawberry.asset");
        AddShopEntry(shop, "Assets/Scripts/Data/Crops/StrawberrySeed.asset");
        AddShopEntry(shop, "Assets/Scripts/Data/Item/hoe.asset");
        AddShopEntry(shop, "Assets/Scripts/Data/Item/wateringcan.asset");
        AddShopEntry(shop, "Assets/Scripts/Data/Item/Tomato.asset");
        AddShopEntry(shop, "Assets/Scripts/Data/Item/Carrot.asset");
        AddShopEntry(shop, "Assets/Scripts/Data/Item/Wood.asset");
        AddShopEntry(shop, "Assets/Scripts/Data/Item/Stone.asset");
        AddShopEntry(shop, "Assets/Scripts/Data/Item/Bread.asset");

        AssetDatabase.CreateAsset(shop, path);
    }

    static void AddShopEntry(ShopData shop, string assetPath)
    {
        var item = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);
        if (item == null) return;
        if (shop.sellItems.Exists(e => e.item == item)) return;
        if (item.buyPrice <= 0) return;
        shop.sellItems.Add(new ShopItemEntry { item = item });
    }

    static void UpdateItemDatabase()
    {
        var db = AssetDatabase.LoadAssetAtPath<ItemDatabase>("Assets/Resources/ItemDatabase.asset");
        if (db == null) return;

        var newItems = new[] {
            "Assets/Scripts/Data/Item/Tomato.asset",
            "Assets/Scripts/Data/Item/Carrot.asset",
            "Assets/Scripts/Data/Item/Wood.asset",
            "Assets/Scripts/Data/Item/Stone.asset",
            "Assets/Scripts/Data/Item/Bread.asset"
        };

        foreach (var p in newItems)
        {
            var item = AssetDatabase.LoadAssetAtPath<ItemData>(p);
            if (item != null && !db.allItems.Contains(item))
                db.allItems.Add(item);
        }
        EditorUtility.SetDirty(db);
    }
}
