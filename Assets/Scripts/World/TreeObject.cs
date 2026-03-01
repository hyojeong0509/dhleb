using UnityEngine;

/// <summary>
/// 나무 - 도끼 7회 → 쓰러짐(밑동), 3회 더 → 파괴.
/// Hit 시 4프레임 애니메이션 재생, 밑동도 Hit 애니 있음
/// hitsRemaining: 10~4=나무, 3~1=밑동
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider2D))]
public class TreeObject : NaturalObject
{
    [Header("나무 설정")]
    public int hitsToFall = 7;
    public int hitsToBreakStump = 3;
    [Tooltip("드롭할 나무 아이템 (비우면 ItemDatabase에서 '나무' 검색)")]
    public ItemData woodItem;
    public int woodDropOnFall = 5;
    public int woodDropOnStump = 2;

    [Header("스프라이트")]
    public Sprite stumpSprite;

    [Header("애니메이션")]
    [Tooltip("나무 Hit 트리거 이름")]
    public string treeHitTrigger = "Hit";
    [Tooltip("밑동 Hit 트리거 이름")]
    public string stumpHitTrigger = "StumpHit";
    [Tooltip("밑동 여부 Bool 파라미터 (Animator에서 Idle↔StumpIdle 전환용)")]
    public string isStumpParam = "IsStump";

    private int _hitsRemaining; // 10~4 나무, 3~1 밑동
    private bool _isStump;
    private SpriteRenderer _sr;
    private Animator _anim;

    public override string ObjectType => "Tree";
    public override int HitsRemaining => _hitsRemaining;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _anim = GetComponent<Animator>();
    }

    public void Initialize(bool fromSave, int savedHitsRemaining = 10)
    {
        _hitsRemaining = fromSave ? savedHitsRemaining : 10;
        _isStump = _hitsRemaining <= hitsToBreakStump && _hitsRemaining > 0;
        if (_anim != null && !string.IsNullOrEmpty(isStumpParam))
            _anim.SetBool(isStumpParam, _isStump);
        else if (_isStump && _sr != null && stumpSprite != null)
            _sr.sprite = stumpSprite; // Animator 없을 때 폴백
    }

    public override bool CanHitWith(ToolType toolType) => toolType == ToolType.Axe;

    public override bool OnHit(ToolType toolType)
    {
        if (!CanHitWith(toolType)) return false;

        if (_isStump)
        {
            _hitsRemaining--;
            if (_anim != null && !string.IsNullOrEmpty(stumpHitTrigger))
                _anim.SetTrigger(stumpHitTrigger);

            if (_hitsRemaining <= 0)
            {
                var item = GetWoodItem();
                if (item != null && InventoryManager.Instance != null)
                    InventoryManager.Instance.AddItem(item, woodDropOnStump);
                if (NaturalObjectManager.Instance != null)
                    NaturalObjectManager.Instance.RemoveNaturalObject(this);
                Destroy(gameObject);
            }
            return true;
        }

        _hitsRemaining--;
        if (_anim != null && !string.IsNullOrEmpty(treeHitTrigger))
            _anim.SetTrigger(treeHitTrigger);

        if (_hitsRemaining <= hitsToBreakStump)
        {
            _isStump = true;
            if (_anim != null && !string.IsNullOrEmpty(isStumpParam))
                _anim.SetBool(isStumpParam, true);
            else if (_sr != null && stumpSprite != null)
                _sr.sprite = stumpSprite; // Animator 없을 때 폴백
            var item = GetWoodItem();
            if (item != null && InventoryManager.Instance != null)
                InventoryManager.Instance.AddItem(item, woodDropOnFall);
        }
        return true;
    }

    ItemData GetWoodItem()
    {
        if (woodItem != null) return woodItem;
        return ItemDatabase.Instance?.GetItemByName("나무");
    }

    public override NaturalObjectSaveData ToSaveData(Vector3Int cellPos)
    {
        return new NaturalObjectSaveData
        {
            cellX = cellPos.x,
            cellY = cellPos.y,
            cellZ = cellPos.z,
            objectType = ObjectType,
            hitsRemaining = _hitsRemaining
        };
    }
}
