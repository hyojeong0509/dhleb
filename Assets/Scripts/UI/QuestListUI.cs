using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 퀘스트 목록 UI. Content 하위에 QuestList 버튼들을 생성하고, 클릭 시 Pn_Quest2 상세 패널 표시.
/// </summary>
public class QuestListUI : MonoBehaviour
{
    [Header("목록")]
    [Tooltip("QuestList 버튼들이 들어갈 Content (Scroll View > Viewport > Content)")]
    public Transform content;
    [Tooltip("QuestList 버튼 템플릿 (하위에 제목 Text 있음). 비활성화된 상태로 두고 복제용")]
    public GameObject questListTemplate;

    [Header("상세 패널")]
    [Tooltip("퀘스트 클릭 시 열리는 상세 패널 (Pn_Quest2)")]
    public GameObject detailPanel;

    [Header("빈 상태")]
    [Tooltip("퀘스트 없을 때 표시할 텍스트 (Content 자식). 있으면 연결")]
    public GameObject emptyStateObject;

    private QuestDetailUI detailUI;
    private List<GameObject> spawnedItems = new List<GameObject>();

    void Start()
    {
        if (detailPanel != null)
            detailUI = detailPanel.GetComponent<QuestDetailUI>();

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAccepted += OnQuestChanged;
            QuestManager.Instance.OnQuestCompleted += OnQuestChanged;
            QuestManager.Instance.OnQuestDataLoaded += OnQuestDataLoadedHandler;
        }

        RefreshList();
    }

    void OnEnable()
    {
        RefreshList();
    }

    void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAccepted -= OnQuestChanged;
            QuestManager.Instance.OnQuestCompleted -= OnQuestChanged;
            QuestManager.Instance.OnQuestDataLoaded -= OnQuestDataLoadedHandler;
        }
    }

    void OnQuestChanged(QuestData _) => RefreshList();
    void OnQuestDataLoadedHandler() => RefreshList();

    public void RefreshList()
    {
        var qm = QuestManager.Instance;
        if (qm == null || content == null) return;

        foreach (var go in spawnedItems)
        {
            if (go != null) Destroy(go);
        }
        spawnedItems.Clear();

        var activeQuests = qm.GetActiveQuests();
        bool hasQuest = activeQuests != null && activeQuests.Count > 0;

        if (emptyStateObject != null)
            emptyStateObject.SetActive(!hasQuest);

        if (questListTemplate != null)
            questListTemplate.SetActive(false);

        if (!hasQuest) return;

        foreach (var quest in activeQuests)
        {
            var item = CreateQuestListItem(quest);
            if (item != null)
                spawnedItems.Add(item);
        }
    }

    GameObject CreateQuestListItem(QuestData quest)
    {
        if (questListTemplate == null || content == null) return null;

        var go = Instantiate(questListTemplate, content);
        go.SetActive(true);

        var btn = go.GetComponent<Button>();
        if (btn != null)
        {
            var q = quest;
            btn.onClick.AddListener(() => OnQuestListClicked(q));
        }

        var titleTxt = go.GetComponentInChildren<TMP_Text>();
        if (titleTxt != null)
            titleTxt.text = quest.title;

        return go;
    }

    void OnQuestListClicked(QuestData quest)
    {
        if (detailPanel != null)
            detailPanel.SetActive(true);

        if (detailUI != null)
            detailUI.ShowQuest(quest);
    }
}
