using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ic_Quest 아이콘에 붙이는 스크립트.
/// - 퀘스트 수락/완료 시 QuestNotice 활성화
/// - 아이콘 클릭 시 QuestNotice 비활성화
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class QuestNoticeController : MonoBehaviour
{
    [Header("알림 오브젝트")]
    [Tooltip("활성/비활성할 QuestNotice 오브젝트. 비우면 자식 중 'QuestNotice' 이름으로 찾음")]
    public GameObject questNoticeObject;

    [Header("클릭 감지")]
    [Tooltip("클릭할 버튼. 비우면 이 오브젝트의 Button 사용")]
    public Button targetButton;

    void Awake()
    {
        if (questNoticeObject == null)
        {
            var found = transform.Find("QuestNotice");
            if (found != null)
                questNoticeObject = found.gameObject;
        }
    }

    void Start()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAccepted += ShowNotice;
            QuestManager.Instance.OnQuestCompleted += ShowNotice;
        }

        // 아이콘(버튼) 클릭 시 알림 숨기기
        var btn = targetButton != null ? targetButton : GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(HideNotice);
    }

    void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAccepted -= ShowNotice;
            QuestManager.Instance.OnQuestCompleted -= ShowNotice;
        }
    }

    void ShowNotice(QuestData _)
    {
        if (questNoticeObject != null)
            questNoticeObject.SetActive(true);
    }

    /// <summary>아이콘 클릭 시 호출. Button onClick에 수동 연결해도 됨.</summary>
    public void HideNotice()
    {
        if (questNoticeObject != null)
            questNoticeObject.SetActive(false);
    }
}
