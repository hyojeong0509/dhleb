using UnityEngine;

/// <summary>
/// 인벤토리 전체 관리 (데이터 + 입력 + UI)
/// - 총 27칸 데이터
/// - 핫바: 0~8번 슬롯 (9칸)
/// - 인벤토리 패널: 0~26번 슬롯 (27칸, E키로 토글)
/// - 핫바와 인벤토리 패널 첫 번째 행은 같은 데이터 공유
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
    public Transform hotbarContainer;       // 핫바 슬롯 9개를 담고 있는 부모

    [Header("인벤토리 패널 (E키로 열리는 전체 창)")]
    public GameObject inventoryPanel;

    [Header("인벤토리 슬롯들의 부모 오브젝트")]
    public Transform inventoryContainer;    // 인벤토리 슬롯 27개를 담고 있는 부모

    // 자동 수집 (Inspector 노출 불필요)
    private InventorySlotUI[] hotbarSlotUIs;
    private InventorySlotUI[] inventorySlotUIs;

    // ── 상태 ────────────────────────────────────────────────
    private int activeSlotIndex = 0;
    private bool isInventoryOpen = false;

    // ────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 슬롯 데이터 초기화
        for (int i = 0; i < TOTAL_SLOT_COUNT; i++)
            slots[i] = new InventorySlotData();

        // 컨테이너에서 슬롯 UI 자동 수집
        if (hotbarContainer != null)
            hotbarSlotUIs = hotbarContainer.GetComponentsInChildren<InventorySlotUI>();

        if (inventoryContainer != null)
            inventorySlotUIs = inventoryContainer.GetComponentsInChildren<InventorySlotUI>();
    }

    void Start()
    {
        // 인벤토리 패널은 시작 시 닫기
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
        // E 키 - 인벤토리 열기/닫기
        if (Input.GetKeyDown(KeyCode.E))
            ToggleInventory();

        // 숫자 키 1~9 - 핫바 슬롯 선택
        for (int i = 0; i < HOTBAR_COUNT; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                SetActiveSlot(i);
        }

        // 마우스 휠 - 핫바 슬롯 순환
        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (scroll > 0f)
            SetActiveSlot((activeSlotIndex - 1 + HOTBAR_COUNT) % HOTBAR_COUNT);
        else if (scroll < 0f)
            SetActiveSlot((activeSlotIndex + 1) % HOTBAR_COUNT);
    }

    // ── 핫바 슬롯 선택 ──────────────────────────────────────

    public void SetActiveSlot(int index)
    {
        if (index < 0 || index >= HOTBAR_COUNT) return;

        // 이전 슬롯 선택 해제
        hotbarSlotUIs[activeSlotIndex].SetSelected(false);

        activeSlotIndex = index;

        // 새 슬롯 선택
        hotbarSlotUIs[activeSlotIndex].SetSelected(true);
    }

    public int GetActiveSlotIndex() => activeSlotIndex;

    public InventorySlotData GetActiveSlotData() => slots[activeSlotIndex];

    // ── 인벤토리 토글 ───────────────────────────────────────

    void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(isInventoryOpen);

        if (isInventoryOpen)
            RefreshInventoryUI();
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

    // ── 아이템 추가/제거 (향후 사용) ──────────────────────────

    /// <summary>
    /// 아이템 추가. 기존 스택에 합산하고, 빈 슬롯에 배치.
    /// </summary>
    public bool AddItem(ItemData item, int count = 1)
    {
        // 1. 기존 스택 찾기 (같은 아이템 + 여유 있는 슬롯)
        for (int i = 0; i < TOTAL_SLOT_COUNT; i++)
        {
            if (!slots[i].IsEmpty && slots[i].item == item
                && slots[i].count < item.maxStackSize)
            {
                int canAdd = item.maxStackSize - slots[i].count;
                int add = Mathf.Min(canAdd, count);
                slots[i].count += add;
                count -= add;

                if (count <= 0)
                {
                    RefreshAll();
                    return true;
                }
            }
        }

        // 2. 빈 슬롯 찾기
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
                    return true;
                }
            }
        }

        // 인벤토리 가득 참
        RefreshAll();
        return false;
    }

    void RefreshAll()
    {
        RefreshHotbarUI();
        if (isInventoryOpen)
            RefreshInventoryUI();
    }
}
