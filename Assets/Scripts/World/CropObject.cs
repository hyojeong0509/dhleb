using UnityEngine;

/// <summary>
/// 씬에 심어진 작물 오브젝트
/// 날짜가 지날 때마다 성장 단계 업데이트
/// </summary>
public class CropObject : MonoBehaviour
{
    private SeedData seedData;
    private int dayPlanted;         // 심은 날
    private int currentStage;       // 현재 성장 단계
    private SpriteRenderer sr;

    public bool IsFullyGrown => currentStage >= seedData.growthStages.Length - 1;
    public SeedData SeedData => seedData;

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
        dayPlanted = currentDay;
        currentStage = 0;
        UpdateSprite();
    }

    /// <summary>
    /// 날짜가 지날 때 TimeManager에서 호출
    /// </summary>
    public void OnDayPassed(int currentDay)
    {
        if (IsFullyGrown) return;

        int daysGrown = currentDay - dayPlanted;
        int totalStages = seedData.growthStages.Length;

        // 성장 진행도에 따라 단계 계산
        int newStage = Mathf.Min(
            Mathf.FloorToInt((float)daysGrown / seedData.growthDays * totalStages),
            totalStages - 1
        );

        if (newStage != currentStage)
        {
            currentStage = newStage;
            UpdateSprite();
            Debug.Log($"[CropObject] {seedData.itemName} 성장: {currentStage + 1} / {totalStages} 단계");
        }
    }

    /// <summary>
    /// 수확 - 수확물 반환 후 오브젝트 삭제
    /// </summary>
    public ItemData Harvest()
    {
        if (!IsFullyGrown) return null;

        ItemData item = seedData.harvestItem;
        int count = seedData.harvestCount;

        // 인벤토리에 수확물 추가
        if (InventoryManager.Instance != null && item != null)
            InventoryManager.Instance.AddItem(item, count);

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
