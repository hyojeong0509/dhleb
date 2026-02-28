using UnityEngine;
using System;

/// <summary>
/// 상점 구매/판매 로직
/// </summary>
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    public bool IsShopOpen { get; private set; }
    public ShopData CurrentShop { get; private set; }

    public event Action<ShopData> OnShopOpened;
    public event Action OnShopClosed;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// 상점 열기
    /// </summary>
    public void OpenShop(ShopData shopData)
    {
        if (shopData == null) return;
        if (IsShopOpen) CloseShop();

        CurrentShop = shopData;
        IsShopOpen = true;
        OnShopOpened?.Invoke(shopData);

        var ui = ShopUI.Instance ?? UnityEngine.Object.FindObjectOfType<ShopUI>();
        if (ui != null)
            ui.ShowShopPanel(shopData);
    }

    /// <summary>
    /// 상점 닫기
    /// </summary>
    public void CloseShop()
    {
        if (!IsShopOpen) return;
        CurrentShop = null;
        IsShopOpen = false;
        OnShopClosed?.Invoke();
    }

    /// <summary>
    /// 아이템 구매
    /// </summary>
    /// <returns>구매 성공 여부</returns>
    public bool BuyItem(ItemData item, int count, out string errorMsg)
    {
        errorMsg = null;
        if (item == null || count <= 0 || CurrentShop == null)
        {
            errorMsg = "구매할 수 없습니다.";
            return false;
        }

        var entry = CurrentShop.sellItems.Find(e => e.item == item);
        if (entry == null)
        {
            errorMsg = "이 상점에서 판매하지 않는 아이템입니다.";
            return false;
        }

        int price = entry.BuyPrice;
        if (price <= 0)
        {
            errorMsg = "구매 불가 아이템입니다.";
            return false;
        }

        int totalCost = price * count;
        if (GoldManager.Instance == null || !GoldManager.Instance.CanAfford(totalCost))
        {
            errorMsg = "골드가 부족합니다.";
            return false;
        }

        if (InventoryManager.Instance == null)
        {
            errorMsg = "인벤토리를 사용할 수 없습니다.";
            return false;
        }

        if (!InventoryManager.Instance.AddItem(item, count, playAcquisitionSound: true))
        {
            errorMsg = "인벤토리가 가득 찼습니다.";
            return false;
        }

        GoldManager.Instance.SpendGold(totalCost);
        return true;
    }

    /// <summary>
    /// 아이템 판매 (작물 등)
    /// </summary>
    /// <returns>판매 성공 여부</returns>
    public bool SellItem(ItemData item, int count, out string errorMsg)
    {
        errorMsg = null;
        if (item == null || count <= 0 || CurrentShop == null)
        {
            errorMsg = "판매할 수 없습니다.";
            return false;
        }

        if (item.sellPrice <= 0)
        {
            errorMsg = "판매 불가 아이템입니다.";
            return false;
        }

        bool typeOk = CurrentShop.sellableTypes == null || CurrentShop.sellableTypes.Length == 0;
        if (!typeOk)
        {
            foreach (var t in CurrentShop.sellableTypes)
            {
                if (item.itemType == t) { typeOk = true; break; }
            }
        }
        if (!typeOk)
        {
            errorMsg = "이 상점에서 받지 않는 아이템입니다.";
            return false;
        }

        var inv = InventoryManager.Instance ?? UnityEngine.Object.FindObjectOfType<InventoryManager>();
        if (inv == null)
        {
            errorMsg = "인벤토리를 사용할 수 없습니다.";
            return false;
        }

        int have = inv.GetItemCount(item);
        if (have < count)
        {
            errorMsg = "보유 수량이 부족합니다.";
            return false;
        }

        int removed = inv.RemoveItem(item, count);
        if (removed <= 0)
        {
            errorMsg = "판매에 실패했습니다.";
            return false;
        }

        int earn = item.sellPrice * removed;
        var gold = GoldManager.Instance ?? UnityEngine.Object.FindObjectOfType<GoldManager>();
        if (gold != null) gold.AddGold(earn);
        return true;
    }

    /// <summary>
    /// 인벤토리에서 해당 아이템 판매 가능 여부
    /// </summary>
    public bool CanSell(ItemData item)
    {
        if (item == null || CurrentShop == null || item.sellPrice <= 0) return false;
        if (CurrentShop.sellableTypes == null || CurrentShop.sellableTypes.Length == 0) return true;
        foreach (var t in CurrentShop.sellableTypes)
            if (item.itemType == t) return true;
        return false;
    }
}
