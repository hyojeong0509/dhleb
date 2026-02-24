using UnityEngine;

/// <summary>
/// 컷신에서 NPC를 ID로 참조하기 위한 컴포넌트.
/// 행패 NPC 등 컷신에 등장하는 NPC에 부착하고 npcId를 설정.
/// </summary>
public class CutsceneNpcRef : MonoBehaviour
{
    [Tooltip("컷신에서 참조할 고유 ID (예: harass_1, harass_2)")]
    public string npcId;
}
