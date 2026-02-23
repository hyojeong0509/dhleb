using UnityEngine;

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
        if (QuestManager.Instance != null && !string.IsNullOrEmpty(giveQuestIdOnDone))
            QuestManager.Instance.AcceptQuest(giveQuestIdOnDone);
    }
}
