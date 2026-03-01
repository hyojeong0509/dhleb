using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

// 타일맵 변경 관리
public class TileManager : MonoBehaviour
{
    public static TileManager Instance { get; private set; }

    [Header("타일맵 레퍼런스 (FarmGrid 자식)")]
    [Tooltip("Ground_0 - 잔디/흙이 깔린 원본 바닥. 여기에 타일이 있어야 경작 가능")]
    public Tilemap groundTilemap;
    [Tooltip("Farm_0.5 - 갈린 흙이 배치되는 레이어. 괭이로 갈면 이 레이어에 타일 생성")]
    public Tilemap farmTilemap;
    [Tooltip("FarmOverlay - 물 준 오버레이. 물뿌리개 사용 시 이 레이어에 표시")]
    public Tilemap waterOverlayTilemap;

    [Header("밭 타일")]
    public TileBase tilledSoilTile;     // 갈린 흙 타일
    public TileBase wateredOverlayTile; // 물 준 오버레이 타일

    [Header("작물 설정")]
    public GameObject cropPrefab;       // 작물 프리팹

    // 심어진 작물 관리 (타일 좌표 → CropObject)
    private Dictionary<Vector3Int, CropObject> crops = new Dictionary<Vector3Int, CropObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnEnable()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.OnDayChanged += OnDayChanged;
    }

    void OnDisable()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.OnDayChanged -= OnDayChanged;
    }

    // TimeManager가 나중에 초기화될 수 있으므로 Start에서도 구독 시도
    void Start()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.OnDayChanged += OnDayChanged;
    }

    // 괭이 사용 - 해당 위치를 밭으로 만들기
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
        QuestManager.Instance?.NotifyObjectiveProgress(QuestObjectiveType.TillSoil, "", 1);
        Debug.Log($"[TileManager] 땅을 갈았습니다: {cellPos}");
        return true;
    }

    // 물뿌리개 사용 - 갈린 흙 위에 오버레이 배치
    public bool TryWater(Vector3 worldPosition)
    {
        Vector3Int cellPos = farmTilemap.WorldToCell(worldPosition);

        // 갈린 흙이어야 물 주기 가능
        if (!IsTilled(worldPosition))
        {
            Debug.Log("[TileManager] 갈린 흙이 아닙니다.");
            return false;
        }

        // 이미 물 줬으면 스킵
        if (IsWatered(worldPosition))
        {
            Debug.Log("[TileManager] 이미 물을 줬습니다.");
            return false;
        }

        // 오버레이 레이어에 물 준 타일 배치
        waterOverlayTilemap.SetTile(cellPos, wateredOverlayTile);
        QuestManager.Instance?.NotifyObjectiveProgress(QuestObjectiveType.WaterSoil, "", 1);
        Debug.Log($"[TileManager] 물을 주었습니다: {cellPos}");
        return true;
    }

    // 해당 위치가 갈린 흙인지 확인
    public bool IsTilled(Vector3 worldPosition)
    {
        Vector3Int cellPos = farmTilemap.WorldToCell(worldPosition);
        return farmTilemap.GetTile(cellPos) == tilledSoilTile;
    }

    // 해당 위치에 물 줬는지 확인
    public bool IsWatered(Vector3 worldPosition)
    {
        Vector3Int cellPos = waterOverlayTilemap.WorldToCell(worldPosition);
        return waterOverlayTilemap.GetTile(cellPos) == wateredOverlayTile;
    }

    // 씨앗 심기
    public bool TryPlantSeed(Vector3 worldPosition, SeedData seed, int currentDay)
    {
        Vector3Int cellPos = farmTilemap.WorldToCell(worldPosition);

        // 갈린 흙 or 물 준 흙이어야 심기 가능
        if (!IsTilled(worldPosition) && !IsWatered(worldPosition))
        {
            Debug.Log("[TileManager] 갈린 흙이 아닙니다.");
            return false;
        }

        // 이미 작물이 있으면 스킵
        if (crops.ContainsKey(cellPos))
        {
            Debug.Log("[TileManager] 이미 작물이 있습니다.");
            return false;
        }

        // 작물 오브젝트 생성
        Vector3 spawnPos = farmTilemap.GetCellCenterWorld(cellPos);
        GameObject cropGo = Instantiate(cropPrefab, spawnPos, Quaternion.identity);
        CropObject crop = cropGo.GetComponent<CropObject>();
        crop.Initialize(seed, currentDay);

        crops[cellPos] = crop;
        QuestManager.Instance?.NotifyObjectiveProgress(QuestObjectiveType.PlantSeed, seed.itemName, 1);
        Debug.Log($"[TileManager] {seed.itemName} 심기 완료: {cellPos}");

        // 첫 씨앗 심기 시 NPC 행패 컷신 → 종료 후 우호 NPC 컷신
        if (GameProgressManager.Instance != null && !GameProgressManager.Instance.HasFlag("planted_first_seed"))
        {
            var cutscene = Resources.Load<CutsceneData>("Cutscene/Cutscene_NPC_Harass");
            if (cutscene != null && CutsceneManager.Instance != null)
            {
                CutsceneManager.Instance.Play(cutscene, () =>
                {
                    var friendly = Resources.Load<CutsceneData>("Cutscene/Cutscene_FriendlyNPC");
                    if (friendly != null)
                        CutsceneManager.Instance.Play(friendly);
                });
            }
        }

        return true;
    }

    // 수확 - 다 자란 작물이 있으면 수확
    public bool TryHarvest(Vector3 worldPosition)
    {
        Vector3Int cellPos = farmTilemap.WorldToCell(worldPosition);

        if (!crops.TryGetValue(cellPos, out CropObject crop))
            return false;

        if (!crop.IsFullyGrown)
        {
            Debug.Log("[TileManager] 아직 다 자라지 않았습니다.");
            return false;
        }

        string cropName = crop.SeedData != null ? crop.SeedData.itemName : "";
        crop.Harvest();
        crops.Remove(cellPos);
        QuestManager.Instance?.NotifyObjectiveProgress(QuestObjectiveType.Harvest, cropName, 1);
        return true;
    }

    // 해당 위치에 작물이 있는지 확인
    public bool HasCrop(Vector3 worldPosition)
    {
        Vector3Int cellPos = farmTilemap.WorldToCell(worldPosition);
        return crops.ContainsKey(cellPos);
    }

    // 해당 위치의 작물이 다 자랐는지 확인
    public bool IsFullyGrown(Vector3 worldPosition)
    {
        Vector3Int cellPos = farmTilemap.WorldToCell(worldPosition);
        return crops.TryGetValue(cellPos, out CropObject crop) && crop.IsFullyGrown;
    }

    // 날짜가 지날 때 모든 작물 성장 업데이트 + 물 오버레이 초기화
    // 물을 준 작물만 성장
    void OnDayChanged()
    {
        int currentDay = TimeManager.Instance.CurrentDay;

        foreach (var kvp in crops)
        {
            if (kvp.Value == null) continue;

            Vector3Int cellPos = kvp.Key;
            bool wasWatered = waterOverlayTilemap.GetTile(cellPos) == wateredOverlayTile;

            if (wasWatered)
                kvp.Value.OnDayPassed(currentDay);
            else
                Debug.Log($"[TileManager] {kvp.Value.SeedData.itemName} 물 부족으로 성장 멈춤");
        }

        ClearWaterOverlay();
    }

    // 하루가 지나면 물 오버레이 전체 제거
    public void ClearWaterOverlay()
    {
        waterOverlayTilemap.ClearAllTiles();
        Debug.Log("[TileManager] 물 오버레이 초기화 완료");
    }

    // ── 저장용 데이터 수집 ────────────────────────────────────

    /// <summary>
    /// 현재 농장 상태를 FarmData로 내보냄
    /// </summary>
    public FarmData ExportFarmData()
    {
        var data = new FarmData();

        // 갈린 흙 타일
        if (farmTilemap != null && tilledSoilTile != null)
        {
            foreach (var pos in farmTilemap.cellBounds.allPositionsWithin)
            {
                if (farmTilemap.GetTile(pos) == tilledSoilTile)
                    data.tilledCells.Add(ToCellSaveData(pos));
            }
        }

        // 물 준 흙 타일
        if (waterOverlayTilemap != null && wateredOverlayTile != null)
        {
            foreach (var pos in waterOverlayTilemap.cellBounds.allPositionsWithin)
            {
                if (waterOverlayTilemap.GetTile(pos) == wateredOverlayTile)
                    data.wateredCells.Add(ToCellSaveData(pos));
            }
        }

        // 작물
        foreach (var kvp in crops)
        {
            if (kvp.Value == null) continue;

            var crop = kvp.Value;
            data.crops.Add(new CropSaveData
            {
                cellX = kvp.Key.x,
                cellY = kvp.Key.y,
                cellZ = kvp.Key.z,
                seedItemName = crop.SeedData.itemName,
                wateredDays = crop.WateredDays,
                currentStage = crop.CurrentStage
            });
        }

        return data;
    }

    static CellSaveData ToCellSaveData(Vector3Int v)
    {
        return new CellSaveData { x = v.x, y = v.y, z = v.z };
    }

    /// <summary>
    /// 저장 데이터에서 농장 상태 복원
    /// </summary>
    public void LoadFarmData(FarmData data)
    {
        if (data == null) return;

        // 기존 타일/작물 제거
        farmTilemap?.ClearAllTiles();
        waterOverlayTilemap?.ClearAllTiles();
        foreach (var crop in crops.Values)
        {
            if (crop != null) Destroy(crop.gameObject);
        }
        crops.Clear();

        // 갈린 흙 복원
        if (farmTilemap != null && tilledSoilTile != null)
        {
            foreach (var cell in data.tilledCells)
            {
                var pos = new Vector3Int(cell.x, cell.y, cell.z);
                farmTilemap.SetTile(pos, tilledSoilTile);
            }
        }

        // 물 준 흙 복원
        if (waterOverlayTilemap != null && wateredOverlayTile != null)
        {
            foreach (var cell in data.wateredCells)
            {
                var pos = new Vector3Int(cell.x, cell.y, cell.z);
                waterOverlayTilemap.SetTile(pos, wateredOverlayTile);
            }
        }

        // 작물 복원
        if (cropPrefab != null && ItemDatabase.Instance != null)
        {
            foreach (var c in data.crops)
            {
                var seed = ItemDatabase.Instance.GetItemByName(c.seedItemName) as SeedData;
                if (seed == null) continue;

                var cellPos = new Vector3Int(c.cellX, c.cellY, c.cellZ);
                var spawnPos = farmTilemap.GetCellCenterWorld(cellPos);

                GameObject cropGo = Instantiate(cropPrefab, spawnPos, Quaternion.identity);
                CropObject crop = cropGo.GetComponent<CropObject>();
                crop.LoadFromSave(seed, c.wateredDays, c.currentStage);

                crops[cellPos] = crop;
            }
        }

        Debug.Log($"[TileManager] 농장 복원 완료: 갈린흙 {data.tilledCells.Count}, 물 {data.wateredCells.Count}, 작물 {data.crops.Count}");
    }
}
