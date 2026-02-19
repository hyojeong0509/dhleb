using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 슬롯 하나의 UI를 담당 (핫바 슬롯, 인벤토리 슬롯 모두 사용)
/// 자식 오브젝트 이름이 "Icon", "Count", "Select" 이면 자동으로 찾음
/// </summary>
public class InventorySlotUI : MonoBehaviour
{
    [Header("UI 요소 (비워두면 자동으로 찾음)")]
    public Image iconImage;       // 아이템 아이콘 (Icon)
    public TMP_Text countText;    // 수량 텍스트 (Count)
    public Image selectImage;     // 선택 표시 (Select)

    void Awake()
    {
        // Inspector에서 직접 연결하지 않았으면 자식 이름으로 자동 탐색
        if (iconImage == null)
            iconImage = transform.Find("Icon")?.GetComponent<Image>();

        if (countText == null)
            countText = transform.Find("Count")?.GetComponent<TMP_Text>();

        if (selectImage == null)
            selectImage = transform.Find("Select")?.GetComponent<Image>();

        // Select 이미지는 기본적으로 비활성화
        if (selectImage != null)
            selectImage.enabled = false;
    }

    /// <summary>
    /// 슬롯 데이터로 UI 갱신
    /// </summary>
    public void UpdateSlot(InventorySlotData data)
    {
        if (data == null || data.IsEmpty)
        {
            ClearSlot();
            return;
        }

        // 아이콘 표시
        if (iconImage != null)
        {
            iconImage.sprite = data.item.icon;
            iconImage.enabled = data.item.icon != null;
        }

        // 수량 표시 (1개면 숨김, 2개 이상이면 표시)
        if (countText != null)
        {
            countText.text = data.count > 1 ? data.count.ToString() : "";
        }
    }

    /// <summary>
    /// 슬롯 비우기
    /// </summary>
    public void ClearSlot()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (countText != null)
            countText.text = "";
    }

    /// <summary>
    /// 선택 표시 On/Off
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (selectImage != null)
            selectImage.enabled = selected;
    }
}
