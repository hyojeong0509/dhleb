using UnityEngine;

[System.Serializable]
public class GiveItemEntry
{
    [Tooltip("ItemDatabase에 등록된 itemName (예: 괭이, 물뿌리개, 딸기 씨앗)")]
    public string itemName;
    public int count = 1;
}

/// <summary>
/// 이벤트 조건 + 대화. EventManager에 등록.
/// </summary>
[CreateAssetMenu(fileName = "NewEvent", menuName = "Event/Event Data")]
public class EventData : ScriptableObject
{
    [Tooltip("대상 NPC ID")]
    public string npcId;

    [Tooltip("재생할 대화")]
    public DialogueData dialogue;

    [Header("조건 (모두 만족해야 실행)")]
    [Tooltip("스토리 진행도 최소값 (-1 = 무시)")]
    public int storyProgressMin = -1;

    [Tooltip("필요한 이벤트 플래그 (하나라도 있으면 OK)")]
    public string[] eventFlagsRequired;

    [Tooltip("차단 플래그 (하나라도 있으면 실행 안 함)")]
    public string[] eventFlagsBlock;

    [Tooltip("NPC 호감도 최소값 (-1 = 무시)")]
    public int affectionMin = -1;

    [Tooltip("실행 후 설정할 플래그 (한 번만 보기)")]
    public string setFlagWhenDone;

    [Header("실행 후 액션")]
    [Tooltip("스토리 진행도 추가 (0 = 안 함)")]
    public int advanceStoryOnDone;
    [Tooltip("수락할 퀘스트 ID (비우면 안 함)")]
    public string giveQuestIdOnDone;
    [Tooltip("대화 종료 시 인벤토리에 지급할 아이템 (itemName으로 ItemDatabase에서 조회)")]
    public GiveItemEntry[] giveItemsOnDone;

    public bool AreConditionsMet()
    {
        if (GameProgressManager.Instance == null) return false;

        if (storyProgressMin >= 0 && GameProgressManager.Instance.StoryProgress < storyProgressMin)
            return false;

        if (eventFlagsBlock != null)
            foreach (var f in eventFlagsBlock)
                if (!string.IsNullOrEmpty(f) && GameProgressManager.Instance.HasFlag(f))
                    return false;

        if (eventFlagsRequired != null && eventFlagsRequired.Length > 0)
        {
            bool hasAny = false;
            foreach (var f in eventFlagsRequired)
                if (!string.IsNullOrEmpty(f) && GameProgressManager.Instance.HasFlag(f))
                    { hasAny = true; break; }
            if (!hasAny) return false;
        }

        if (affectionMin >= 0 && GameProgressManager.Instance.GetAffection(npcId) < affectionMin)
            return false;

        if (!string.IsNullOrEmpty(setFlagWhenDone) && GameProgressManager.Instance.HasFlag(setFlagWhenDone))
            return false;

        return true;
    }

    public void MarkAsDone()
    {
        if (GameProgressManager.Instance != null && !string.IsNullOrEmpty(setFlagWhenDone))
            GameProgressManager.Instance.SetFlag(setFlagWhenDone);
        if (GameProgressManager.Instance != null && advanceStoryOnDone > 0)
            GameProgressManager.Instance.AdvanceStoryProgress(advanceStoryOnDone);
        // giveItemsOnDone이 있으면 퀘스트는 대화 종료 시 지급 (아이템 먼저 → 퀘스트)
        if (QuestManager.Instance != null && !string.IsNullOrEmpty(giveQuestIdOnDone) && !HasGiveItemsOnDone)
            QuestManager.Instance.AcceptQuest(giveQuestIdOnDone);
    }

    /// <summary>대화 종료 시 호출. 아이템 지급 후 퀘스트 수락</summary>
    public void OnDialogueEnded()
    {
        // 1. 아이템 먼저 지급
        if (giveItemsOnDone != null && giveItemsOnDone.Length > 0
            && InventoryManager.Instance != null && ItemDatabase.Instance != null)
        {
            foreach (var entry in giveItemsOnDone)
            {
                if (string.IsNullOrEmpty(entry.itemName) || entry.count <= 0) continue;
                var item = ItemDatabase.Instance.GetItemByName(entry.itemName);
                if (item != null)
                    InventoryManager.Instance.AddItem(item, entry.count);
            }
        }

        // 2. 아이템 지급 이벤트인 경우 퀘스트는 대화 종료 시 수락 (트리오네스: 아이템 → 퀘스트)
        if (QuestManager.Instance != null && !string.IsNullOrEmpty(giveQuestIdOnDone) && HasGiveItemsOnDone)
            QuestManager.Instance.AcceptQuest(giveQuestIdOnDone);
    }

    public bool HasGiveItemsOnDone => giveItemsOnDone != null && giveItemsOnDone.Length > 0;
}
