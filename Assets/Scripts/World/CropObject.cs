using UnityEngine;

/// <summary>
/// 씬에 심어진 작물 오브젝트
/// 날짜가 지날 때마다 성장 단계 업데이트
/// </summary>
public class CropObject : MonoBehaviour
{
    private SeedData seedData;
    private int currentStage;       // 현재 성장 단계
    private int wateredDays;        // 실제로 물을 준 날 수 (달력 날짜 X)
    private SpriteRenderer sr;

    public bool IsFullyGrown => seedData != null && currentStage >= seedData.growthStages.Length - 1;
    public SeedData SeedData => seedData;
    public int WateredDays => wateredDays;
    public int CurrentStage => currentStage;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 씨앗을 심을 때 초기화
    /// </summary>
    public void Initialize(SeedData seed, int currentDay)
    {
        seedData = seed;
        currentStage = 0;
        wateredDays = 0;
        UpdateSprite();
    }

    /// <summary>
    /// 저장 데이터에서 복원 (로드 시)
    /// </summary>
    public void LoadFromSave(SeedData seed, int savedWateredDays, int savedStage)
    {
        seedData = seed;
        wateredDays = savedWateredDays;
        currentStage = Mathf.Min(savedStage, seed.growthStages.Length - 1);
        UpdateSprite();
    }

    /// <summary>
    /// 물을 준 날에만 호출됨 → 실제 성장 횟수 기반으로 단계 계산
    /// </summary>
    public void OnDayPassed(int currentDay)
    {
        if (IsFullyGrown) return;

        wateredDays++;  // 물 준 날만 카운트

        int totalStages = seedData.growthStages.Length;

        // 물 준 횟수 기반으로 단계 계산 (날짜 차이 X)
        int newStage = Mathf.Min(
            Mathf.FloorToInt((float)wateredDays / seedData.growthDays * totalStages),
            totalStages - 1
        );

        if (newStage != currentStage)
        {
            currentStage = newStage;
            UpdateSprite();
        }

        Debug.Log($"[CropObject] {seedData.itemName} | 물 준 날: {wateredDays}/{seedData.growthDays} | 단계: {currentStage + 1}/{totalStages}");
    }

    /// <summary>
    /// 수확 - 수확물 반환 후 오브젝트 삭제
    /// </summary>
    public ItemData Harvest()
    {
        if (!IsFullyGrown) return null;

        ItemData item = seedData.harvestItem;
        int count = seedData.harvestCount;
        Vector3 position = transform.position;

        // 인벤토리 가득 찬 경우 캔 자리에서 드롭(스타듀밸리처럼)
        if (item != null)
        {
            if (!ItemPickup.SpawnAt(item, count, position))
            {
                if (InventoryManager.Instance != null)
                    InventoryManager.Instance.AddItem(item, count);
            }
        }

        Debug.Log($"[CropObject] {item?.itemName} x{count} 수확!");
        Destroy(gameObject);
        return item;
    }

    void UpdateSprite()
    {
        if (sr == null || seedData.growthStages == null) return;
        if (currentStage < seedData.growthStages.Length)
            sr.sprite = seedData.growthStages[currentStage];
    }
}
