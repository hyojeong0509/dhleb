using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 퀘스트 진행 관리. 수락/완료, 목표 진행도 갱신.
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Tooltip("등록된 퀘스트 (비우면 Resources/QuestRegistry 로드)")]
    public List<QuestData> quests = new List<QuestData>();

    [Tooltip("또는 QuestRegistry 에셋 직접 지정")]
    public QuestRegistry registry;

    private string activeQuestId;
    private List<int> activeQuestProgress = new List<int>();
    private HashSet<string> completedQuestIds = new HashSet<string>();

    public event Action<QuestData> OnQuestAccepted;
    public event Action<QuestData> OnQuestCompleted;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (quests.Count == 0 && registry != null && registry.quests != null)
            quests.AddRange(registry.quests);
        if (quests.Count == 0)
        {
            var reg = Resources.Load<QuestRegistry>("QuestRegistry");
            if (reg != null && reg.quests != null)
                quests.AddRange(reg.quests);
        }
    }

    /// <summary>퀘스트 수락</summary>
    public bool AcceptQuest(string questId)
    {
        var data = GetQuestData(questId);
        if (data == null || !data.CanAccept()) return false;

        activeQuestId = questId;
        activeQuestProgress.Clear();
        for (int i = 0; i < data.objectives.Count; i++)
            activeQuestProgress.Add(0);

        OnQuestAccepted?.Invoke(data);
        return true;
    }

    /// <summary>목표 진행 알림 (TileManager, NPCInteract 등에서 호출)</summary>
    public void NotifyObjectiveProgress(QuestObjectiveType type, string targetId = "", int amount = 1)
    {
        if (string.IsNullOrEmpty(activeQuestId)) return;

        var data = GetQuestData(activeQuestId);
        if (data == null || data.objectives == null) return;

        for (int i = 0; i < data.objectives.Count; i++)
        {
            var obj = data.objectives[i];
            if (obj.type != type) continue;

            // targetId 매칭 (비어있으면 타입만, 있으면 일치해야 함)
            if (!string.IsNullOrEmpty(obj.targetId) && !string.IsNullOrEmpty(targetId)
                && !string.Equals(obj.targetId, targetId, StringComparison.OrdinalIgnoreCase))
                continue;

            int idx = Mathf.Min(i, activeQuestProgress.Count - 1);
            if (idx < 0) continue;

            int prev = activeQuestProgress[idx];
            int next = Mathf.Min(prev + amount, obj.requiredCount);
            activeQuestProgress[idx] = next;

            if (IsAllObjectivesComplete())
                CompleteQuest();
            break;
        }
    }

    /// <summary>SetFlag 타입 목표 - 플래그 설정 시 호출</summary>
    public void NotifyFlagSet(string flagName)
    {
        NotifyObjectiveProgress(QuestObjectiveType.SetFlag, flagName, 1);
    }

    /// <summary>퀘스트 완료 처리</summary>
    void CompleteQuest()
    {
        if (string.IsNullOrEmpty(activeQuestId)) return;

        var data = GetQuestData(activeQuestId);
        if (data == null) return;

        completedQuestIds.Add(activeQuestId);

        if (data.advanceStoryOnComplete > 0 && GameProgressManager.Instance != null)
            GameProgressManager.Instance.AdvanceStoryProgress(data.advanceStoryOnComplete);

        // 보상 지급
        if (data.rewards != null)
        {
            if (data.rewards.gold > 0 && GoldManager.Instance != null)
                GoldManager.Instance.AddGold(data.rewards.gold);

            if (data.rewards.npcAffections != null && GameProgressManager.Instance != null)
            {
                foreach (var s in data.rewards.npcAffections)
                {
                    if (string.IsNullOrEmpty(s)) continue;
                    var parts = s.Split(':');
                    if (parts.Length >= 2 && int.TryParse(parts[1], out int amt))
                        GameProgressManager.Instance.AddAffection(parts[0].Trim(), amt);
                }
            }

            if (data.rewards.completionDialogue != null && DialogueManager.Instance != null)
                DialogueManager.Instance.Play(data.rewards.completionDialogue);
        }

        OnQuestCompleted?.Invoke(data);

        activeQuestId = null;
        activeQuestProgress.Clear();
    }

    bool IsAllObjectivesComplete()
    {
        if (string.IsNullOrEmpty(activeQuestId)) return false;
        var data = GetQuestData(activeQuestId);
        if (data == null || data.objectives == null) return false;

        for (int i = 0; i < data.objectives.Count; i++)
        {
            int current = i < activeQuestProgress.Count ? activeQuestProgress[i] : 0;
            int required = data.objectives[i].requiredCount;
            if (current < required) return false;
        }
        return true;
    }

    public QuestData GetActiveQuest()
    {
        return GetQuestData(activeQuestId);
    }

    public string ActiveQuestId => activeQuestId;

    /// <summary>현재 목표별 진행도 (0~requiredCount)</summary>
    public int GetObjectiveProgress(int objectiveIndex)
    {
        if (objectiveIndex < 0 || objectiveIndex >= activeQuestProgress.Count)
            return 0;
        return activeQuestProgress[objectiveIndex];
    }

    public bool IsQuestActive(string questId) => activeQuestId == questId;
    public bool IsQuestCompleted(string questId) => completedQuestIds.Contains(questId);

    public QuestData GetQuestData(string questId)
    {
        if (string.IsNullOrEmpty(questId) || quests == null) return null;
        foreach (var q in quests)
            if (q != null && q.questId == questId)
                return q;
        return null;
    }

    // ── Save/Load ─────────────────────────────────────────────

    public void LoadFromSaveData(QuestSaveData data)
    {
        if (data == null) return;

        activeQuestId = data.activeQuestId ?? "";
        activeQuestProgress.Clear();
        if (data.activeQuestProgress != null)
            activeQuestProgress.AddRange(data.activeQuestProgress);

        completedQuestIds.Clear();
        if (data.completedQuestIds != null)
            foreach (var id in data.completedQuestIds)
                if (!string.IsNullOrEmpty(id))
                    completedQuestIds.Add(id);
    }

    public void FillSaveData(QuestSaveData data)
    {
        if (data == null) return;

        data.activeQuestId = activeQuestId ?? "";
        data.activeQuestProgress = new List<int>(activeQuestProgress);
        data.completedQuestIds = new List<string>(completedQuestIds);
    }
}
