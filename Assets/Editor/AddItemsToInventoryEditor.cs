#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Tools 메뉴에서 인벤토리에 아이템을 쉽게 추가할 수 있는 에디터 창
/// 저장 파일에 직접 적용하거나, 플레이 모드에서 즉시 추가 가능
/// </summary>
public class AddItemsToInventoryEditor : EditorWindow
{
    const string SAVE_FOLDER = "Saves";
    const string SAVE_FILE_PREFIX = "Slot_";
    const string SAVE_FILE_EXT = ".json";
    const int MAX_SLOTS = 3;
    const int INVENTORY_SLOT_COUNT = 27;

    [System.Serializable]
    class ItemEntry
    {
        public ItemData item;
        public int count = 1;
    }

    List<ItemEntry> entries = new List<ItemEntry>();
    Vector2 scrollPos;
    int targetSlotIndex = 0;
    ItemDatabase itemDb;

    [MenuItem("Tools/인벤토리 아이템 추가")]
    static void Open()
    {
        var w = GetWindow<AddItemsToInventoryEditor>("인벤토리 아이템 추가");
        w.minSize = new Vector2(360, 320);
    }

    void OnEnable()
    {
        itemDb = Resources.Load<ItemDatabase>("ItemDatabase");
        if (entries.Count == 0)
            entries.Add(new ItemEntry());
    }

    void OnGUI()
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("인벤토리에 추가할 아이템", EditorStyles.boldLabel);

        // 대상 슬롯
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("대상 저장 슬롯", GUILayout.Width(100));
        targetSlotIndex = EditorGUILayout.IntPopup(targetSlotIndex,
            new[] { "슬롯 0", "슬롯 1", "슬롯 2" },
            new[] { 0, 1, 2 });
        EditorGUILayout.EndHorizontal();

        if (itemDb == null)
        {
            EditorGUILayout.HelpBox("ItemDatabase를 찾을 수 없습니다.\nAssets/Resources/ItemDatabase.asset 가 있는지 확인하세요.", MessageType.Warning);
            return;
        }

        EditorGUILayout.Space(4);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));

        for (int i = 0; i < entries.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            entries[i].item = (ItemData)EditorGUILayout.ObjectField(entries[i].item, typeof(ItemData), false, GUILayout.Width(180));
            entries[i].count = Mathf.Max(1, EditorGUILayout.IntField(entries[i].count, GUILayout.Width(50)));

            if (GUILayout.Button("×", GUILayout.Width(24)) && entries.Count > 1)
            {
                entries.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("+ 아이템 행 추가"))
            entries.Add(new ItemEntry());

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(8);

        // 저장 파일에 적용
        if (GUILayout.Button("저장 파일에 적용 (다음 로드 시 반영)", GUILayout.Height(28)))
        {
            ApplyToSaveFile();
        }

        // 플레이 모드에서 주변에 드롭
        EditorGUI.BeginDisabledGroup(!Application.isPlaying);
        if (GUILayout.Button("플레이 중일 때 주변에 아이템 드롭", GUILayout.Height(24)))
        {
            SpawnItemsAroundPlayer();
        }
        if (!Application.isPlaying)
            EditorGUILayout.HelpBox("플레이 모드에서만 사용 가능합니다.", MessageType.Info);
        EditorGUI.EndDisabledGroup();
    }

    void ApplyToSaveFile()
    {
        string folder = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
        string path = Path.Combine(folder, $"{SAVE_FILE_PREFIX}{targetSlotIndex}{SAVE_FILE_EXT}");

        SaveData data;
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<SaveData>(json);
        }
        else
        {
            data = CreateMinimalSaveData();
        }

        if (data.playerData.inventorySlots == null || data.playerData.inventorySlots.Length != INVENTORY_SLOT_COUNT)
            data.playerData.inventorySlots = new InventorySlotSaveData[INVENTORY_SLOT_COUNT];

        // 기존 슬롯을 리스트로 변환 (병합 로직용)
        var slots = new List<InventorySlotSaveData>(data.playerData.inventorySlots);

        foreach (var e in entries)
        {
            if (e.item == null || e.count <= 0) continue;

            int remaining = e.count;
            int maxStack = Mathf.Max(1, e.item.maxStackSize);

            // 1) 기존 같은 아이템 슬롯에 합산
            for (int i = 0; i < slots.Count && remaining > 0; i++)
            {
                if (string.IsNullOrEmpty(slots[i].itemName)) continue;
                if (slots[i].itemName != e.item.itemName) continue;

                int canAdd = maxStack - slots[i].count;
                if (canAdd <= 0) continue;

                int add = Mathf.Min(canAdd, remaining);
                slots[i].count += add;
                remaining -= add;
            }

            // 2) 빈 슬롯에 배치
            while (remaining > 0)
            {
                int emptyIdx = -1;
                for (int i = 0; i < slots.Count; i++)
                {
                    if (string.IsNullOrEmpty(slots[i].itemName) || slots[i].count <= 0)
                    {
                        emptyIdx = i;
                        break;
                    }
                }
                if (emptyIdx < 0) break;

                int put = Mathf.Min(maxStack, remaining);
                slots[emptyIdx].itemName = e.item.itemName;
                slots[emptyIdx].count = put;
                remaining -= put;
            }
        }

        data.playerData.inventorySlots = slots.ToArray();
        data.slotIndex = targetSlotIndex;
        data.lastSaveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string outJson = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, outJson);

        Debug.Log($"[인벤토리 추가] 슬롯 {targetSlotIndex} 저장 파일에 아이템 적용 완료 → {path}");
        EditorUtility.DisplayDialog("완료", $"슬롯 {targetSlotIndex} 저장 파일에 아이템이 적용되었습니다.\n다음에 게임을 로드하면 인벤토리에 반영됩니다.", "확인");
    }

    SaveData CreateMinimalSaveData()
    {
        var data = new SaveData();
        data.slotIndex = targetSlotIndex;
        data.farmName = "테스트 농장";
        data.createdTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        data.lastSaveTime = data.createdTime;
        data.totalPlayTimeMinutes = 0;
        data.playerData = new PlayerData();
        data.playerData.playerName = "플레이어";
        data.playerData.currentYear = 1;
        data.playerData.currentSequence = 0;
        data.playerData.currentDay = 1;
        data.playerData.gameMinutes = 360f;
        data.playerData.inventorySlots = new InventorySlotSaveData[INVENTORY_SLOT_COUNT];
        for (int i = 0; i < INVENTORY_SLOT_COUNT; i++)
            data.playerData.inventorySlots[i] = new InventorySlotSaveData { itemName = "", count = 0 };
        data.farmData = new FarmData();
        data.gameProgressData = new GameProgressData();
        return data;
    }

    void SpawnItemsAroundPlayer()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("오류", "플레이 모드에서만 사용할 수 있습니다.", "확인");
            return;
        }

        var inv = InventoryManager.Instance;
        if (inv == null)
        {
            EditorUtility.DisplayDialog("오류", "InventoryManager를 찾을 수 없습니다. 게임 씬이 로드된 상태인지 확인하세요.", "확인");
            return;
        }

        if (inv.droppedItemPrefab == null || inv.playerTransform == null)
        {
            EditorUtility.DisplayDialog("오류", "InventoryManager에 droppedItemPrefab 또는 playerTransform이 설정되지 않았습니다.", "확인");
            return;
        }

        int spawned = 0;
        foreach (var e in entries)
        {
            if (e.item == null || e.count <= 0) continue;

            Vector3 dropPos = inv.playerTransform.position + (Vector3)Random.insideUnitCircle.normalized * 1.5f;
            GameObject dropped = Object.Instantiate(inv.droppedItemPrefab, dropPos, Quaternion.identity);
            var pickup = dropped.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                pickup.itemData = e.item;
                pickup.count = e.count;
                pickup.SetupVisual();
            }
            spawned++;
        }

        Debug.Log($"[인벤토리 추가] 플레이어 주변에 {spawned}종류 아이템 드롭 완료");
        EditorUtility.DisplayDialog("완료", "플레이어 주변에 아이템이 드롭되었습니다.", "확인");
    }
}
#endif
