using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// 상점 UI - 구매/판매 모드 토글, 슬롯 좌클릭(1개)/우클릭(5개)
public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance { get; private set; }

    [Header("패널")]
    public GameObject shopPanel;
    public TMP_Text txtShopName;
    public Button btnClose;

    [Header("모드 전환")]
    public Button btnBuy;
    public Button btnSell;

    [Header("슬롯")]
    public Transform slotContent;
    public GameObject slotPrefab;

    [Header("골드")]
    public TMP_Text txtGold;

    [Header("알림")]
    public TMP_Text txtMessage;
    public float messageDuration = 2f;

    [Header("디버그 (패널 수동 활성화 시)")]
    [Tooltip("테스트용 - 패널만 켰을 때 이 ShopData로 목록 표시")]
    public ShopData debugShopData;

    private bool isBuyMode = true;
    private bool hasSubscribed;
    private List<GameObject> slots = new List<GameObject>();
    private float messageHideTime;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        if (btnClose != null)
        {
            btnClose.onClick.RemoveAllListeners();
            btnClose.onClick.AddListener(OnCloseClick);
        }
        if (btnBuy != null) btnBuy.onClick.AddListener(() => SetMode(true));
        if (btnSell != null) btnSell.onClick.AddListener(() => SetMode(false));

        if (GoldManager.Instance != null)
            GoldManager.Instance.OnGoldChanged += RefreshGold;

        TrySubscribeToShopManager();
    }

    void TrySubscribeToShopManager()
    {
        if (hasSubscribed) return;
        if (ShopManager.Instance == null) return;
        ShopManager.Instance.OnShopOpened += OnShopOpened;
        ShopManager.Instance.OnShopClosed += OnShopClosed;
        hasSubscribed = true;
    }

    void OnEnable()
    {
        TrySubscribeToShopManager();
        if (shopPanel == null || !shopPanel.activeSelf) return;
        var shop = ShopManager.Instance?.CurrentShop ?? debugShopData;
        if (shop != null)
        {
            if (ShopManager.Instance != null && !ShopManager.Instance.IsShopOpen)
                ShopManager.Instance.OpenShop(shop);
            if (txtShopName != null) txtShopName.text = shop.shopName;
            RefreshGold();
            RefreshSlotsWithShop(shop);
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (GoldManager.Instance != null)
            GoldManager.Instance.OnGoldChanged -= RefreshGold;
        if (hasSubscribed && ShopManager.Instance != null)
        {
            ShopManager.Instance.OnShopOpened -= OnShopOpened;
            ShopManager.Instance.OnShopClosed -= OnShopClosed;
        }
    }

    void Update()
    {
        if (txtMessage != null && txtMessage.gameObject.activeSelf && Time.unscaledTime >= messageHideTime)
            txtMessage.gameObject.SetActive(false);
    }

    void OnShopOpened(ShopData shop)
    {
        ShowShopPanel(shop);
    }

    public void ShowShopPanel(ShopData shop)
    {
        if (shop == null) return;
        if (shopPanel != null) shopPanel.SetActive(true);
        if (txtShopName != null) txtShopName.text = shop.shopName;
        GameInputBlocker.Block();
        isBuyMode = true;
        RefreshGold();
        RefreshSlots();
    }

    void RefreshSlotsWithShop(ShopData shop)
    {
        ClearSlots();
        if (slotContent == null || slotPrefab == null) return;

        if (isBuyMode)
        {
            if (shop == null) return;
            foreach (var entry in shop.sellItems)
            {
                if (entry.item == null) continue;
                CreateSlot(entry.item, true);
            }
        }
        else
        {
            var inv = InventoryManager.Instance ?? FindObjectOfType<InventoryManager>();
            if (inv == null) return;
            var seen = new HashSet<ItemData>();
            for (int i = 0; i < InventoryManager.TOTAL_SLOT_COUNT; i++)
            {
                var slot = inv.GetSlotData(i);
                if (slot == null || slot.IsEmpty) continue;
                if (slot.item.sellPrice <= 0) continue;
                if (seen.Contains(slot.item)) continue;
                seen.Add(slot.item);
                CreateSlot(slot.item, false);
            }
        }

        if (slotContent is RectTransform rt)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    void OnShopClosed()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        GameInputBlocker.Unblock();
    }

    public void OnCloseClick()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
        if (ShopManager.Instance != null && ShopManager.Instance.IsShopOpen)
            ShopManager.Instance.CloseShop();
    }

    void SetMode(bool buy)
    {
        isBuyMode = buy;
        RefreshSlots();
    }

    void RefreshGold()
    {
        if (txtGold != null && GoldManager.Instance != null)
            txtGold.text = GoldManager.Instance.CurrentGold.ToString("N0");
    }

    void RefreshSlots()
    {
        var shop = ShopManager.Instance?.CurrentShop ?? debugShopData;
        RefreshSlotsWithShop(shop);
    }

    void CreateSlot(ItemData item, bool buyMode)
    {
        var go = Instantiate(slotPrefab, slotContent);
        slots.Add(go);

        EnsureSlotCanReceiveEvents(go);

        var slotUI = go.GetComponent<ShopSlotUI>();
        if (slotUI == null) slotUI = go.AddComponent<ShopSlotUI>();
        slotUI.Setup(item, buyMode, this);
    }

    void EnsureSlotCanReceiveEvents(GameObject slotRoot)
    {
        var rootImage = slotRoot.GetComponent<Image>();
        if (rootImage == null)
        {
            rootImage = slotRoot.AddComponent<Image>();
            rootImage.color = Color.clear;
        }
        rootImage.raycastTarget = true;
    }

    void ClearSlots()
    {
        foreach (var go in slots)
        {
            if (go != null) Destroy(go);
        }
        slots.Clear();
    }

    public void TryBuy(ItemData item, int count)
    {
        if (ShopManager.Instance == null) return;
        if (ShopManager.Instance.BuyItem(item, count, out string err))
        {
            ShowMessage($"{item.itemName} x{count} 구매 완료");
            RefreshSlots();
            RefreshGold();
        }
        else
        {
            ShowMessage(err ?? "구매 실패");
        }
    }

    public void TrySell(ItemData item, int count)
    {
        if (ShopManager.Instance == null) return;
        var inv = InventoryManager.Instance ?? FindObjectOfType<InventoryManager>();
        int have = inv != null ? inv.GetItemCount(item) : 0;
        count = Mathf.Min(count, have);
        if (count <= 0)
        {
            ShowMessage("보유 수량이 부족합니다.");
            return;
        }
        if (ShopManager.Instance.SellItem(item, count, out string err))
        {
            int earn = item.sellPrice * count;
            ShowMessage($"{item.itemName} x{count} 판매 완료 (+{earn} G)");
            RefreshSlots();
            RefreshGold();
        }
        else
        {
            ShowMessage(err ?? "판매 실패");
        }
    }

    bool CanSellItem(ItemData item, ShopData shop)
    {
        if (item == null || item.sellPrice <= 0) return false;
        if (shop == null) return true;
        if (shop.sellableTypes == null || shop.sellableTypes.Length == 0) return true;
        foreach (var t in shop.sellableTypes)
            if (item.itemType == t) return true;
        return false;
    }

    void ShowMessage(string msg)
    {
        if (txtMessage != null)
        {
            txtMessage.text = msg;
            txtMessage.gameObject.SetActive(true);
            messageHideTime = Time.unscaledTime + messageDuration;
        }
    }
}
