using UnityEngine;

/// <summary>
/// 씬에 배치된 아이템 오브젝트.
/// - 플레이어가 일정 거리 안에 오면 자동으로 끌어당겨져서 인벤토리에 추가
/// - 버려진 아이템으로도 사용 (SetupVisual로 아이콘 설정)
/// - 스타듀밸리처럼 둥둥 떠다니는 애니메이션
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

    [Header("둥둥 떠다니는 효과")]
    public float bobHeight = 0.15f;   // 위아래 움직임 높이
    public float bobSpeed = 1.5f;     // 움직임 속도

    [Header("스폰 시 뿅~ 효과")]
    public float popDuration = 0.2f;  // 0→1 스케일 애니메이션 시간

    [Header("포물선 스폰")]
    public float arcCellRadius = 1f;   // 주변 N셀(유닛) 안에서 랜덤 착지
    public float arcDuration = 0.35f;  // 포물선 비행 시간
    public float arcHeight = 0.4f;     // 포물선 최대 높이

    private Transform playerTransform;
    private bool isAttracting = false;
    private SpriteRenderer sr;
    private float spawnTime;
    private Vector3 basePosition;     // 둥둥 애니메이션 기준 위치
    private Vector3 arcOrigin;        // 포물선 시작점 (캔한 자리)
    private Vector3 arcDestination;   // 포물선 착지점
    private bool useArcSpawn;         // 포물선 스폰 사용 여부

    /// <summary>캔/수확한 자리에서 아이템 스폰 - 주변 1셀 내 포물선으로 뿅~</summary>
    public static bool SpawnAt(ItemData item, int count, Vector3 position)
    {
        var inv = InventoryManager.Instance;
        if (inv == null || inv.droppedItemPrefab == null || item == null) return false;
        var go = Object.Instantiate(inv.droppedItemPrefab, position, Quaternion.identity);
        var pickup = go.GetComponent<ItemPickup>();
        if (pickup != null)
        {
            pickup.itemData = item;
            pickup.count = count;
            pickup.SetupVisual();
            pickup.SetArcSpawn(position);
        }
        return true;
    }

    /// <summary>포물선 스폰 설정 - 캔한 자리에서 주변 1셀 내로 뿅~</summary>
    public void SetArcSpawn(Vector3 origin)
    {
        useArcSpawn = true;
        arcOrigin = origin;
        arcDestination = origin + (Vector3)(Random.insideUnitCircle * arcCellRadius);
        basePosition = arcDestination;
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        SetupVisual();
        spawnTime = Time.time;
        if (!useArcSpawn)
            basePosition = transform.position;
        transform.localScale = Vector3.zero; // 뿅~ 효과: 처음엔 0에서 시작

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    void Update()
    {
        if (itemData == null) return;

        float elapsed = Time.time - spawnTime;

        // 스폰 시 뿅~ 스케일 효과 (0 → 1)
        if (elapsed < popDuration && popDuration > 0f)
        {
            float t = elapsed / popDuration;
            t = 1f - (1f - t) * (1f - t); // ease-out
            transform.localScale = Vector3.one * t;
        }
        else
        {
            transform.localScale = Vector3.one;
        }

        // 포물선 비행: 캔한 자리 → 주변 1셀 내 착지
        if (useArcSpawn && elapsed < arcDuration && arcDuration > 0f)
        {
            float t = elapsed / arcDuration;
            t = 1f - (1f - t) * (1f - t); // ease-out
            float parabola = 4f * t * (1f - t); // 0→1→0 (t=0.5에서 최고점)
            transform.position = Vector3.Lerp(arcOrigin, arcDestination, t) + Vector3.up * (arcHeight * parabola);
            return;
        }
        if (useArcSpawn && elapsed >= arcDuration)
        {
            useArcSpawn = false;
            transform.position = arcDestination;
        }

        if (Time.time - spawnTime < pickupDelay) return;

        if (playerTransform == null)
        {
            Bobbing();
            return;
        }

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        if (dist <= attractRange)
            isAttracting = true;

        if (isAttracting)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                playerTransform.position,
                attractSpeed * Time.deltaTime
            );
            basePosition = transform.position;

            if (dist <= pickupRange)
                TryPickup();
        }
        else
        {
            Bobbing();
        }
    }

    void Bobbing()
    {
        float yOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = basePosition + Vector3.up * yOffset;
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
