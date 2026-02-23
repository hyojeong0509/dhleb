using UnityEngine;

/// <summary>
/// NPC 기본 데이터 (ScriptableObject)
/// Project 창에서 우클릭 → Create → NPC → NPC Data
/// </summary>
[CreateAssetMenu(fileName = "NewNPC", menuName = "NPC/NPC Data")]
public class NPCData : ScriptableObject
{
    [Tooltip("고유 ID (호감도, 이벤트에서 사용)")]
    public string npcId;

    [Tooltip("표시 이름")]
    public string displayName;

    [Tooltip("기본 대화 (조건 없을 때)")]
    public DialogueData defaultDialogue;
}
