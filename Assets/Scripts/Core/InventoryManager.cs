using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 인벤토리 전체 관리 (데이터 + 입력 + UI + 드래그)
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public const int TOTAL_SLOT_COUNT = 27;
    public const int HOTBAR_COUNT = 9;

    // ── 인벤토리 데이터 ──────────────────────────────────────
    private InventorySlotData[] slots = new InventorySlotData[TOTAL_SLOT_COUNT];

    // ── Inspector 연결 ──────────────────────────────────────
    [Header("핫바 슬롯들의 부모 오브젝트")]
    public Transform hotbarContainer;

    [Header("인벤토리 패널 (E키로 열리는 전체 창)")]
    public GameObject inventoryPanel;

    [Header("인벤토리 슬롯들의 부모 오브젝트")]
    public Transform inventoryContainer;

    [Header("땅에 버릴 때 생성할 프리팹 (ItemPickup 붙어있는 오브젝트)")]
    public GameObject droppedItemPrefab;

    [Header("플레이어 트랜스폼 (버리기 위치)")]
    public Transform playerTransform;

    // 자동 수집
    private InventorySlotUI[] hotbarSlotUIs;
    private InventorySlotUI[] inventorySlotUIs;

    // ── 상태 ────────────────────────────────────────────────
    private int activeSlotIndex = 0;
    private bool isInventoryOpen = false;
    public bool IsInventoryOpen => isInventoryOpen;

    // ── 들고있는 아이템 상태 ─────────────────────────────────
    private ItemData heldItem = null;
    private int heldCount = 0;
    public bool IsHoldingItem => heldItem != null;

    // ────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        for (int i = 0; i < TOTAL_SLOT_COUNT; i++)
            slots[i] = new InventorySlotData();

        if (hotbarContainer != null)
        {
            hotbarSlotUIs = hotbarContainer.GetComponentsInChildren<InventorySlotUI>();
            for (int i = 0; i < hotbarSlotUIs.Length; i++)
                hotbarSlotUIs[i].Setup(i);
        }

        if (inventoryContainer != null)
        {
            inventorySlotUIs = inventoryContainer.GetComponentsInChildren<InventorySlotUI>();
            for (int i = 0; i < inventorySlotUIs.Length; i++)
                inventorySlotUIs[i].Setup(i);
        }
    }

    void Start()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        RefreshHotbarUI();
        SetActiveSlot(0);
    }

    void Update()
    {
        HandleInput();
    }

    // ── 입력 처리 ───────────────────────────────────────────

    void HandleInput()
    {
        // 컷신 중에는 E키로 인벤토리 열기 차단
        if (CutsceneManager.Instance != null && CutsceneManager.Instance.IsPlaying)
            return;

        if (Input.GetKeyDown(KeyCode.E))
            ToggleInventory();

        // 인벤토리 열려있을 때는 핫바 단축키 비활성화
        if (isInventoryOpen) return;

        for (int i = 0; i < HOTBAR_COUNT; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                SetActiveSlot(i);
        }

        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (scroll > 0f)
            SetActiveSlot((activeSlotIndex - 1 + HOTBAR_COUNT) % HOTBAR_COUNT);
        else if (scroll < 0f)
            SetActiveSlot((activeSlotIndex + 1) % HOTBAR_COUNT);
    }

    /// <summary>
    /// 인벤토리 밖 클릭 시 들고있는 아이템 전부 버리기
    /// LateUpdate에서 처리해 슬롯 클릭(OnPointerClick)보다 나중에 실행되게 함
    /// </summary>
    void LateUpdate()
    {
        HandleOutsideClick();
    }

    void HandleOutsideClick()
    {
        if (!isInventoryOpen || !IsHoldingItem) return;
        if (!Input.GetMouseButtonDown(0)) return;

        // UI 위를 클릭하지 않은 경우 = 인벤토리 밖 클릭
        if (!EventSystem.current.IsPointerOverGameObject())
            DropHeldItem();
    }

    // ── 핫바 슬롯 선택 ──────────────────────────────────────

    public void SetActiveSlot(int index)
    {
        if (index < 0 || index >= HOTBAR_COUNT) return;

        hotbarSlotUIs[activeSlotIndex].SetSelected(false);
        activeSlotIndex = index;
        hotbarSlotUIs[activeSlotIndex].SetSelected(true);
    }

    public int GetActiveSlotIndex() => activeSlotIndex;
    public InventorySlotData GetActiveSlotData() => slots[activeSlotIndex];
    public InventorySlotData GetSlotData(int index) => (index >= 0 && index < TOTAL_SLOT_COUNT) ? slots[index] : null;

    // ── 인벤토리 토글 ───────────────────────────────────────

    void ToggleInventory()
    {
        // 들고있는 아이템이 있으면 원래 자리 / 빈 슬롯으로 반환
        if (isInventoryOpen && IsHoldingItem)
            ReturnHeldItem();

        // 닫을 때 툴팁 숨기기
        if (isInventoryOpen)
            ItemTooltip.Instance?.Hide();

        isInventoryOpen = !isInventoryOpen;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(isInventoryOpen);

        if (isInventoryOpen)
            RefreshInventoryUI();
    }

    // ── 슬롯 클릭 처리 ──────────────────────────────────────

    /// <summary>
    /// 좌클릭: 아이템 집기 / 놓기 / 스왑
    /// </summary>
    public void OnSlotLeftClick(int index)
    {
        if (!isInventoryOpen) return;

        if (!IsHoldingItem)
        {
            // 슬롯에 아이템 있으면 집기
            if (!slots[index].IsEmpty)
                PickUpAll(index);
        }
        else
        {
            // 빈 슬롯 → 놓기
            if (slots[index].IsEmpty)
                PlaceAll(index);
            // 같은 아이템 → 합치기
            else if (slots[index].item == heldItem)
                MergeToSlot(index);
            // 다른 아이템 → 스왑
            else
                SwapWithSlot(index);
        }

        RefreshAll();
    }

    /// <summary>
    /// 우클릭: 아이템 절반 집기 / 1개씩 놓기
    /// </summary>
    public void OnSlotRightClick(int index)
    {
        if (!isInventoryOpen) return;

        if (!IsHoldingItem)
        {
            // 슬롯에 아이템 있으면 절반 집기
            if (!slots[index].IsEmpty)
                PickUpHalf(index);
        }
        else
        {
            // 같은 아이템이거나 빈 슬롯이면 1개 놓기
            if (slots[index].IsEmpty || slots[index].item == heldItem)
                PlaceOne(index);
        }

        RefreshAll();
    }

    // ── 아이템 집기/놓기 로직 ───────────────────────────────

    void PickUpAll(int index)
    {
        heldItem = slots[index].item;
        heldCount = slots[index].count;
        slots[index].Clear();
        CursorItem.Instance?.Show(heldItem, heldCount);
    }

    void PickUpHalf(int index)
    {
        int half = Mathf.CeilToInt(slots[index].count / 2f);
        heldItem = slots[index].item;
        heldCount = half;
        slots[index].count -= half;
        if (slots[index].count <= 0) slots[index].Clear();
        CursorItem.Instance?.Show(heldItem, heldCount);
    }

    void PlaceAll(int index)
    {
        slots[index].item = heldItem;
        slots[index].count = heldCount;
        ClearHeld();
    }

    void PlaceOne(int index)
    {
        if (slots[index].IsEmpty)
        {
            slots[index].item = heldItem;
            slots[index].count = 0;
        }

        if (slots[index].count < heldItem.maxStackSize)
        {
            slots[index].count++;
            heldCount--;
        }

        if (heldCount <= 0)
            ClearHeld();
        else
            CursorItem.Instance?.Show(heldItem, heldCount);
    }

    void MergeToSlot(int index)
    {
        int canAdd = heldItem.maxStackSize - slots[index].count;
        int add = Mathf.Min(canAdd, heldCount);
        slots[index].count += add;
        heldCount -= add;

        if (heldCount <= 0)
            ClearHeld();
        else
            CursorItem.Instance?.Show(heldItem, heldCount);
    }

    void SwapWithSlot(int index)
    {
        ItemData tempItem = slots[index].item;
        int tempCount = slots[index].count;

        slots[index].item = heldItem;
        slots[index].count = heldCount;

        heldItem = tempItem;
        heldCount = tempCount;
        CursorItem.Instance?.Show(heldItem, heldCount);
    }

    /// <summary>
    /// 들고있는 아이템을 빈 슬롯으로 반환 (인벤토리 닫을 때)
    /// </summary>
    void ReturnHeldItem()
    {
        AddItem(heldItem, heldCount, playAcquisitionSound: false);
        ClearHeld();
    }

    /// <summary>
    /// 들고있는 아이템을 땅에 버리기 (인벤토리 밖 클릭)
    /// </summary>
    void DropHeldItem()
    {
        if (!IsHoldingItem) return;

        if (droppedItemPrefab != null && playerTransform != null)
        {
            Vector3 dropPos = playerTransform.position + (Vector3)Random.insideUnitCircle * 0.5f;
            GameObject dropped = Instantiate(droppedItemPrefab, dropPos, Quaternion.identity);
            ItemPickup pickup = dropped.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                pickup.itemData = heldItem;
                pickup.count = heldCount;
                pickup.SetupVisual();
            }
        }

        ClearHeld();
        RefreshAll();
    }

    void ClearHeld()
    {
        heldItem = null;
        heldCount = 0;
        CursorItem.Instance?.Hide();
    }

    // ── UI 갱신 ─────────────────────────────────────────────

    public void RefreshHotbarUI()
    {
        for (int i = 0; i < HOTBAR_COUNT; i++)
        {
            if (i < hotbarSlotUIs.Length && hotbarSlotUIs[i] != null)
                hotbarSlotUIs[i].UpdateSlot(slots[i]);
        }
    }

    void RefreshInventoryUI()
    {
        for (int i = 0; i < TOTAL_SLOT_COUNT; i++)
        {
            if (i < inventorySlotUIs.Length && inventorySlotUIs[i] != null)
                inventorySlotUIs[i].UpdateSlot(slots[i]);
        }
    }

    public void RefreshAll()
    {
        RefreshHotbarUI();
        if (isInventoryOpen)
            RefreshInventoryUI();
    }

    // ── 저장용 데이터 수집 ────────────────────────────────────

    /// <summary>
    /// 인벤토리 데이터를 저장용 배열에 채움
    /// </summary>
    public void FillSaveData(InventorySlotSaveData[] arr)
    {
        if (arr == null || arr.Length < TOTAL_SLOT_COUNT) return;

        for (int i = 0; i < TOTAL_SLOT_COUNT; i++)
        {
            if (slots[i].IsEmpty)
                arr[i] = new InventorySlotSaveData { itemName = "", count = 0 };
            else
                arr[i] = new InventorySlotSaveData { itemName = slots[i].item.itemName, count = slots[i].count };
        }
    }

    /// <summary>
    /// 저장 데이터에서 인벤토리 복원
    /// </summary>
    public void LoadFromSaveData(InventorySlotSaveData[] arr)
    {
        if (arr == null) return;

        for (int i = 0; i < TOTAL_SLOT_COUNT && i < arr.Length; i++)
        {
            slots[i].Clear();

            if (string.IsNullOrEmpty(arr[i].itemName) || arr[i].count <= 0) continue;

            ItemData item = ItemDatabase.Instance?.GetItemByName(arr[i].itemName);
            if (item != null)
            {
                slots[i].item = item;
                slots[i].count = arr[i].count;
            }
        }

        RefreshAll();
    }

    // ── 아이템 추가 ─────────────────────────────────────────

    public bool AddItem(ItemData item, int count = 1, bool playAcquisitionSound = true)
    {
        // 기존 스택에 합산
        for (int i = 0; i < TOTAL_SLOT_COUNT; i++)
        {
            if (!slots[i].IsEmpty && slots[i].item == item && slots[i].count < item.maxStackSize)
            {
                int canAdd = item.maxStackSize - slots[i].count;
                int add = Mathf.Min(canAdd, count);
                slots[i].count += add;
                count -= add;
                if (count <= 0)
                {
                    RefreshAll();
                    if (playAcquisitionSound && SoundManager.Instance != null)
                        SoundManager.Instance.PlayItemPickupSound();
                    return true;
                }
            }
        }

        // 빈 슬롯에 배치
        for (int i = 0; i < TOTAL_SLOT_COUNT; i++)
        {
            if (slots[i].IsEmpty)
            {
                slots[i].item = item;
                slots[i].count = Mathf.Min(count, item.maxStackSize);
                count -= slots[i].count;
                if (count <= 0)
                {
                    RefreshAll();
                    if (playAcquisitionSound && SoundManager.Instance != null)
                        SoundManager.Instance.PlayItemPickupSound();
                    return true;
                }
            }
        }

        RefreshAll();
        return false;
    }
}
