using UnityEngine;

// ============================================================
// 아이템 데이터 정의 (ScriptableObject)
// Project 창에서 우클릭 → Create → Inventory → Item 으로 생성
// ============================================================
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    public string itemName;         // 아이템 이름
    public Sprite icon;             // 아이템 아이콘
    public int maxStackSize = 99;   // 최대 스택 수량

    [Header("아이템 종류")]
    public ItemType itemType;       // 아이템 타입 (향후 사용)

    [TextArea]
    public string description;      // 아이템 설명 (향후 사용)
}

// 아이템 종류 (확장 가능)
public enum ItemType
{
    None,
    Tool,       // 도구 (괭이, 물뿌리개 등)
    Seed,       // 씨앗
    Crop,       // 작물
    Material,   // 재료
    Etc         // 기타
}

// ============================================================
// 인벤토리 슬롯 데이터 (아이템 + 수량)
// ============================================================
[System.Serializable]
public class InventorySlotData
{
    public ItemData item;  // 아이템 (없으면 null)
    public int count;      // 수량

    public bool IsEmpty => item == null || count <= 0;

    public void Clear()
    {
        item = null;
        count = 0;
    }
}
