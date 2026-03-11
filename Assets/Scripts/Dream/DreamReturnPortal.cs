using UnityEngine;

/// <summary>
/// 꿈맵 포탈. 플레이어가 F키(또는 클릭)로 상호작용하면:
///   1. returnDialogue 재생 (독백 + "돌아갈까요?" 선택지)
///   2. "네" 선택 → 페이드 아웃 → 복귀 좌표 텔레포트 → 페이드 인 → 플래그 설정
///   3. "아니오" 선택 → 대화 닫힘, 포탈 재상호작용 가능
///
/// DialogueData 설정:
///   - 마지막 선택 노드에서 "네" 선택지의 Next Node Id = "yes" (YES_NODE_ID와 일치)
///   - "아니오" 선택지의 Next Node Id = (빈칸)
///   - "yes" 노드는 별도로 추가하지 않아도 됨
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DreamReturnPortal : MonoBehaviour
{
    // "네" 선택지의 nextNodeId. DialogueData의 "네" 선택지 Next Node Id와 반드시 일치.
    public const string YES_NODE_ID = "yes";

    [Header("상호작용")]
    public string playerTag = "Player";
    public float interactRange = 2f;
    public KeyCode interactKey = KeyCode.F;

    [Header("귀환 대화")]
    [Tooltip("독백 + '돌아갈까요?' Yes/No 선택지가 담긴 DialogueData.")]
    public DialogueData returnDialogue;

    [Header("귀환 설정")]
    [Tooltip("플레이어가 복귀할 좌표 (침대 위치)")]
    public Vector3 returnPosition;
    [Tooltip("귀환 완료 후 설정할 플래그. 예: saw_dream_1")]
    public string setFlagWhenDone;
    [Tooltip("'네' 선택 후 페이드 아웃 시간 (초)")]
    public float fadeOutDuration = 0.4f;

    [Header("BedInteract 연결")]
    [Tooltip("저장/패널 호출용. 비우면 씬에서 자동 검색.")]
    public BedInteract bedInteract;

    private bool isInteracting;
    private bool returnChosen;

    void Update()
    {
        if (isInteracting) return;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying) return;
        if (CutsceneManager.Instance != null && CutsceneManager.Instance.IsPlaying) return;

        if (Input.GetKeyDown(interactKey) && IsPlayerInRange())
            TryInteract();

        if (Input.GetMouseButtonDown(0) && IsMouseOver() && IsPlayerInRange())
            TryInteract();
    }

    public void TryInteract()
    {
        if (isInteracting) return;
        if (returnDialogue == null || DialogueManager.Instance == null) return;

        isInteracting = true;
        returnChosen = false;

        // Advance 호출 시 nextNodeId를 감지 → "yes"면 귀환으로 처리
        DialogueManager.Instance.OnAdvancing += OnAdvancing;
        DialogueManager.Instance.Play(returnDialogue, OnReturnDialogueEnd);
    }

    void OnAdvancing(string nextNodeId)
    {
        if (nextNodeId == YES_NODE_ID)
            returnChosen = true;
    }

    void OnReturnDialogueEnd()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.OnAdvancing -= OnAdvancing;

        if (returnChosen)
            DoReturn();
        else
            isInteracting = false;
    }

    void DoReturn()
    {
        if (fadeOutDuration > 0f && ScreenFadeManager.Instance != null)
        {
            // 페이드 아웃 → 검은 화면에서 플레이어 이동 + 저장 패널 활성화
            // 페이드 인은 [다음] 버튼 클릭 시 BedInteract에서 처리
            ScreenFadeManager.Instance.FadeOut(fadeOutDuration, () =>
            {
                TeleportPlayerBack();
                OnReturnComplete();
            });
        }
        else
        {
            TeleportPlayerBack();
            OnReturnComplete();
        }
    }

    void TeleportPlayerBack()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Vector3 pos = returnPosition;
        pos.z = player.transform.position.z;

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.MovePosition(pos);
        else player.transform.position = pos;
    }

    void OnReturnComplete()
    {
        // 플래그 설정
        if (!string.IsNullOrEmpty(setFlagWhenDone) && GameProgressManager.Instance != null)
            GameProgressManager.Instance.SetFlag(setFlagWhenDone);

        isInteracting = false;

        // BedInteract에 귀환 완료 알림 → 저장 + 저장 완료 패널 표시
        var bed = bedInteract != null ? bedInteract : FindObjectOfType<BedInteract>();
        if (bed != null)
            bed.OnDreamReturnComplete();
    }

    public bool IsPlayerInRange()
    {
        var player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.transform.position) <= interactRange;
    }

    bool IsMouseOver()
    {
        if (Camera.main == null) return false;
        var col = GetComponent<Collider2D>();
        if (col == null) return false;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        return col.OverlapPoint(mouseWorld);
    }
}
