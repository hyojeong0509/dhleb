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
        UpdateHighlight();

        if (Input.GetMouseButtonDown(0))
            TryUseTool();
    }

    /// <summary>
    /// 매 프레임 하이라이트 위치와 색상 갱신
    /// </summary>
    void UpdateHighlight()
    {
        if (TileHighlight.Instance == null) return;

        ToolData tool = GetEquippedTool();
        if (tool == null)
        {
            TileHighlight.Instance.Hide();
            return;
        }

        Vector3 mousePos = GetMouseWorldPosition();
        bool isValid = IsInRange(mousePos);
        TileHighlight.Instance.Show(mousePos, isValid);
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

        Debug.Log($"[PlayerAction] 도구: {tool.toolType} / 타겟 위치: {targetPos}");

        switch (tool.toolType)
        {
            case ToolType.Hoe:
                UsHoe(targetPos);
                break;
            case ToolType.WateringCan:
                UseWateringCan(targetPos);
                break;
            default:
                Debug.Log($"[PlayerAction] {tool.toolType} 는 아직 구현되지 않았습니다.");
                break;
        }
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

    void UsHoe(Vector3 targetPos)
    {
        if (TileManager.Instance == null) return;
        TileManager.Instance.TryTill(targetPos);
    }

    void UseWateringCan(Vector3 targetPos)
    {
        if (TileManager.Instance == null) return;
        TileManager.Instance.TryWater(targetPos);
    }
}
