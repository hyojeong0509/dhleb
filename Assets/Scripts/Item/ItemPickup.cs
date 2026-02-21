using UnityEngine;

/// <summary>
/// 씬에 배치된 아이템 오브젝트.
/// - 플레이어가 일정 거리 안에 오면 자동으로 끌어당겨져서 인벤토리에 추가
/// - 버려진 아이템으로도 사용 (SetupVisual로 아이콘 설정)
/// </summary>
public class ItemPickup : MonoBehaviour
{
    [Header("아이템 설정")]
    public ItemData itemData;
    public int count = 1;

    [Header("자동 픽업 설정")]
    public float attractRange = 3f;   // 이 거리 안에 들어오면 끌려오기 시작
    public float pickupRange = 0.3f;  // 이 거리 안에 들어오면 인벤토리에 추가
    public float attractSpeed = 5f;   // 끌려오는 속도
    public float pickupDelay = 1f;    // 생성 후 이 시간 동안 픽업 안 됨 (버리기 직후 즉시 줍힘 방지)

    private Transform playerTransform;
    private bool isAttracting = false;
    private SpriteRenderer sr;
    private float spawnTime;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        SetupVisual();
        spawnTime = Time.time;

        // 씬에서 Player 태그로 플레이어 탐색
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    void Update()
    {
        if (playerTransform == null || itemData == null) return;

        // 생성 직후 딜레이 동안 픽업 비활성화
        if (Time.time - spawnTime < pickupDelay) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        // 끌어당기기 범위 진입
        if (dist <= attractRange)
            isAttracting = true;

        if (isAttracting)
        {
            // 플레이어 방향으로 이동
            transform.position = Vector2.MoveTowards(
                transform.position,
                playerTransform.position,
                attractSpeed * Time.deltaTime
            );

            // 픽업 범위 진입 시 인벤토리에 추가
            if (dist <= pickupRange)
                TryPickup();
        }
    }

    void TryPickup()
    {
        if (InventoryManager.Instance == null) return;

        bool added = InventoryManager.Instance.AddItem(itemData, count);
        if (added)
        {
            Debug.Log($"[ItemPickup] {itemData.itemName} x{count} 획득!");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("[ItemPickup] 인벤토리가 가득 찼습니다.");
            isAttracting = false; // 가득 차면 끌어당기기 중단
        }
    }

    /// <summary>
    /// 버려진 아이템으로 생성될 때 아이콘 설정
    /// </summary>
    public void SetupVisual()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr != null && itemData != null && itemData.icon != null)
            sr.sprite = itemData.icon;
    }

    // 범위 시각화 (에디터 전용)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attractRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}
