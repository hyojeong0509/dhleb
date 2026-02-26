using UnityEngine;
using System.Collections.Generic;

// 퀘스트/스토리/호감도에 따라 대화, 이벤트 분기.
// NPC 상호작용 시 GetDialogueForNPC 호출 → 조건 만족하는 이벤트의 대화 반환.
public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    [Tooltip("등록된 이벤트 (비우면 Resources에서 EventRegistry 로드)")]
    public List<EventData> events = new List<EventData>();

    [Tooltip("또는 EventRegistry 에셋 직접 지정")]
    public EventRegistry registry;

    // 대화 종료 시 아이템 지급용 (대화가 끝나면 GiveItemsOnDialogueEnd 호출)
    private EventData pendingGiveItemsEvent;
    private DialogueData pendingGiveItemsDialogue;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (events.Count == 0 && registry != null && registry.events != null)
            events.AddRange(registry.events);
        if (events.Count == 0)
        {
            var reg = Resources.Load<EventRegistry>("EventRegistry");
            if (reg != null && reg.events != null)
                events.AddRange(reg.events);
        }

        StartCoroutine(SubscribeWhenReady());
    }

    System.Collections.IEnumerator SubscribeWhenReady()
    {
        while (DialogueManager.Instance == null)
            yield return null;
        DialogueManager.Instance.OnDialogueEnded += OnDialogueEnded;
    }

    void OnDestroy()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.OnDialogueEnded -= OnDialogueEnded;
    }

    void OnDialogueEnded(DialogueData data, bool completed)
    {
        if (pendingGiveItemsEvent == null || pendingGiveItemsDialogue != data) return;
        if (!completed)
        {
            pendingGiveItemsEvent = null;
            pendingGiveItemsDialogue = null;
            return; // 닫기 버튼 등으로 중단 시 아이템/퀘스트 지급 안 함
        }
        pendingGiveItemsEvent.MarkAsDone(); // 대화 완료 시에만 플래그/스토리 설정
        pendingGiveItemsEvent.OnDialogueEnded();
        pendingGiveItemsEvent = null;
        pendingGiveItemsDialogue = null;
    }

    // NPC와 상호작용 시 재생할 대화 반환.
    // 조건 만족하는 이벤트가 있으면 해당 대화, 없으면 null (호출측에서 defaultDialogue 사용)
    public DialogueData GetDialogueForNPC(string npcId)
    {
        if (string.IsNullOrEmpty(npcId) || events == null) return null;

        pendingGiveItemsEvent = null;
        pendingGiveItemsDialogue = null;

        foreach (var evt in events)
        {
            if (evt == null || evt.dialogue == null) continue;
            if (evt.npcId != npcId) continue;
            if (!evt.AreConditionsMet()) continue;

            // giveItemsOnDone 있으면 대화 완료 시 MarkAsDone (닫기 후 재시도 가능)
            if (!evt.HasGiveItemsOnDone)
                evt.MarkAsDone();
            if (evt.HasGiveItemsOnDone)
            {
                pendingGiveItemsEvent = evt;
                pendingGiveItemsDialogue = evt.dialogue;
            }
            return evt.dialogue;
        }
        return null;
    }
}
