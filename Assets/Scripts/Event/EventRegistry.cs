using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 등록할 이벤트 목록. Resources/EventRegistry 에 두거나 씬의 EventManager에 할당.
/// </summary>
[CreateAssetMenu(fileName = "EventRegistry", menuName = "Event/Event Registry")]
public class EventRegistry : ScriptableObject
{
    public List<EventData> events = new List<EventData>();
}
