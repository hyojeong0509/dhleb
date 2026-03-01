using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

// 자연물(잔디, 나무, 돌) 생성/제거/저장/로드
// Game 씬에 빈 오브젝트로 배치하고 프리팹/영역 설정
public class NaturalObjectManager : MonoBehaviour
{
    public static NaturalObjectManager Instance { get; private set; }

    [Header("참조")]
    [Tooltip("Farm_0.5 (갈린 흙 타일맵). NaturalObjectManager 선택 시 Scene 뷰에 농장 영역이 녹색 박스로 표시됨")]
    public Tilemap referenceTilemap;

    [Header("농장 영역 (타일 셀 좌표)")]
    [Tooltip("자연물 스폰 시작 좌표. Farm_0.5 타일맵 기준 셀 좌표 (Inspector 우클릭 → 타일맵 범위에서 가져오기)")]
    public Vector3Int farmBoundsMin = new Vector3Int(-15, -15, 0);
    [Tooltip("자연물 스폰 끝 좌표")]
    public Vector3Int farmBoundsMax = new Vector3Int(15, 15, 0);
    [Tooltip("Ground_0에 타일이 있는 셀에서만 스폰. false면 영역 내 모든 셀에 스폰")]
    public bool requireGroundTile = true;

    [Header("프리팹")]
    public GameObject grassPrefab;
    public GameObject treePrefab;
    public GameObject stonePrefab;

    [Header("첫날 랜덤 배치")]
    [Range(0f, 1f)]
    public float grassSpawnChance = 0.4f;
    [Range(0f, 1f)]
    public float treeSpawnChance = 0.08f;
    [Range(0f, 1f)]
    public float stoneSpawnChance = 0.05f;
    [Tooltip("나무/돌 최소 간격 (칸)")]
    public int treeStoneMinSpacing = 2;
    [Tooltip("플레이어 스폰 주변 비울 반경 (칸)")]
    public int playerSpawnClearRadius = 3;

    private Dictionary<Vector3Int, NaturalObject> _objects = new Dictionary<Vector3Int, NaturalObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (referenceTilemap == null && TileManager.Instance != null)
            referenceTilemap = TileManager.Instance.farmTilemap;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Start()
    {
        if (_objects.Count == 0 && GetTilemap() != null)
        {
            SpawnInitialNaturalObjects();
        }
    }

    Tilemap GetTilemap()
    {
        if (referenceTilemap != null) return referenceTilemap;
        if (TileManager.Instance != null) return TileManager.Instance.farmTilemap;
        return null;
    }

    public Vector3Int WorldToCell(Vector3 worldPos)
    {
        var tm = GetTilemap();
        return tm != null ? tm.WorldToCell(worldPos) : Vector3Int.zero;
    }

    public Vector3 CellToWorld(Vector3Int cellPos)
    {
        var tm = GetTilemap();
        return tm != null ? tm.GetCellCenterWorld(cellPos) : (Vector3)cellPos;
    }

    /// <summary>
    /// 마우스 위치(월드)에 있는 자연물 반환.
    /// 나무: 등록 셀은 나무 중간, 밑둥은 그 아래 셀. 밑둥 셀 클릭 시에만 hit.
    /// 잔디/돌: 클릭한 셀에서만 hit.
    /// </summary>
    public NaturalObject GetNaturalObjectAt(Vector3 worldPos)
    {
        var cell = WorldToCell(worldPos);

        // 나무: 위쪽 셀(y+1)에 나무가 있으면 → 지금 클릭한 셀은 밑둥 → hit 허용
        var cellAbove = cell + new Vector3Int(0, 1, 0);
        if (_objects.TryGetValue(cellAbove, out var obj) && obj is TreeObject)
            return obj;

        // 잔디/돌: 클릭한 셀에서 hit
        if (_objects.TryGetValue(cell, out obj))
        {
            if (obj is TreeObject) return null; // 나무는 밑둥에서만 (위에서 처리)
            return obj;
        }
        return null;
    }

    public bool HasNaturalObjectAt(Vector3 worldPos)
    {
        return GetNaturalObjectAt(worldPos) != null;
    }

    public void Register(NaturalObject obj, Vector3Int cellPos)
    {
        _objects[cellPos] = obj;
    }

    public void RemoveNaturalObject(NaturalObject obj)
    {
        var cell = WorldToCell(obj.transform.position);
        _objects.Remove(cell);
    }

    public bool TryHitNaturalObject(Vector3 worldPos, ToolType toolType, out bool consumed)
    {
        consumed = false;
        var obj = GetNaturalObjectAt(worldPos);
        if (obj == null || !obj.CanHitWith(toolType)) return false;
        consumed = obj.OnHit(toolType);
        return consumed;
    }

    public void ExportToFarmData(FarmData data)
    {
        if (data == null) return;
        data.naturalObjects.Clear();
        foreach (var kvp in _objects)
        {
            if (kvp.Value == null) continue;
            data.naturalObjects.Add(kvp.Value.ToSaveData(kvp.Key));
        }
    }

    public void LoadFromFarmData(FarmData data)
    {
        if (data == null) return;

        foreach (var obj in _objects.Values)
        {
            if (obj != null) Destroy(obj.gameObject);
        }
        _objects.Clear();

        if (data.naturalObjects == null || data.naturalObjects.Count == 0)
        {
            SpawnInitialNaturalObjects();
            return;
        }

        var tm = GetTilemap();
        if (tm == null) return;

        foreach (var save in data.naturalObjects)
        {
            var cellPos = new Vector3Int(save.cellX, save.cellY, save.cellZ);
            GameObject prefab = save.objectType switch
            {
                "Grass" => grassPrefab,
                "Tree" => treePrefab,
                "Stone" => stonePrefab,
                _ => null
            };
            if (prefab == null) continue;

            var spawnPos = tm.GetCellCenterWorld(cellPos);
            var go = Instantiate(prefab, spawnPos, Quaternion.identity);
            var natural = go.GetComponent<NaturalObject>();
            if (natural == null) continue;

            _objects[cellPos] = natural;

            if (natural is TreeObject tree)
                tree.Initialize(true, save.hitsRemaining);
        }
    }

    void SpawnInitialNaturalObjects()
    {
        var tm = GetTilemap();
        if (tm == null)
        {
            Debug.LogWarning("[NaturalObjectManager] 타일맵이 없습니다. referenceTilemap 또는 TileManager.farmTilemap을 할당하세요.");
            return;
        }
        if (grassPrefab == null && treePrefab == null && stonePrefab == null)
        {
            Debug.LogWarning("[NaturalObjectManager] 프리팹이 할당되지 않았습니다. Inspector에서 grassPrefab, treePrefab, stonePrefab을 설정하세요.");
            return;
        }

        var player = GameObject.FindGameObjectWithTag("Player");
        var playerCell = player != null ? WorldToCell(player.transform.position) : Vector3Int.zero;

        var treeStoneCells = new HashSet<Vector3Int>();

        for (int x = farmBoundsMin.x; x <= farmBoundsMax.x; x++)
        {
            for (int y = farmBoundsMin.y; y <= farmBoundsMax.y; y++)
            {
                var cell = new Vector3Int(x, y, 0);
                if (Vector3Int.Distance(cell, playerCell) <= playerSpawnClearRadius)
                    continue;

                if (requireGroundTile)
                {
                    var groundTm = TileManager.Instance?.groundTilemap;
                    if (groundTm != null && groundTm.GetTile(cell) == null)
                    {
                        continue;
                    }
                }

                float r = Random.value;
                if (r < grassSpawnChance && grassPrefab != null)
                {
                    SpawnAtCell(grassPrefab, cell, null);
                }
                else if (r < grassSpawnChance + treeSpawnChance && treePrefab != null)
                {
                    if (IsFarEnoughFromTreeStone(cell, treeStoneCells))
                    {
                        SpawnAtCell(treePrefab, cell, go =>
                        {
                            var t = go.GetComponent<TreeObject>();
                            if (t != null) t.Initialize(false);
                        });
                        treeStoneCells.Add(cell);
                    }
                }
                else if (r < grassSpawnChance + treeSpawnChance + stoneSpawnChance && stonePrefab != null)
                {
                    if (IsFarEnoughFromTreeStone(cell, treeStoneCells))
                    {
                        SpawnAtCell(stonePrefab, cell, null);
                        treeStoneCells.Add(cell);
                    }
                }
            }
        }

        int totalSpawned = _objects.Count;
        Debug.Log($"[NaturalObjectManager] 첫날 자연물 스폰 완료: {totalSpawned}개 (영역: {farmBoundsMin}~{farmBoundsMax}, requireGroundTile={requireGroundTile})");
    }

    bool IsFarEnoughFromTreeStone(Vector3Int cell, HashSet<Vector3Int> existing)
    {
        foreach (var c in existing)
        {
            if (Vector3Int.Distance(cell, c) < treeStoneMinSpacing)
                return false;
        }
        return true;
    }

    void SpawnAtCell(GameObject prefab, Vector3Int cellPos, System.Action<GameObject> onSpawned)
    {
        if (_objects.ContainsKey(cellPos)) return;

        var tm = GetTilemap();
        var spawnPos = tm.GetCellCenterWorld(cellPos);
        var go = Instantiate(prefab, spawnPos, Quaternion.identity);
        var natural = go.GetComponent<NaturalObject>();
        if (natural != null)
        {
            _objects[cellPos] = natural;
            onSpawned?.Invoke(go);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        var tm = referenceTilemap != null ? referenceTilemap : (TileManager.Instance != null ? TileManager.Instance.farmTilemap : null);
        if (tm == null) return;

        Vector3 minWorld = tm.GetCellCenterWorld(farmBoundsMin);
        Vector3 maxWorld = tm.GetCellCenterWorld(farmBoundsMax);
        Vector3 size = maxWorld - minWorld + (Vector3)tm.cellSize;
        Vector3 center = (minWorld + maxWorld) * 0.5f;

        Gizmos.color = new Color(0.2f, 1f, 0.3f, 0.4f);
        Gizmos.DrawWireCube(center, size);
    }

    [UnityEngine.ContextMenu("타일맵 범위에서 가져오기")]
    void SetBoundsFromTilemap()
    {
        var tileMgr = TileManager.Instance ?? FindObjectOfType<TileManager>();
        var groundTm = tileMgr != null ? tileMgr.groundTilemap : null;
        if (groundTm == null) groundTm = referenceTilemap;
        if (groundTm == null)
        {
            Debug.LogWarning("[NaturalObjectManager] Ground_0 타일맵이 없습니다. TileManager에 groundTilemap을 할당하세요.");
            return;
        }

        var bounds = groundTm.cellBounds;
        farmBoundsMin = bounds.min;
        farmBoundsMax = bounds.max;
        Debug.Log($"[NaturalObjectManager] 농장 영역 설정: Min={farmBoundsMin}, Max={farmBoundsMax}");
    }
#endif
}