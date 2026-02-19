using UnityEngine;

/// <summary>
/// 씬에 배치된 아이템 오브젝트.
/// 플레이어가 닿으면 자동으로 인벤토리에 추가됨.
/// </summary>
public class ItemPickup : MonoBehaviour
{
    [Header("아이템 설정")]
    public ItemData itemData;   // 어떤 아이템인지
    public int count = 1;       // 줍는 수량

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (InventoryManager.Instance == null) return;

        bool added = InventoryManager.Instance.AddItem(itemData, count);

        if (added)
        {
            Debug.Log($"[ItemPickup] {itemData.itemName} x{count} 획득!");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("[ItemPickup] 인벤토리가 가득 찼습니다.");
        }
    }
}
