using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 컷신 데이터. 액션 리스트 순차 실행.
/// Create → Cutscene → Cutscene Data
/// </summary>
[CreateAssetMenu(fileName = "NewCutscene", menuName = "Cutscene/Cutscene Data")]
public class CutsceneData : ScriptableObject
{
    [Tooltip("실행할 액션 순서")]
    public List<CutsceneAction> actions = new List<CutsceneAction>();
}
