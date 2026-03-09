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
    [Tooltip("나무-나무 최소 간격 (나무 스프라이트가 커서 겹침 방지, treeStoneMinSpacing보다 크게)")]
    public int treeMinSpacing = 4;
    [Tooltip("나무 스폰 시 주변 제거할 잔디 반경 (나무 foliage가 잔디와 겹치지 않도록)")]
    public int treeGrassClearRadius = 2;
    [Tooltip("플레이어 스폰 주변 비울 반경 (칸)")]
    public int playerSpawnClearRadius = 3;

    [Header("오브젝트 풀")]
    [Tooltip("잔디/나무/돌 풀 초기 크기 (0이면 풀링 비활성화)")]
    public int poolInitialSize = 50;

    private Dictionary<Vector3Int, NaturalObject> _objects = new Dictionary<Vector3Int, NaturalObject>();
    private ObjectPool<GrassObject> _grassPool;
    private ObjectPool<TreeObject> _treePool;
    private ObjectPool<StoneObject> _stonePool;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
            Instance = null;
        }
        Instance = this;

        if (referenceTilemap == null && TileManager.Instance != null)
            referenceTilemap = TileManager.Instance.farmTilemap;

        if (poolInitialSize > 0)
        {
            if (grassPrefab != null)
                _grassPool = new ObjectPool<GrassObject>(grassPrefab.GetComponent<GrassObject>(), poolInitialSize, transform);
            if (treePrefab != null)
                _treePool = new ObjectPool<TreeObject>(treePrefab.GetComponent<TreeObject>(), Mathf.Min(poolInitialSize / 4, 20), transform);
            if (stonePrefab != null)
                _stonePool = new ObjectPool<StoneObject>(stonePrefab.GetComponent<StoneObject>(), Mathf.Min(poolInitialSize / 4, 15), transform);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Start()
    {
        var tm = GetTilemap();
        if (_objects.Count == 0 && tm != null)
        {
            SpawnInitialNaturalObjects();
        }
        else if (_objects.Count == 0 && tm == null) { }
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

    /// <summary>자연물 제거 후 풀에 반환 (풀링 사용 시). 풀링 비활성화 시 Destroy</summary>
    public void ReturnToPool(NaturalObject obj)
    {
        var cell = WorldToCell(obj.transform.position);
        _objects.Remove(cell);

        if (obj is GrassObject g && _grassPool != null)
            _grassPool.Return(g);
        else if (obj is TreeObject t && _treePool != null)
            _treePool.Return(t);
        else if (obj is StoneObject s && _stonePool != null)
            _stonePool.Return(s);
        else
            Destroy(obj.gameObject);
    }

    public bool TryHitNaturalObject(Vector3 worldPos, ToolType toolType, out bool consumed)
    {
        consumed = false;
        var obj = GetNaturalObjectAt(worldPos);
        if (obj == null || !obj.CanHitWith(toolType)) return false;

        // 괭이로 잔디 클릭 시 주변 1셀(3x3) 잔디도 함께 제거
        if (obj is GrassObject && toolType == ToolType.Hoe)
        {
            var centerCell = WorldToCell(worldPos);
            var toHit = new List<GrassObject>();
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    var cell = centerCell + new Vector3Int(dx, dy, 0);
                    if (_objects.TryGetValue(cell, out var o) && o is GrassObject g)
                        toHit.Add(g);
                }
            }
            foreach (var g in toHit)
                g.OnHit(toolType);
            consumed = toHit.Count > 0;
            return consumed;
        }

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

        var toReturn = new List<NaturalObject>(_objects.Values);
        _objects.Clear();
        foreach (var obj in toReturn)
        {
            if (obj != null) ReturnToPool(obj);
        }

        if (data.naturalObjects == null || data.naturalObjects.Count == 0)
        {
            SpawnInitialNaturalObjects();
            return;
        }

        var tm = GetTilemap();
        if (tm == null) return;

        int loaded = 0;
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
            NaturalObject natural = null;

            if (save.objectType == "Grass" && _grassPool != null)
            {
                var g = _grassPool.Get();
                g.transform.position = spawnPos;
                natural = g;
            }
            else if (save.objectType == "Tree" && _treePool != null)
            {
                var t = _treePool.Get();
                t.transform.position = spawnPos;
                natural = t;
            }
            else if (save.objectType == "Stone" && _stonePool != null)
            {
                var s = _stonePool.Get();
                s.transform.position = spawnPos;
                natural = s;
            }
            else
            {
                var go = Instantiate(prefab, spawnPos, Quaternion.identity);
                natural = go.GetComponent<NaturalObject>();
            }

            if (natural == null) continue;

            if (save.objectType == "Tree")
                RemoveGrassAroundCell(cellPos);

            _objects[cellPos] = natural;
            DepthSortByY.Apply(natural.transform);
            loaded++;

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

        var treeCells = new HashSet<Vector3Int>();
        var stoneCells = new HashSet<Vector3Int>();

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
                    if (IsFarEnoughFrom(cell, treeCells, treeMinSpacing) && IsFarEnoughFrom(cell, stoneCells, treeStoneMinSpacing))
                    {
                        RemoveGrassAroundCell(cell);
                        SpawnAtCell(treePrefab, cell, go =>
                        {
                            var t = go.GetComponent<TreeObject>();
                            if (t != null) t.Initialize(false);
                        });
                        treeCells.Add(cell);
                    }
                }
                else if (r < grassSpawnChance + treeSpawnChance + stoneSpawnChance && stonePrefab != null)
                {
                    if (IsFarEnoughFrom(cell, treeCells, treeStoneMinSpacing) && IsFarEnoughFrom(cell, stoneCells, treeStoneMinSpacing))
                    {
                        SpawnAtCell(stonePrefab, cell, null);
                        stoneCells.Add(cell);
                    }
                }
            }
        }

    }

    /// <summary>나무 주변 잔디 제거 (나무 foliage가 잔디와 겹치지 않도록)</summary>
    void RemoveGrassAroundCell(Vector3Int centerCell)
    {
        if (treeGrassClearRadius <= 0) return;
        for (int dx = -treeGrassClearRadius; dx <= treeGrassClearRadius; dx++)
        {
            for (int dy = -treeGrassClearRadius; dy <= treeGrassClearRadius; dy++)
            {
                var cell = centerCell + new Vector3Int(dx, dy, 0);
                if (_objects.TryGetValue(cell, out var obj) && obj is GrassObject g)
                {
                    ReturnToPool(g);
                }
            }
        }
    }

    bool IsFarEnoughFrom(Vector3Int cell, HashSet<Vector3Int> existing, int minDistance)
    {
        foreach (var c in existing)
        {
            if (Vector3Int.Distance(cell, c) < minDistance)
                return false;
        }
        return true;
    }

    void SpawnAtCell(GameObject prefab, Vector3Int cellPos, System.Action<GameObject> onSpawned)
    {
        if (_objects.ContainsKey(cellPos)) return;

        var tm = GetTilemap();
        var spawnPos = tm.GetCellCenterWorld(cellPos);
        GameObject go = null;
        NaturalObject natural = null;

        if (prefab == grassPrefab && _grassPool != null)
        {
            var g = _grassPool.Get();
            g.transform.position = spawnPos;
            go = g.gameObject;
            natural = g;
        }
        else if (prefab == treePrefab && _treePool != null)
        {
            var t = _treePool.Get();
            t.transform.position = spawnPos;
            go = t.gameObject;
            natural = t;
        }
        else if (prefab == stonePrefab && _stonePool != null)
        {
            var s = _stonePool.Get();
            s.transform.position = spawnPos;
            go = s.gameObject;
            natural = s;
        }
        else
        {
            go = Instantiate(prefab, spawnPos, Quaternion.identity);
            natural = go.GetComponent<NaturalObject>();
        }

        if (natural != null)
        {
            _objects[cellPos] = natural;
            DepthSortByY.Apply(natural.transform);
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
    }
#endif
}