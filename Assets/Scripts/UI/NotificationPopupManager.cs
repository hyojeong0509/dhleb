using UnityEngine;
using TMPro;

/// <summary>
/// 하단 팝업 알림 (호감도, 시간 등). LateNightNotification과 동일한 스타일.
/// CutsceneAction 등에서 NotificationPopupManager.Instance.Show("호감도 +5", 2.5f) 호출.
/// </summary>
public class NotificationPopupManager : MonoBehaviour
{
    public static NotificationPopupManager Instance { get; private set; }

    [Header("팝업")]
    public GameObject popupPanel;
    public TMP_Text popupText;

    [Header("설정")]
    public float defaultDuration = 2.5f;

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

    void Start()
    {
        if (popupPanel != null) popupPanel.SetActive(false);
    }

    /// <summary>팝업 표시 (시간이 늦어지고있어요 스타일)</summary>
    public void Show(string text, float duration = -1f)
    {
        if (popupText != null)
            popupText.text = text ?? "";

        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
            float d = duration > 0 ? duration : defaultDuration;
            CancelInvoke(nameof(Hide));
            Invoke(nameof(Hide), d);
        }
    }

    void Hide()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }
}
