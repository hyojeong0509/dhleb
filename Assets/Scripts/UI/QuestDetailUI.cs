using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 퀘스트 상세 패널 (Pn_Quest2). 제목, 설명, 목표, 보상 표시. RewardAccept 클릭 시 보상 수령.
/// </summary>
public class QuestDetailUI : MonoBehaviour
{
    [Header("텍스트")]
    public TMP_Text titleTxt;
    [Tooltip("퀘스트 설명 텍스트")]
    public TMP_Text descriptionTxt;
    public TMP_Text countTxt;  // 목표 진행 (0/5 형식)

    [Header("보상")]
    public GameObject rewardRoot;  // Reward 빈 오브젝트
    public GameObject rewardBG;    // RewardBG
    public Image icReward;         // Ic_Reward (아이템 아이콘, 없으면 비활성)
    public TMP_Text rewardCountTxt; // RewardCount (500골드, 씨앗 5개 등)
    public Button rewardAcceptBtn;  // RewardAccept (조건 달성 시 활성화)

    [Header("닫기")]
    public Button closeBtn;  // CloseBtn

    private QuestData currentQuest;

    void Start()
    {
        if (rewardAcceptBtn != null)
            rewardAcceptBtn.onClick.AddListener(OnRewardAcceptClicked);

        if (closeBtn != null)
            closeBtn.onClick.AddListener(ClosePanel);

        // Interactable으로 제어 (버튼은 항상 보이되, 조건 달성 시에만 클릭 가능)
        if (rewardAcceptBtn != null)
            rewardAcceptBtn.interactable = false;
    }

    public void ShowQuest(QuestData quest)
    {
        currentQuest = quest;
        var qm = QuestManager.Instance;
        if (qm == null || quest == null) return;

        if (titleTxt != null)
            titleTxt.text = quest.title;

        if (descriptionTxt != null)
            descriptionTxt.text = quest.description;

        if (countTxt != null && quest.objectives != null)
        {
            var lines = new List<string>();
            for (int i = 0; i < quest.objectives.Count; i++)
            {
                var obj = quest.objectives[i];
                int current = qm.GetObjectiveProgress(quest.questId, i);
                int required = obj.requiredCount;
                string typeStr = GetObjectiveTypeName(obj.type);
                lines.Add($"{typeStr}: {current}/{required}");
            }
            countTxt.text = string.Join("\n", lines);
        }

        // 보상 텍스트
        if (rewardCountTxt != null && quest.rewards != null)
        {
            var parts = new List<string>();
            if (quest.rewards.gold > 0)
                parts.Add($"{quest.rewards.gold}골드");
            if (quest.rewards.items != null)
            {
                foreach (var e in quest.rewards.items)
                {
                    if (string.IsNullOrEmpty(e.itemName) || e.count <= 0) continue;
                    parts.Add($"{e.itemName} {e.count}개");
                }
            }
            if (quest.rewards.npcAffections != null)
            {
                foreach (var s in quest.rewards.npcAffections)
                {
                    if (string.IsNullOrEmpty(s)) continue;
                    var p = s.Split(':');
                    if (p.Length >= 2 && int.TryParse(p[1], out int amt))
                        parts.Add($"{p[0].Trim()} 호감도 +{amt}");
                }
            }
            rewardCountTxt.text = parts.Count > 0 ? string.Join(", ", parts) : "없음";
        }

        // RewardAccept: 조건 달성 시에만 interactable
        bool ready = qm.IsReadyToClaim(quest.questId);
        if (rewardAcceptBtn != null)
            rewardAcceptBtn.interactable = ready;
    }

    void Update()
    {
        if (currentQuest == null || !gameObject.activeInHierarchy) return;

        var qm = QuestManager.Instance;
        if (qm == null || !qm.IsQuestActive(currentQuest.questId)) return;

        // 진행도 실시간 갱신
        if (countTxt != null && currentQuest.objectives != null)
        {
            var lines = new List<string>();
            for (int i = 0; i < currentQuest.objectives.Count; i++)
            {
                var obj = currentQuest.objectives[i];
                int current = qm.GetObjectiveProgress(currentQuest.questId, i);
                int required = obj.requiredCount;
                string typeStr = GetObjectiveTypeName(obj.type);
                lines.Add($"{typeStr}: {current}/{required}");
            }
            countTxt.text = string.Join("\n", lines);
        }

        // 조건 달성 시 버튼 interactable
        if (qm.IsReadyToClaim(currentQuest.questId) && rewardAcceptBtn != null)
            rewardAcceptBtn.interactable = true;
    }

    void OnRewardAcceptClicked()
    {
        if (QuestManager.Instance == null || currentQuest == null) return;
        if (!QuestManager.Instance.IsReadyToClaim(currentQuest.questId)) return;

        QuestManager.Instance.ClaimReward(currentQuest.questId);
        ClosePanel();
    }

    void ClosePanel()
    {
        gameObject.SetActive(false);
        currentQuest = null;
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
