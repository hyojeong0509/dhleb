using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 퀘스트 목표 타입
/// </summary>
public enum QuestObjectiveType
{
    TalkToNpc,      // NPC와 대화 (targetId = npcId)
    TillSoil,       // 땅 갈기 (targetId 비움, requiredCount)
    PlantSeed,      // 씨앗 심기 (targetId = seed itemName 또는 비움)
    WaterSoil,      // 물 주기 (requiredCount)
    Harvest,        // 수확 (targetId = crop itemName 또는 비움)
    SetFlag         // 플래그 설정 시 완료 (targetId = flagName, 이벤트/컷신에서 설정)
}

/// <summary>
/// 퀘스트 목표 1개
/// </summary>
[System.Serializable]
public class QuestObjectiveData
{
    public QuestObjectiveType type;
    [Tooltip("NPC ID, 씨앗/작물 이름, 플래그 이름 등 (비워두면 타입만 체크)")]
    public string targetId;
    [Tooltip("필요 수량 (TalkToNpc, SetFlag는 1로 고정)")]
    public int requiredCount = 1;
}

/// <summary>
/// 퀘스트 보상
/// </summary>
[System.Serializable]
public class QuestRewardData
{
    [Tooltip("골드 보상")]
    public int gold;
    [Tooltip("NPC 호감도 (npcId:amount 형식, 여러 개 가능)")]
    public string[] npcAffections;
    [Tooltip("아이템 보상 (itemName, count)")]
    public GiveItemEntry[] items;
    [Tooltip("완료 시 재생할 대화 (선택)")]
    public DialogueData completionDialogue;
}

/// <summary>
/// 퀘스트 데이터. Create → Quest → Quest Data
/// </summary>
[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest/Quest Data")]
public class QuestData : ScriptableObject
{
    [Tooltip("고유 ID (영문, 소문자 권장)")]
    public string questId;

    [Header("표시")]
    public string title;
    [TextArea(2, 4)]
    public string description;

    [Header("목표")]
    public List<QuestObjectiveData> objectives = new List<QuestObjectiveData>();

    [Header("보상")]
    public QuestRewardData rewards;
    [Tooltip("완료 시 스토리 진행도 추가 (0 = 안 함)")]
    public int advanceStoryOnComplete;

    [Header("수락 조건")]
    [Tooltip("선행 퀘스트 (모두 완료해야 수락 가능)")]
    public string[] prerequisiteQuestIds;
    [Tooltip("필요 스토리 진행도 (-1 = 무시)")]
    public int prerequisiteStoryProgress = -1;
    [Tooltip("필요 플래그 (하나라도 있으면 수락 가능)")]
    public string[] prerequisiteFlags;

    /// <summary>수락 가능한지</summary>
    public bool CanAccept()
    {
        if (QuestManager.Instance == null || GameProgressManager.Instance == null)
            return false;

        if (QuestManager.Instance.IsQuestCompleted(questId))
            return false;

        if (QuestManager.Instance.IsQuestActive(questId))
            return false;

        if (prerequisiteQuestIds != null)
            foreach (var qid in prerequisiteQuestIds)
                if (!string.IsNullOrEmpty(qid) && !QuestManager.Instance.IsQuestCompleted(qid))
                    return false;

        if (prerequisiteStoryProgress >= 0 && GameProgressManager.Instance.StoryProgress < prerequisiteStoryProgress)
            return false;

        if (prerequisiteFlags != null && prerequisiteFlags.Length > 0)
        {
            bool hasAny = false;
            foreach (var f in prerequisiteFlags)
                if (!string.IsNullOrEmpty(f) && GameProgressManager.Instance.HasFlag(f))
                { hasAny = true; break; }
            if (!hasAny) return false;
        }

        return true;
    }
}
