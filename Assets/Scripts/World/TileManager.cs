using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 타일맵 변경 관리 (괭이로 땅 갈기, 물 주기 등)
/// </summary>
public class TileManager : MonoBehaviour
{
    public static TileManager Instance { get; private set; }

    [Header("타일맵 레퍼런스")]
    public Tilemap groundTilemap;   // Ground_0 (원본 바닥, 읽기만 사용)
    public Tilemap farmTilemap;     // Farm_0.5 (밭 레이어, 여기에 쓰기)

    [Header("밭 타일")]
    public TileBase tilledSoilTile;     // 갈린 흙 타일
    public TileBase wateredSoilTile;    // 물 준 흙 타일 (향후 사용)

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 괭이 사용 - 해당 위치를 밭으로 만들기
    /// </summary>
    public bool TryTill(Vector3 worldPosition)
    {
        Vector3Int cellPos = farmTilemap.WorldToCell(worldPosition);

        // 이미 갈린 흙이면 스킵
        if (farmTilemap.GetTile(cellPos) == tilledSoilTile)
        {
            Debug.Log("[TileManager] 이미 갈린 흙입니다.");
            return false;
        }

        // Ground 레이어에 타일이 있어야 경작 가능
        if (groundTilemap.GetTile(cellPos) == null)
        {
            Debug.Log("[TileManager] 경작 불가능한 위치입니다.");
            return false;
        }

        // 밭 레이어에 갈린 흙 배치
        farmTilemap.SetTile(cellPos, tilledSoilTile);
        Debug.Log($"[TileManager] 땅을 갈았습니다: {cellPos}");
        return true;
    }

    /// <summary>
    /// 물뿌리개 사용 - 갈린 흙에 물 주기 (향후 사용)
    /// </summary>
    public bool TryWater(Vector3 worldPosition)
    {
        Vector3Int cellPos = farmTilemap.WorldToCell(worldPosition);

        // 갈린 흙이어야 물 주기 가능
        if (farmTilemap.GetTile(cellPos) != tilledSoilTile)
        {
            Debug.Log("[TileManager] 갈린 흙이 아닙니다.");
            return false;
        }

        farmTilemap.SetTile(cellPos, wateredSoilTile);
        Debug.Log($"[TileManager] 물을 주었습니다: {cellPos}");
        return true;
    }

    /// <summary>
    /// 해당 위치가 갈린 흙인지 확인 (씨앗 심기 등에서 사용)
    /// </summary>
    public bool IsTilled(Vector3 worldPosition)
    {
        Vector3Int cellPos = farmTilemap.WorldToCell(worldPosition);
        TileBase tile = farmTilemap.GetTile(cellPos);
        return tile == tilledSoilTile || tile == wateredSoilTile;
    }
}
