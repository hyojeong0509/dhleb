using UnityEngine;

/// <summary>
/// NPC가 시작 위치 기준 1칸 반경 안에서 자유롭게 배회
/// Animator: Speed (0=Idle, 1=Walk), Walk 애니메이션은 오른쪽 방향 기준 → flipX로 좌우 전환
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class NPCWander : MonoBehaviour
{
    [Header("배회 설정")]
    [Tooltip("체크 시 배회, 해제 시 제자리에 고정")]
    public bool enableWandering = true;

    [Tooltip("시작 위치 기준 반경 (1 = 1타일)")]
    public float wanderRadius = 1f;

    [Tooltip("이동 속도")]
    public float moveSpeed = 1.5f;

    [Tooltip("한 곳 도착 후 대기 시간 (최소~최대 초)")]
    public Vector2 idleTimeRange = new Vector2(2f, 5f);

    [Header("참조")]
    [Tooltip("벽 등 장애물 레이어")]
    public LayerMask collisionMask;

    [Tooltip("NPC 레이어 (겹침 방지용, 비어있으면 NPCWander 있는 오브젝트만 검사)")]
    public LayerMask npcLayer;

    [Tooltip("플레이어 레이어 (겹침 방지, 비우면 무시)")]
    public LayerMask playerLayer;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;
    private ContactFilter2D contactFilter;
    private ContactFilter2D npcContactFilter;
    private ContactFilter2D playerContactFilter;

    private Vector2 homePosition;
    private Vector2 targetPosition;
    private float idleTimer;
    private bool isMoving;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        contactFilter.SetLayerMask(collisionMask);
        contactFilter.useLayerMask = collisionMask != 0;
        npcContactFilter.SetLayerMask(npcLayer);
        npcContactFilter.useLayerMask = npcLayer != 0;
        playerContactFilter.SetLayerMask(playerLayer);
        playerContactFilter.useLayerMask = playerLayer != 0;
    }

    void Start()
    {
        homePosition = transform.position;
        PickNewTarget();
    }

    void Update()
    {
        if (!enableWandering)
        {
            SetAnimSpeed(0f);
            return;
        }

        if (isMoving)
        {
            float dist = Vector2.Distance(transform.position, targetPosition);
            if (dist < 0.05f)
            {
                // 도착
                isMoving = false;
                idleTimer = Random.Range(idleTimeRange.x, idleTimeRange.y);
                SetAnimSpeed(0f);
            }
        }
        else
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f)
            {
                PickNewTarget();
                isMoving = true;
                SetAnimSpeed(1f);
            }
        }
    }

    void FixedUpdate()
    {
        if (!enableWandering || !isMoving) return;

        Vector2 dir = (targetPosition - (Vector2)transform.position).normalized;

        // flipX: Walk가 오른쪽 기준이므로, 왼쪽 갈 때 flip
        if (sr != null)
            sr.flipX = dir.x < 0;

        Vector2 move = dir * moveSpeed * Time.fixedDeltaTime;

        // 벽 + NPC 충돌 시 이동 안 함 (겹침 방지)
        if (HasCollision(move))
        {
            move = Vector2.zero;
            // 막혀있으면 Idle로 전환 (끼인 상태에서 Walk 계속 재생 방지)
            isMoving = false;
            idleTimer = Random.Range(idleTimeRange.x, idleTimeRange.y);
            SetAnimSpeed(0f);
        }

        if (move.sqrMagnitude > 0)
            rb.MovePosition(rb.position + move);
    }

    void PickNewTarget()
    {
        // 홈 기준 반경 내 랜덤 위치
        Vector2 offset = Random.insideUnitCircle * wanderRadius;
        targetPosition = homePosition + offset;
    }

    void SetAnimSpeed(float speed)
    {
        if (anim != null)
        {
            // Speed 파라미터가 있으면 사용 (Animator에 추가 필요)
            if (HasAnimParam("Speed"))
                anim.SetFloat("Speed", speed);
        }
    }

    bool HasAnimParam(string name)
    {
        foreach (var p in anim.parameters)
            if (p.name == name) return true;
        return false;
    }

    bool HasCollision(Vector2 move)
    {
        if (rb == null) return false;
        float dist = move.magnitude + 0.05f;
        Vector2 dir = move.normalized;
        RaycastHit2D[] hits = new RaycastHit2D[5];

        // 벽 등
        if (collisionMask != 0)
        {
            int c = rb.Cast(dir, contactFilter, hits, dist);
            if (c > 0) return true;
        }

        // NPC (겹침 방지)
        if (npcLayer != 0)
        {
            int c = rb.Cast(dir, npcContactFilter, hits, dist);
            for (int i = 0; i < c; i++)
            {
                var other = hits[i].collider.GetComponentInParent<NPCWander>();
                if (other != null && other != this) return true; // 다른 NPC면 충돌
            }
        }

        // 플레이어 (겹침 방지)
        if (playerLayer != 0)
        {
            int c = rb.Cast(dir, playerContactFilter, hits, dist);
            if (c > 0) return true;
        }

        if (npcLayer == 0)
        {
            // npcLayer 비어있으면 NPCWander 있는 오브젝트 수동 검사
            var allHits = Physics2D.RaycastAll(rb.position, dir, dist);
            var myCol = GetComponent<Collider2D>();
            foreach (var h in allHits)
            {
                if (h.collider == null || h.collider == myCol) continue;
                var other = h.collider.GetComponentInParent<NPCWander>();
                if (other != null && other != this) return true;
            }
        }

        return false;
    }

    void OnDrawGizmosSelected()
    {
        Vector2 home = Application.isPlaying ? homePosition : (Vector2)transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(home, wanderRadius);
    }
}
