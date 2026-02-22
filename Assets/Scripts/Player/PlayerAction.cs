using UnityEngine;

/// <summary>
/// 플레이어 도구 사용 처리
/// - 좌클릭: 핫바의 현재 도구 사용
/// - 마우스 위치 기준으로 타일 선택
/// </summary>
public class PlayerAction : MonoBehaviour
{
    [Header("도구 설정")]
    public float maxUseDistance = 2.5f; // 최대 사용 거리 (타일 단위)
    public float highlightViewRange = 5f; // 하이라이트 표시 범위 (이 거리 밖은 표시 안 함)

    private PlayerMovement movement;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    void Start()
    {
        // TileHighlight에 타일맵 참조 전달
        if (TileHighlight.Instance != null && TileManager.Instance != null)
            TileHighlight.Instance.Init(TileManager.Instance.farmTilemap);
    }

    void Update()
    {
        // 인벤토리 열려있으면 하이라이트 숨기고 모든 게임 액션 차단
        if (InventoryManager.Instance != null && InventoryManager.Instance.IsInventoryOpen)
        {
            TileHighlight.Instance?.Hide();
            return;
        }

        // ESC 메뉴, 침대 팝업 등 UI 패널 열려있으면 입력 차단
        if (GameInputBlocker.IsBlocked)
        {
            TileHighlight.Instance?.Hide();
            return;
        }

        UpdateHighlight();

        if (Input.GetMouseButtonDown(0))
        {
            if (GetEquippedSeed() != null)
                TryPlantSeed();
            else
                TryUseTool();
        }

        // 우클릭 - 수확
        if (Input.GetMouseButtonDown(1))
            TryHarvest();
    }

    /// <summary>
    /// 매 프레임 하이라이트 위치와 색상 갱신
    /// </summary>
    void UpdateHighlight()
    {
        if (TileHighlight.Instance == null) return;

        Vector3 mousePos = GetMouseWorldPosition();

        // 플레이어 중심 5칸 밖이면 하이라이트 숨김
        if (Vector2.Distance(transform.position, mousePos) > highlightViewRange)
        {
            TileHighlight.Instance.Hide();
            return;
        }

        // 씨앗을 들고 있을 때 - 갈린 흙/물 준 흙에서만 표시
        SeedData seed = GetEquippedSeed();
        if (seed != null)
        {
            bool isPlantable = IsInRange(mousePos)
                && (TileManager.Instance.IsTilled(mousePos) || TileManager.Instance.IsWatered(mousePos))
                && !TileManager.Instance.HasCrop(mousePos);

            if (isPlantable)
                TileHighlight.Instance.Show(mousePos, true);
            else
                TileHighlight.Instance.Hide();
            return;
        }

        // 도구를 들고 있을 때 - 범위 내 초록, 범위 밖/탈진 빨강 (집 안에서는 표시 안 함)
        ToolData tool = GetEquippedTool();
        if (tool != null)
        {
            if (IndoorArea.IsPlayerIndoor && (tool.toolType == ToolType.Hoe || tool.toolType == ToolType.WateringCan))
            {
                TileHighlight.Instance.Hide();
            }
            else
            {
                bool inRange = IsInRange(mousePos);
                bool hasStamina = StaminaManager.Instance == null || StaminaManager.Instance.CanUseStamina(1);
                bool isValid = inRange && hasStamina;
                TileHighlight.Instance.Show(mousePos, isValid);
            }
            return;
        }

        // 빈 손 - 다 자란 작물 위에서만 표시
        if (TileManager.Instance != null && TileManager.Instance.IsFullyGrown(mousePos) && IsInRange(mousePos))
        {
            TileHighlight.Instance.Show(mousePos, true);
            return;
        }

        TileHighlight.Instance.Hide();
    }

    void TryUseTool()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[PlayerAction] InventoryManager 없음!");
            return;
        }

        ToolData tool = GetEquippedTool();
        if (tool == null) return;

        if (TileManager.Instance == null)
        {
            Debug.LogWarning("[PlayerAction] TileManager 없음!");
            return;
        }

        Vector3 targetPos = GetMouseWorldPosition();

        // 거리 체크
        if (!IsInRange(targetPos))
        {
            Debug.Log("[PlayerAction] 너무 멀어서 사용 불가");
            return;
        }

        // 스테미나 체크 (괭이, 물뿌리개 - 사전 검사)
        if ((tool.toolType == ToolType.Hoe || tool.toolType == ToolType.WateringCan)
            && (StaminaManager.Instance == null || !StaminaManager.Instance.CanUseStamina(1)))
        {
            Debug.Log("[PlayerAction] 스테미나가 부족합니다.");
            return;
        }

        Debug.Log($"[PlayerAction] 도구: {tool.toolType} / 타겟 위치: {targetPos}");

        bool consumed = false;
        switch (tool.toolType)
        {
            case ToolType.Hoe:
                consumed = UsHoe(targetPos);
                break;
            case ToolType.WateringCan:
                consumed = UseWateringCan(targetPos);
                break;
            default:
                Debug.Log($"[PlayerAction] {tool.toolType} 는 아직 구현되지 않았습니다.");
                break;
        }

        // 괭이/물뿌리개 사용 성공 시에만 스테미나 소모
        if (consumed && (tool.toolType == ToolType.Hoe || tool.toolType == ToolType.WateringCan))
            StaminaManager.Instance?.UseStamina(1);
    }

    /// <summary>
    /// 씨앗 심기
    /// </summary>
    void TryPlantSeed()
    {
        SeedData seed = GetEquippedSeed();
        if (seed == null) return;
        if (TileManager.Instance == null || TimeManager.Instance == null) return;

        Vector3 targetPos = GetMouseWorldPosition();
        if (!IsInRange(targetPos)) return;

        bool planted = TileManager.Instance.TryPlantSeed(targetPos, seed, TimeManager.Instance.CurrentDay);
        if (planted)
        {
            // 인벤토리에서 씨앗 1개 소모
            InventorySlotData slotData = InventoryManager.Instance.GetActiveSlotData();
            slotData.count--;
            if (slotData.count <= 0) slotData.Clear();
            InventoryManager.Instance.RefreshHotbarUI();
        }
    }

    /// <summary>
    /// 수확 (우클릭) - 작물 있는 땅에서만 시도
    /// </summary>
    void TryHarvest()
    {
        if (TileManager.Instance == null) return;
        Vector3 targetPos = GetMouseWorldPosition();
        if (!IsInRange(targetPos)) return;
        if (!TileManager.Instance.HasCrop(targetPos)) return; // 작물 없으면 아무것도 안 함
        TileManager.Instance.TryHarvest(targetPos);
    }

    /// <summary>
    /// 현재 핫바에 장착된 도구 반환 (없으면 null)
    /// </summary>
    ToolData GetEquippedTool()
    {
        InventorySlotData slotData = InventoryManager.Instance?.GetActiveSlotData();
        if (slotData == null || slotData.IsEmpty) return null;
        return slotData.item as ToolData;
    }

    /// <summary>
    /// 현재 핫바에 장착된 씨앗 반환 (없으면 null)
    /// </summary>
    SeedData GetEquippedSeed()
    {
        InventorySlotData slotData = InventoryManager.Instance?.GetActiveSlotData();
        if (slotData == null || slotData.IsEmpty) return null;
        return slotData.item as SeedData;
    }

    /// <summary>
    /// 마우스가 가리키는 타일의 중심 좌표 반환 (타일 격자에 스냅)
    /// </summary>
    Vector3 GetMouseWorldPosition()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        // 타일 격자에 스냅된 중심 좌표로 변환
        if (TileManager.Instance != null)
        {
            var tilemap = TileManager.Instance.farmTilemap;
            Vector3Int cell = tilemap.WorldToCell(mouseWorld);
            return tilemap.GetCellCenterWorld(cell);
        }

        return mouseWorld;
    }

    /// <summary>
    /// 플레이어와 대상 위치 사이의 거리가 범위 내인지 확인
    /// </summary>
    bool IsInRange(Vector3 targetPos)
    {
        return Vector2.Distance(transform.position, targetPos) <= maxUseDistance;
    }

    bool UsHoe(Vector3 targetPos)
    {
        if (TileManager.Instance == null) return false;
        return TileManager.Instance.TryTill(targetPos);
    }

    bool UseWateringCan(Vector3 targetPos)
    {
        if (TileManager.Instance == null) return false;
        return TileManager.Instance.TryWater(targetPos);
    }
}
