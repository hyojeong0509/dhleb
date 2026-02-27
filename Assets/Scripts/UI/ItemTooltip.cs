using UnityEngine;
using TMPro;

// 인벤토리 슬롯 마우스 오버 시 아이템 정보 툴팁 표시
public class ItemTooltip : MonoBehaviour
{
    public static ItemTooltip Instance { get; private set; }

    [Header("텍스트")]
    public TMP_Text txtName;
    public TMP_Text txtType;
    public TMP_Text txtDescription;
    [Tooltip("구매 가격")]
    public TMP_Text txtBuyPrice;
    [Tooltip("판매 가격")]
    public TMP_Text txtSellPrice;

    private RectTransform rt;
    private RectTransform canvasRt;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        rt = GetComponent<RectTransform>();
        canvasRt = GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        // 툴팁이 마우스 이벤트를 가로막지 않게 설정 (깜빡임 방지)
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;

        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!gameObject.activeSelf) return;
        FollowMouse();
    }

    void FollowMouse()
    {
        // 마우스 스크린 좌표 → 캔버스 로컬 좌표 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRt, Input.mousePosition,
            GetComponentInParent<Canvas>().worldCamera,
            out Vector2 pos
        );

        // 툴팁 크기 기반으로 오른쪽 하단에 배치
        float w = rt.rect.width;
        float h = rt.rect.height;
        rt.anchoredPosition = pos + new Vector2(w * 0.5f + 40f, -h * 0.5f - 30f); // (숫자 늘리면 오른쪽으로 더 감, -숫자 늘리면 아래로 더 감)
    }

    public void Show(ItemData item)
    {
        if (item == null) { Hide(); return; }

        gameObject.SetActive(true);

        if (txtName != null) txtName.text = item.itemName;
        if (txtType != null) txtType.text = item.itemType.ToString();
        if (txtDescription != null) txtDescription.text = item.description;

        if (txtBuyPrice != null)
        {
            txtBuyPrice.gameObject.SetActive(item.buyPrice > 0);
            txtBuyPrice.text = item.buyPrice > 0 ? $"{item.buyPrice:N0}G" : "";
        }
        if (txtSellPrice != null)
        {
            txtSellPrice.gameObject.SetActive(item.sellPrice > 0);
            txtSellPrice.text = item.sellPrice > 0 ? $"{item.sellPrice:N0}G" : "";
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
