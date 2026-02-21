using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 슬롯 하나의 UI를 담당 (핫바 슬롯, 인벤토리 슬롯 모두 사용)
/// 자식 오브젝트 이름이 "Icon", "Count", "Select" 이면 자동으로 찾음
/// </summary>
public class InventorySlotUI : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 요소 (비워두면 자동으로 찾음)")]
    public Image iconImage;
    public TMP_Text countText;
    public Image selectImage;

    // 슬롯 인덱스 (InventoryManager에서 Setup 시 할당)
    public int SlotIndex { get; private set; }

    void Awake()
    {
        if (iconImage == null)
            iconImage = transform.Find("Icon")?.GetComponent<Image>();
        if (countText == null)
            countText = transform.Find("Count")?.GetComponent<TMP_Text>();
        if (selectImage == null)
            selectImage = transform.Find("Select")?.GetComponent<Image>();

        if (selectImage != null)
            selectImage.gameObject.SetActive(false);
    }

    public void Setup(int index)
    {
        SlotIndex = index;
    }

    // ── 포인터 이벤트 ────────────────────────────────────────

    public void OnPointerClick(PointerEventData eventData)
    {
        if (InventoryManager.Instance == null) return;

        if (eventData.button == PointerEventData.InputButton.Left)
            InventoryManager.Instance.OnSlotLeftClick(SlotIndex);
        else if (eventData.button == PointerEventData.InputButton.Right)
            InventoryManager.Instance.OnSlotRightClick(SlotIndex);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ItemTooltip.Instance == null) return;
        if (InventoryManager.Instance == null) return;

        // 인벤토리가 열려있을 때만 툴팁 표시
        if (!InventoryManager.Instance.IsInventoryOpen) return;

        var data = InventoryManager.Instance.GetSlotData(SlotIndex);
        if (data != null && !data.IsEmpty)
            ItemTooltip.Instance.Show(data.item);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltip.Instance?.Hide();
    }

    // ── 슬롯 UI 갱신 ────────────────────────────────────────

    public void UpdateSlot(InventorySlotData data)
    {
        if (data == null || data.IsEmpty)
        {
            ClearSlot();
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = data.item.icon;
            iconImage.enabled = data.item.icon != null;
        }

        if (countText != null)
            countText.text = data.count > 1 ? data.count.ToString() : "";
    }

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

    public void SetSelected(bool selected)
    {
        if (selectImage != null)
            selectImage.gameObject.SetActive(selected);
    }
}
