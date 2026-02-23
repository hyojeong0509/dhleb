using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 퀘스트 진행 관리. 수락/완료, 목표 진행도 갱신. 다중 활성 퀘스트 지원.
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Tooltip("등록된 퀘스트 (비우면 Resources/QuestRegistry 로드)")]
    public List<QuestData> quests = new List<QuestData>();

    [Tooltip("또는 QuestRegistry 에셋 직접 지정")]
    public QuestRegistry registry;

    private List<string> activeQuestIds = new List<string>();
    private Dictionary<string, List<int>> activeQuestProgress = new Dictionary<string, List<int>>();
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

        if (activeQuestIds.Contains(questId)) return false;

        activeQuestIds.Add(questId);
        activeQuestProgress[questId] = new List<int>();
        for (int i = 0; i < data.objectives.Count; i++)
            activeQuestProgress[questId].Add(0);

        OnQuestAccepted?.Invoke(data);
        return true;
    }

    /// <summary>목표 진행 알림 (TileManager, NPCInteract 등에서 호출)</summary>
    public void NotifyObjectiveProgress(QuestObjectiveType type, string targetId = "", int amount = 1)
    {
        foreach (var questId in new List<string>(activeQuestIds))
        {
            var data = GetQuestData(questId);
            if (data == null || data.objectives == null) continue;

            for (int i = 0; i < data.objectives.Count; i++)
            {
                var obj = data.objectives[i];
                if (obj.type != type) continue;

                if (!string.IsNullOrEmpty(obj.targetId) && !string.IsNullOrEmpty(targetId)
                    && !string.Equals(obj.targetId, targetId, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!activeQuestProgress.TryGetValue(questId, out var progress)) continue;
                int idx = Mathf.Min(i, progress.Count - 1);
                if (idx < 0) continue;

                int prev = progress[idx];
                int next = Mathf.Min(prev + amount, obj.requiredCount);
                progress[idx] = next;
                break;
            }
        }
    }

    /// <summary>SetFlag 타입 목표 - 플래그 설정 시 호출</summary>
    public void NotifyFlagSet(string flagName)
    {
        NotifyObjectiveProgress(QuestObjectiveType.SetFlag, flagName, 1);
    }

    void CompleteQuest(string questId)
    {
        if (string.IsNullOrEmpty(questId) || !activeQuestIds.Contains(questId)) return;

        var data = GetQuestData(questId);
        if (data == null) return;

        activeQuestIds.Remove(questId);
        activeQuestProgress.Remove(questId);
        completedQuestIds.Add(questId);

        if (data.advanceStoryOnComplete > 0 && GameProgressManager.Instance != null)
            GameProgressManager.Instance.AdvanceStoryProgress(data.advanceStoryOnComplete);

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

            if (data.rewards.items != null && InventoryManager.Instance != null && ItemDatabase.Instance != null)
            {
                foreach (var entry in data.rewards.items)
                {
                    if (string.IsNullOrEmpty(entry.itemName) || entry.count <= 0) continue;
                    var item = ItemDatabase.Instance.GetItemByName(entry.itemName);
                    if (item != null)
                        InventoryManager.Instance.AddItem(item, entry.count);
                }
            }

            if (data.rewards.completionDialogue != null && DialogueManager.Instance != null)
                DialogueManager.Instance.Play(data.rewards.completionDialogue);
        }

        OnQuestCompleted?.Invoke(data);
    }

    bool IsAllObjectivesComplete(string questId)
    {
        if (string.IsNullOrEmpty(questId) || !activeQuestProgress.TryGetValue(questId, out var progress))
            return false;
        var data = GetQuestData(questId);
        if (data == null || data.objectives == null) return false;

        for (int i = 0; i < data.objectives.Count; i++)
        {
            int current = i < progress.Count ? progress[i] : 0;
            if (current < data.objectives[i].requiredCount) return false;
        }
        return true;
    }

    /// <summary>활성 퀘스트 목록 (퀘스트 개수만큼 UI 생성용)</summary>
    public List<QuestData> GetActiveQuests()
    {
        var list = new List<QuestData>();
        foreach (var id in activeQuestIds)
        {
            var q = GetQuestData(id);
            if (q != null) list.Add(q);
        }
        return list;
    }

    /// <summary>단일 활성 퀘스트 (QuestTrackerUI 호환용)</summary>
    public QuestData GetActiveQuest()
    {
        return activeQuestIds.Count > 0 ? GetQuestData(activeQuestIds[0]) : null;
    }

    /// <summary>특정 퀘스트 목표 진행도</summary>
    public int GetObjectiveProgress(string questId, int objectiveIndex)
    {
        if (!activeQuestProgress.TryGetValue(questId, out var progress)) return 0;
        if (objectiveIndex < 0 || objectiveIndex >= progress.Count) return 0;
        return progress[objectiveIndex];
    }

    public bool IsQuestActive(string questId) => activeQuestIds.Contains(questId);
    public bool IsQuestCompleted(string questId) => completedQuestIds.Contains(questId);

    public bool IsReadyToClaim(string questId) => IsAllObjectivesComplete(questId);

    /// <summary>보상 수령 (UI RewardAccept 버튼에서 호출)</summary>
    public void ClaimReward(string questId)
    {
        if (string.IsNullOrEmpty(questId) || !IsReadyToClaim(questId)) return;
        CompleteQuest(questId);
    }

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

        activeQuestIds.Clear();
        activeQuestProgress.Clear();
        completedQuestIds.Clear();

        // 구버전 호환
        if (!string.IsNullOrEmpty(data.activeQuestId) && (data.activeQuestIds == null || data.activeQuestIds.Count == 0))
        {
            activeQuestIds.Add(data.activeQuestId);
            if (data.activeQuestProgress != null && data.activeQuestProgress.Count > 0)
            {
                var entry = new QuestProgressEntry { questId = data.activeQuestId, progress = new List<int>(data.activeQuestProgress) };
                activeQuestProgress[data.activeQuestId] = entry.progress;
            }
            else
            {
                var q = GetQuestData(data.activeQuestId);
                if (q != null)
                {
                    var progress = new List<int>();
                    for (int i = 0; i < q.objectives.Count; i++) progress.Add(0);
                    activeQuestProgress[data.activeQuestId] = progress;
                }
            }
        }
        else if (data.activeQuestIds != null && data.activeQuestProgressList != null)
        {
            for (int i = 0; i < data.activeQuestIds.Count; i++)
            {
                var id = data.activeQuestIds[i];
                activeQuestIds.Add(id);
                if (i < data.activeQuestProgressList.Count && data.activeQuestProgressList[i].questId == id)
                    activeQuestProgress[id] = new List<int>(data.activeQuestProgressList[i].progress);
                else
                {
                    var q = GetQuestData(id);
                    var progress = new List<int>();
                    if (q != null) for (int j = 0; j < q.objectives.Count; j++) progress.Add(0);
                    activeQuestProgress[id] = progress;
                }
            }
        }

        if (data.completedQuestIds != null)
            foreach (var id in data.completedQuestIds)
                if (!string.IsNullOrEmpty(id))
                    completedQuestIds.Add(id);
    }

    public void FillSaveData(QuestSaveData data)
    {
        if (data == null) return;

        data.activeQuestId = activeQuestIds.Count > 0 ? activeQuestIds[0] : "";
        data.activeQuestIds = new List<string>(activeQuestIds);
        data.activeQuestProgressList.Clear();
        foreach (var id in activeQuestIds)
        {
            if (activeQuestProgress.TryGetValue(id, out var progress))
                data.activeQuestProgressList.Add(new QuestProgressEntry { questId = id, progress = new List<int>(progress) });
        }
        data.completedQuestIds = new List<string>(completedQuestIds);
    }
}
