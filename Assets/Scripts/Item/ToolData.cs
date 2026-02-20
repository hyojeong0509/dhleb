using UnityEngine;

/// <summary>
/// 도구 아이템 데이터 (ItemData 상속)
/// Project 창에서 우클릭 → Create → Inventory → Tool 로 생성
/// </summary>
[CreateAssetMenu(fileName = "NewTool", menuName = "Inventory/Tool")]
public class ToolData : ItemData
{
    [Header("도구 설정")]
    public ToolType toolType;   // 도구 종류
    public int range = 1;       // 사용 범위 (칸 수, 기본 1칸)
}

public enum ToolType
{
    Hoe,            // 괭이 - 땅 갈기
    WateringCan,    // 물뿌리개 - 물 주기
    Axe,            // 도끼 - 나무 채취
    Pickaxe,        // 곡괭이 - 광석 채취
    Sword           // 검 - 전투
}
