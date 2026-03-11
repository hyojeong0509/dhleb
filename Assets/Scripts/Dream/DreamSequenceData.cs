using UnityEngine;

/// <summary>
/// 꿈 시퀀스 조건 + 재생할 컷신/대화.
/// BedInteract에서 침대 Sleep 시 조건을 검사해 재생.
/// </summary>
[CreateAssetMenu(fileName = "NewDreamSequence", menuName = "Dream/Dream Sequence Data")]
public class DreamSequenceData : ScriptableObject
{
    [Header("재생할 콘텐츠")]
    [Tooltip("재생할 컷신 (우선)")]
    public CutsceneData cutscene;
    [Tooltip("컷신이 없을 때 재생할 대화")]
    public DialogueData dialogue;

    [Header("날짜 조건")]
    [Tooltip("최소 일차 (-1 = 무시). 예: 1이면 1일차 이후")]
    public int minDay = -1;
    [Tooltip("최대 일차 (-1 = 무시). 예: 5이면 5일차 이전")]
    public int maxDay = -1;

    [Header("확률")]
    [Tooltip("0~1. 1 = 무조건, 0.1 = 10%")]
    [Range(0f, 1f)]
    public float probability = 1f;

    [Header("선행 조건")]
    [Tooltip("이 꿈을 보려면 필요한 플래그 (모두 있어야 함). 예: saw_dream_1, saw_dream_2")]
    public string[] requiredFlags;

    [Header("한 번만 보기")]
    [Tooltip("재생 후 설정할 플래그 (중복 재생 방지). 예: saw_dream_1")]
    public string setFlagWhenDone;

    /// <summary>
    /// 조건 만족 여부. dayBeforeSleep = 잠 자기 직전 날짜 (1일차 잠 = 1).
    /// </summary>
    public bool AreConditionsMet(int dayBeforeSleep)
    {
        if (GameProgressManager.Instance == null) return false;

        // 이미 본 꿈이면 스킵
        if (!string.IsNullOrEmpty(setFlagWhenDone) && GameProgressManager.Instance.HasFlag(setFlagWhenDone))
            return false;

        // 날짜 범위
        if (minDay >= 0 && dayBeforeSleep < minDay) return false;
        if (maxDay >= 0 && dayBeforeSleep > maxDay) return false;

        // 선행 플래그 (모두 있어야 함)
        if (requiredFlags != null)
        {
            foreach (var f in requiredFlags)
            {
                if (string.IsNullOrEmpty(f)) continue;
                if (!GameProgressManager.Instance.HasFlag(f))
                    return false;
            }
        }

        // 확률
        if (probability < 1f && Random.value > probability)
            return false;

        // 재생할 콘텐츠가 있어야 함
        if (cutscene == null && dialogue == null)
            return false;

        return true;
    }

    /// <summary>재생할 콘텐츠가 있는지</summary>
    public bool HasContent => cutscene != null || dialogue != null;
}
