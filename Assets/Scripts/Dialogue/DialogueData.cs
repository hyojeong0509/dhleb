using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 대화 선택지 (선택지가 없으면 자동으로 nextNodeId로 진행)
/// </summary>
[System.Serializable]
public class DialogueChoice
{
    public string text;       // 선택지 텍스트
    public string nextNodeId; // 선택 시 이동할 노드 ID
}

/// <summary>
/// 대화 노드 (한 줄의 대사)
/// </summary>
[System.Serializable]
public class DialogueNode
{
    public string nodeId;          // 고유 ID (예: "triones_01")
    public string speakerName;     // 발화자 이름 (빈 문자열 = 나레이션, 초상화 로드에도 사용)
    [Tooltip("체크 시 화면에 ??? 표시 (초상화는 speakerName으로 로드)")]
    public bool hideSpeakerName;
    [TextArea(2, 5)]
    public string text;           // 대사 내용
    public string nextNodeId;     // 다음 노드 (비어있으면 대화 종료)
    public List<DialogueChoice> choices; // 선택지 (있으면 nextNodeId 무시, 선택 시 해당 nextNodeId로)

    public bool HasChoices => choices != null && choices.Count > 0;
}

/// <summary>
/// 대화 데이터 (ScriptableObject)
/// Project 창에서 우클릭 → Create → Dialogue → Dialogue Data
/// </summary>
[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Tooltip("시작 노드 ID")]
    public string startNodeId = "start";

    [Tooltip("대화 노드 목록")]
    public List<DialogueNode> nodes = new List<DialogueNode>();

    /// <summary>nodeId로 노드 찾기</summary>
    public DialogueNode GetNode(string nodeId)
    {
        if (string.IsNullOrEmpty(nodeId)) return null;
        foreach (var n in nodes)
            if (n.nodeId == nodeId) return n;
        return null;
    }
}
