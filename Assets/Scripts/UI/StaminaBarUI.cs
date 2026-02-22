using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 스테미나바 UI - StaminaManager와 연동하여 fillAmount 갱신
/// 마우스 오버 시 숫자 표시 (하위 txt)
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class StaminaBarUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("참조")]
    [Tooltip("바를 채우는 Image (Image Type: Filled)")]
    public Image fillImage;

    [Tooltip("마우스 오버 시 표시할 숫자 텍스트 (하위)")]
    public TMP_Text txtStamina;

    void Start()
    {
        if (fillImage == null)
        {
            var fillTr = transform.Find("Fill");
            if (fillTr != null)
                fillImage = fillTr.GetComponent<Image>();
            if (fillImage == null)
                fillImage = GetComponentInChildren<Image>();
        }

        if (txtStamina == null)
        {
            var txtTr = transform.Find("txt") ?? transform.Find("Txt") ?? transform.Find("Text");
            if (txtTr != null)
                txtStamina = txtTr.GetComponent<TMP_Text>();
            if (txtStamina == null)
                txtStamina = GetComponentInChildren<TMP_Text>();
        }

        if (fillImage != null)
            fillImage.type = Image.Type.Filled;

        if (txtStamina != null)
            txtStamina.gameObject.SetActive(false);

        // 마우스 오버 감지 - 부모에 Image가 있어야 함 (raycastTarget 체크)
        var bgImage = GetComponent<Image>();
        if (bgImage != null)
            bgImage.raycastTarget = true;

        if (StaminaManager.Instance != null)
        {
            StaminaManager.Instance.OnStaminaChanged += Refresh;
            Refresh();
        }
    }

    void OnDestroy()
    {
        if (StaminaManager.Instance != null)
            StaminaManager.Instance.OnStaminaChanged -= Refresh;
    }

    void Refresh()
    {
        if (fillImage == null || StaminaManager.Instance == null) return;
        fillImage.fillAmount = StaminaManager.Instance.NormalizedStamina;

        if (txtStamina != null && txtStamina.gameObject.activeSelf)
            UpdateStaminaText();
    }

    void UpdateStaminaText()
    {
        if (txtStamina == null || StaminaManager.Instance == null) return;
        txtStamina.text = $"{StaminaManager.Instance.CurrentStamina}/{StaminaManager.Instance.MaxStamina}";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (txtStamina != null)
        {
            UpdateStaminaText();
            txtStamina.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (txtStamina != null)
            txtStamina.gameObject.SetActive(false);
    }
}
