using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 꿈 시퀀스 목록. 위에서부터 순서대로 검사해 첫 번째 조건 만족하는 꿈을 재생.
/// </summary>
[CreateAssetMenu(fileName = "DreamSequenceRegistry", menuName = "Dream/Dream Sequence Registry")]
public class DreamSequenceRegistry : ScriptableObject
{
    [Tooltip("꿈 시퀀스 목록 (위가 우선)")]
    public List<DreamSequenceData> dreams = new List<DreamSequenceData>();

    /// <summary>
    /// 조건 만족하는 첫 번째 꿈 반환. 없으면 null.
    /// </summary>
    public DreamSequenceData GetDreamToPlay(int dayBeforeSleep)
    {
        if (dreams == null) return null;

        foreach (var dream in dreams)
        {
            if (dream == null) continue;
            if (dream.AreConditionsMet(dayBeforeSleep))
                return dream;
        }
        return null;
    }
}
