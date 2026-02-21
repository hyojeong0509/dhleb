using UnityEngine;

/// <summary>
/// 씨앗 아이템 데이터 (ItemData 상속)
/// Project 창에서 우클릭 → Create → Inventory → Seed 로 생성
/// </summary>
[CreateAssetMenu(fileName = "NewSeed", menuName = "Inventory/Seed")]
public class SeedData : ItemData
{
    [Header("성장 설정")]
    public int growthDays = 4;              // 완전히 자라는 데 걸리는 일 수

    [Header("성장 단계 스프라이트 (index 0 = 심은 직후, 마지막 = 수확 가능)")]
    public Sprite[] growthStages;           // 단계별 스프라이트 배열

    [Header("수확")]
    public ItemData harvestItem;            // 수확 시 얻는 아이템
    public int harvestCount = 1;            // 수확 수량

    // 향후 사용: 특정 Sequence에서만 자라게 하는 설정
    // public int[] validSequences;
}
