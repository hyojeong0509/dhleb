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

    [Header("플레이어 지나갈 때 반투명")]
    [Range(0.1f, 1f)]
    [Tooltip("플레이어가 나무 뒤로 들어왔을 때 알파값 (Polygon Collider Is Trigger 체크 필요)")]
    public float fadeAlpha = 0.4f;
    public string playerTag = "Player";

    private int _hitsRemaining; // 10~4 나무, 3~1 밑동
    private bool _isStump;
    private SpriteRenderer _sr;
    private Animator _anim;
    private SpriteRenderer[] _allSprites;
    private Color[] _originalColors;
    private int _playerOverlapCount;

    public override string ObjectType => "Tree";
    public override int HitsRemaining => _hitsRemaining;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _anim = GetComponent<Animator>();
        _allSprites = GetComponentsInChildren<SpriteRenderer>(true);
        _originalColors = new Color[_allSprites.Length];
        for (int i = 0; i < _allSprites.Length; i++)
        {
            if (_allSprites[i] != null)
                _originalColors[i] = _allSprites[i].color;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (_isStump) return; // 밑둥만 있을 때는 반투명 적용 안 함
        _playerOverlapCount++;
        SetAlpha(fadeAlpha);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        _playerOverlapCount = Mathf.Max(0, _playerOverlapCount - 1);
        if (_playerOverlapCount == 0)
            RestoreAlpha();
    }

    void SetAlpha(float alpha)
    {
        if (_allSprites == null || _originalColors == null) return;
        for (int i = 0; i < _allSprites.Length; i++)
        {
            if (_allSprites[i] == null) continue;
            var c = _originalColors[i];
            _allSprites[i].color = new Color(c.r, c.g, c.b, alpha);
        }
    }

    void RestoreAlpha()
    {
        if (_allSprites == null || _originalColors == null) return;
        for (int i = 0; i < _allSprites.Length; i++)
        {
            if (_allSprites[i] == null) continue;
            _allSprites[i].color = _originalColors[i];
        }
    }

    public void Initialize(bool fromSave, int savedHitsRemaining = 10)
    {
        _hitsRemaining = fromSave ? savedHitsRemaining : 10;
        _isStump = _hitsRemaining <= hitsToBreakStump && _hitsRemaining > 0;
        _playerOverlapCount = 0;
        RestoreAlpha();
        if (_anim != null && !string.IsNullOrEmpty(isStumpParam))
            _anim.SetBool(isStumpParam, _isStump);
        else if (_isStump && _sr != null && stumpSprite != null)
            _sr.sprite = stumpSprite; // Animator 없을 때 폴백
    }

    public override bool CanHitWith(ToolType toolType) => toolType == ToolType.Axe;

    public override bool OnHit(ToolType toolType)
    {
        if (!CanHitWith(toolType)) return false;

        SoundManager.Instance?.PlayHitWoodSound();

        if (_isStump)
        {
            _hitsRemaining--;
            if (_anim != null && !string.IsNullOrEmpty(stumpHitTrigger))
                _anim.SetTrigger(stumpHitTrigger);

            if (_hitsRemaining <= 0)
            {
                var item = GetWoodItem();
                if (item != null)
                {
                    if (!ItemPickup.SpawnAt(item, woodDropOnStump, transform.position))
                    {
                        if (InventoryManager.Instance != null)
                            InventoryManager.Instance.AddItem(item, woodDropOnStump);
                    }
                }
                if (NaturalObjectManager.Instance != null)
                    NaturalObjectManager.Instance.ReturnToPool(this);
            }
            return true;
        }

        _hitsRemaining--;
        if (_anim != null && !string.IsNullOrEmpty(treeHitTrigger))
            _anim.SetTrigger(treeHitTrigger);

        if (_hitsRemaining <= hitsToBreakStump)
        {
            _isStump = true;
            _playerOverlapCount = 0; // 밑둥 전환 시 반투명 해제
            RestoreAlpha();
            if (_anim != null && !string.IsNullOrEmpty(isStumpParam))
                _anim.SetBool(isStumpParam, true);
            else if (_sr != null && stumpSprite != null)
                _sr.sprite = stumpSprite; // Animator 없을 때 폴백
            var item = GetWoodItem();
            if (item != null)
            {
                if (!ItemPickup.SpawnAt(item, woodDropOnFall, transform.position))
                {
                    if (InventoryManager.Instance != null)
                        InventoryManager.Instance.AddItem(item, woodDropOnFall);
                }
            }
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
