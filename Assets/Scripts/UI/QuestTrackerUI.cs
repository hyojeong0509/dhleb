using UnityEngine;
using TMPro;

/// <summary>
/// 활성 퀘스트 목표 표시. Canvas에 배치하고 txtTitle, txtObjectives 연결.
/// </summary>
public class QuestTrackerUI : MonoBehaviour
{
    [Header("UI 참조")]
    public TMP_Text txtTitle;
    public TMP_Text txtObjectives;
    [Tooltip("퀘스트 없을 때 숨길 패널 (비워두면 항상 표시)")]
    public GameObject panel;

    void Start()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAccepted += OnQuestChanged;
            QuestManager.Instance.OnQuestCompleted += OnQuestChanged;
        }
        Refresh();
    }

    void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAccepted -= OnQuestChanged;
            QuestManager.Instance.OnQuestCompleted -= OnQuestChanged;
        }
    }

    void OnQuestChanged(QuestData _) => Refresh();

    void Refresh()
    {
        var quest = QuestManager.Instance?.GetActiveQuest();
        bool hasQuest = quest != null;

        if (panel != null)
            panel.SetActive(hasQuest);

        if (!hasQuest)
        {
            if (txtTitle != null) txtTitle.text = "";
            if (txtObjectives != null) txtObjectives.text = "";
            return;
        }

        if (txtTitle != null)
            txtTitle.text = quest.title;

        if (txtObjectives != null && quest.objectives != null)
        {
            var lines = new System.Collections.Generic.List<string>();
            for (int i = 0; i < quest.objectives.Count; i++)
            {
                var obj = quest.objectives[i];
                int current = QuestManager.Instance.GetObjectiveProgress(i);
                int required = obj.requiredCount;
                string target = string.IsNullOrEmpty(obj.targetId) ? "" : $" ({obj.targetId})";
                string typeStr = GetObjectiveTypeName(obj.type);
                lines.Add($"• {typeStr}{target}: {current}/{required}");
            }
            txtObjectives.text = string.Join("\n", lines);
        }
    }

    string GetObjectiveTypeName(QuestObjectiveType type)
    {
        switch (type)
        {
            case QuestObjectiveType.TalkToNpc: return "대화하기";
            case QuestObjectiveType.TillSoil: return "땅 갈기";
            case QuestObjectiveType.PlantSeed: return "씨앗 심기";
            case QuestObjectiveType.WaterSoil: return "물 주기";
            case QuestObjectiveType.Harvest: return "수확";
            case QuestObjectiveType.SetFlag: return "조건 달성";
            default: return "목표";
        }
    }
}
