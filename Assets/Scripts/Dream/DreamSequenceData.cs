using UnityEngine;

/// <summary>
/// 꿈 시퀀스 조건 + 재생 방식 설정.
/// - 맵 이동형: dreamEntryPosition을 설정 → 플레이어를 해당 좌표로 텔레포트
/// - 컷신형: cutscene 설정
/// - 대화형: dialogue 설정
/// </summary>
[CreateAssetMenu(fileName = "NewDreamSequence", menuName = "Dream/Dream Sequence Data")]
public class DreamSequenceData : ScriptableObject
{
    [Header("맵 이동형 꿈 (자유 탐색)")]
    [Tooltip("플레이어가 이동할 꿈맵 좌표. (0,0,0)이면 맵 이동 없음.")]
    public Vector3 dreamEntryPosition;

    [Header("컷신/대화형 꿈")]
    [Tooltip("재생할 컷신 (맵 이동형이 아닐 때 사용)")]
    public CutsceneData cutscene;
    [Tooltip("재생할 대화 (컷신도 없을 때 사용)")]
    public DialogueData dialogue;

    [Header("날짜 조건")]
    [Tooltip("최소 일차 (-1 = 무시). 예: 1이면 1일차 이상")]
    public int minDay = -1;
    [Tooltip("최대 일차 (-1 = 무시). 예: 1이면 1일차 이하")]
    public int maxDay = -1;

    [Header("확률")]
    [Tooltip("0~1. 1 = 무조건, 0.1 = 10%")]
    [Range(0f, 1f)]
    public float probability = 1f;

    [Header("선행 조건")]
    [Tooltip("이 꿈을 보려면 모두 있어야 하는 플래그. 예: saw_dream_1")]
    public string[] requiredFlags;

    [Header("한 번만 보기")]
    [Tooltip("재생 후 설정할 플래그 (중복 재생 방지). 맵 이동형은 DreamReturnPortal에서 설정.")]
    public string setFlagWhenDone;

    /// <summary>맵 이동형 꿈인지 (dreamEntryPosition이 설정되어 있으면 맵 이동형)</summary>
    public bool IsMapWalk => dreamEntryPosition != Vector3.zero;

    /// <summary>재생할 콘텐츠가 있는지</summary>
    public bool HasContent => IsMapWalk || cutscene != null || dialogue != null;

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
        if (!HasContent)
            return false;

        return true;
    }
}
