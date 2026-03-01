using UnityEngine;
using System.Collections;

/// <summary>
/// 돌 - 곡괭이 1회로 파괴. Hit 시 1프레임 스프라이트 교체 후 파괴
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class StoneObject : NaturalObject
{
    [Header("스프라이트")]
    public Sprite normalSprite;
    public Sprite hitSprite;

    [Header("드롭")]
    [Tooltip("드롭할 돌 아이템 (비우면 ItemDatabase에서 '돌' 검색)")]
    public ItemData dropItem;
    public int dropCount = 1;

    [Header("피격 효과")]
    public float hitSpriteDuration = 0.08f;

    private SpriteRenderer _sr;
    private bool _isBeingDestroyed;

    public override string ObjectType => "Stone";
    public override int HitsRemaining => 1;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (normalSprite != null && _sr != null)
            _sr.sprite = normalSprite;
    }

    public override bool CanHitWith(ToolType toolType) => toolType == ToolType.Pickaxe;

    public override bool OnHit(ToolType toolType)
    {
        if (!CanHitWith(toolType) || _isBeingDestroyed) return false;
        _isBeingDestroyed = true;
        StartCoroutine(HitAndDestroy());
        return true;
    }

    IEnumerator HitAndDestroy()
    {
        if (_sr != null && hitSprite != null)
        {
            _sr.sprite = hitSprite;
            yield return new WaitForSecondsRealtime(hitSpriteDuration);
        }

        var item = dropItem != null ? dropItem : ItemDatabase.Instance?.GetItemByName("돌");
        if (item != null)
            SpawnDroppedItem(item, dropCount);

        if (NaturalObjectManager.Instance != null)
            NaturalObjectManager.Instance.RemoveNaturalObject(this);
        Destroy(gameObject);
    }

    void SpawnDroppedItem(ItemData item, int count)
    {
        var inv = InventoryManager.Instance;
        if (inv == null || inv.droppedItemPrefab == null || inv.playerTransform == null)
        {
            inv?.AddItem(item, count);
            return;
        }
        Vector3 dropPos = inv.playerTransform.position + (Vector3)Random.insideUnitCircle.normalized * 1.5f;
        var go = Instantiate(inv.droppedItemPrefab, dropPos, Quaternion.identity);
        var pickup = go.GetComponent<ItemPickup>();
        if (pickup != null)
        {
            pickup.itemData = item;
            pickup.count = count;
            pickup.SetupVisual();
        }
    }

    public override NaturalObjectSaveData ToSaveData(Vector3Int cellPos)
    {
        return new NaturalObjectSaveData
        {
            cellX = cellPos.x,
            cellY = cellPos.y,
            cellZ = cellPos.z,
            objectType = ObjectType,
            hitsRemaining = 1
        };
    }
}
