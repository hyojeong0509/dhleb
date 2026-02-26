using UnityEngine;

// NPC 상호작용.
// 플레이어가 범위 내에서 클릭하거나 F키로 대화 시작.

[RequireComponent(typeof(Collider2D))]
public class NPCInteract : MonoBehaviour
{
    [Header("NPC 데이터")]
    public NPCData npcData;

    [Header("상호작용 설정")]
    [Tooltip("상호작용 가능 거리 (플레이어 ~ NPC)")]
    public float interactRange = 2f;

    [Tooltip("상호작용 키 (기본 F)")]
    public KeyCode interactKey = KeyCode.F;

    [Tooltip("대화 시작 시 비활성화할 오브젝트 (가이드 이미지 등)")]
    public GameObject deactivateOnInteract;

    private Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
            col.isTrigger = true;
    }

    void Update()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying)
            return;
        if (CutsceneManager.Instance != null && CutsceneManager.Instance.IsPlaying)
            return;

        if (Input.GetKeyDown(interactKey) && IsPlayerInRange())
            TryInteract();
    }

    // 상호작용 시도
    public void TryInteract()
    {
        if (!IsPlayerInRange()) return;
        Interact();
    }

    // 대화 시작. EventManager에서 조건별 대화 먼저 확인, 없으면 defaultDialogue
    public void Interact()
    {
        if (DialogueManager.Instance == null) return;

        DialogueData dialogue = null;
        if (EventManager.Instance != null && npcData != null && !string.IsNullOrEmpty(npcData.npcId))
            dialogue = EventManager.Instance.GetDialogueForNPC(npcData.npcId);

        if (dialogue == null && npcData != null)
            dialogue = npcData.defaultDialogue;

        if (dialogue == null)
        {
            Debug.LogWarning($"[NPCInteract] {gameObject.name}: 대화 데이터 없음");
            return;
        }

        if (deactivateOnInteract != null)
            deactivateOnInteract.SetActive(false);

        string npcId = npcData != null ? npcData.npcId : "";
        string npcDisplayName = (npcData != null && !string.IsNullOrEmpty(npcData.displayName))
            ? npcData.displayName : gameObject.name;
        DialogueManager.Instance.Play(dialogue, () =>
        {
            if (!string.IsNullOrEmpty(npcId))
                QuestManager.Instance?.NotifyObjectiveProgress(QuestObjectiveType.TalkToNpc, npcId, 1);
        }, npcDisplayName, npcId);
    }

    public bool IsPlayerInRange()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.transform.position) <= interactRange;
    }

    // 마우스가 이 NPC 위에 있는지
    public bool IsMouseOver()
    {
        if (col == null || Camera.main == null) return false;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        return col.OverlapPoint(mouseWorld);
    }
}
