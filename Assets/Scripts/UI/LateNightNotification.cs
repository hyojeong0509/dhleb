using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 01:30 도달 시 하단 팝업 "시간이 늦어지고있어요"
/// </summary>
public class LateNightNotification : MonoBehaviour
{
    [Header("팝업")]
    public GameObject popupPanel;
    public TMP_Text popupText;

    [Header("설정")]
    public float displayDuration = 3f;

    private bool hasShownThisDay;

    void Start()
    {
        if (popupPanel != null) popupPanel.SetActive(false);
        if (popupText != null && string.IsNullOrEmpty(popupText.text))
            popupText.text = "시간이 늦어지고있어요";

        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnMinuteChanged += CheckAndShow;
            TimeManager.Instance.OnDayChanged += OnDayChanged;
        }
    }

    void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnMinuteChanged -= CheckAndShow;
            TimeManager.Instance.OnDayChanged -= OnDayChanged;
        }
    }

    void OnDayChanged()
    {
        hasShownThisDay = false;
    }

    void CheckAndShow()
    {
        if (hasShownThisDay || TimeManager.Instance == null) return;
        if (!TimeManager.Instance.IsPast0130) return;

        hasShownThisDay = true;
        Show();
    }

    void Show()
    {
        if (popupText != null)
            popupText.text = "시간이 늦어지고있어요";
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
            Invoke(nameof(Hide), displayDuration);
        }
    }

    void Hide()
    {
        if (popupPanel != null) popupPanel.SetActive(false);
    }
}
