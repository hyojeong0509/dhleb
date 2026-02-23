using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 등록할 퀘스트 목록. Resources/QuestRegistry 에 두거나 QuestManager에 할당.
/// </summary>
[CreateAssetMenu(fileName = "QuestRegistry", menuName = "Quest/Quest Registry")]
public class QuestRegistry : ScriptableObject
{
    public List<QuestData> quests = new List<QuestData>();
}
