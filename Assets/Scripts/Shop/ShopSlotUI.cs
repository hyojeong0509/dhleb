using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 상점 슬롯 1개 - 툴팁, 좌클릭(1개), 우클릭(5개) 처리
/// </summary>
public class ShopSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 요소 (비워두면 자동 찾음)")]
    public Image iconImage;
    public TMP_Text countText;

    public ItemData Item { get; private set; }
    public bool IsBuyMode { get; private set; }

    private ShopUI shopUI;

    void Awake()
    {
        if (iconImage == null) iconImage = transform.Find("Icon")?.GetComponent<Image>();
        if (iconImage == null) iconImage = transform.Find("Item/Icon")?.GetComponent<Image>();
        if (iconImage == null) iconImage = transform.Find("Item")?.GetComponent<Image>();
        if (iconImage == null) iconImage = transform.Find("Item")?.GetComponentInChildren<Image>();
        if (countText == null) countText = transform.Find("Count")?.GetComponent<TMP_Text>();
        if (countText == null) countText = transform.Find("Item/Count")?.GetComponent<TMP_Text>();

        shopUI = GetComponentInParent<ShopUI>();
    }

    public void Setup(ItemData item, bool isBuyMode, ShopUI ui)
    {
        Item = item;
        IsBuyMode = isBuyMode;
        shopUI = ui;

        EnsureSlotReceivesPointerEvents();

        if (item == null || item.icon == null)
        {
            if (iconImage != null) { iconImage.sprite = null; iconImage.enabled = false; }
            if (countText != null) countText.text = "";
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = true;
        }

        // 판매 모드: 갯수 표시 안 함
        if (countText != null)
            countText.gameObject.SetActive(false);
    }

    /// <summary>
    /// 슬롯 루트가 포인터 이벤트를 받도록 설정 (자식 Image raycast 비활성화)
    /// </summary>
    void EnsureSlotReceivesPointerEvents()
    {
        var rootImage = GetComponent<Image>();
        if (rootImage != null && !rootImage.raycastTarget)
            rootImage.raycastTarget = true;

        foreach (var img in GetComponentsInChildren<Image>(true))
        {
            if (img.gameObject == gameObject) continue;
            if (img.raycastTarget) img.raycastTarget = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Item == null || shopUI == null) return;

        int count = (eventData.button == PointerEventData.InputButton.Left) ? 1 : 5;

        if (IsBuyMode)
            shopUI.TryBuy(Item, count);
        else
            shopUI.TrySell(Item, count);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Item == null || ItemTooltip.Instance == null) return;
        ItemTooltip.Instance.Show(Item);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltip.Instance?.Hide();
    }
}
