using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 인벤토리에서 아이템을 들고 있을 때 마우스를 따라다니는 아이템 이미지
/// </summary>
public class CursorItem : MonoBehaviour
{
    public static CursorItem Instance { get; private set; }

    [Header("UI 요소")]
    public Image iconImage;
    public TMP_Text countText;

    private RectTransform rt;
    private RectTransform canvasRt;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        rt = GetComponent<RectTransform>();
        canvasRt = GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        // 클릭을 가로막지 않게 설정 (슬롯 스왑/놓기 버그 방지)
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
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRt, Input.mousePosition,
            GetComponentInParent<Canvas>().worldCamera,
            out Vector2 pos
        );
        rt.anchoredPosition = pos;
    }

    public void Show(ItemData item, int count)
    {
        gameObject.SetActive(true);

        if (iconImage != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = item.icon != null;
        }

        if (countText != null)
            countText.text = count > 1 ? count.ToString() : "";
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
