using UnityEngine;
using System.Collections.Generic;

// ============================================================
// 인벤토리 슬롯 저장용 (JsonUtility용 직렬화)
// ============================================================
[System.Serializable]
public class InventorySlotSaveData
{
    public string itemName;  // 비어있으면 ""
    public int count;
}

// ============================================================
// 타일 셀 좌표 저장용 (Vector3Int 대체)
// ============================================================
[System.Serializable]
public class CellSaveData
{
    public int x, y, z;
}

// ============================================================
// 작물 저장용
// ============================================================
[System.Serializable]
public class CropSaveData
{
    public int cellX, cellY, cellZ;
    public string seedItemName;
    public int wateredDays;
    public int currentStage;
}

// ============================================================
// 슬롯 전체 저장 데이터 (JSON 1개 = 이 클래스 1개)
// ============================================================
[System.Serializable]
public class SaveData
{
    [Header("슬롯 정보")]
    public int slotIndex;               // 슬롯 번호 (0, 1, 2)
    public string farmName;             // 농장 이름
    public string lastSaveTime;         // 마지막 저장 시각 "yyyy-MM-dd HH:mm"
    public string createdTime;          // 생성 시각 (가장 오래된 슬롯 판별용)
    public float totalPlayTimeMinutes;  // 총 플레이 시간 (분 단위)

    [Header("하위 데이터")]
    public PlayerData playerData;
    public FarmData farmData;

    public SaveData()
    {
        playerData = new PlayerData();
        farmData = new FarmData();
    }
}

// ============================================================
// 플레이어 데이터
// ============================================================
[System.Serializable]
public class PlayerData
{
    [Header("기본 정보")]
    public string playerName;   // 플레이어 이름
    public int level;           // 플레이어 레벨

    [Header("위치")]
    public float positionX;
    public float positionY;

    [Header("시간/날짜")]
    public int currentYear;     // 년
    public int currentSequence; // 시퀀스 (0=Lumae, 1=Velum, 2=Fracta)
    public int currentDay;      // 일
    public float gameMinutes;   // 하루 내 시간 (분)

    [Header("인벤토리")]
    public InventorySlotSaveData[] inventorySlots;

    [Header("스테미나")]
    public int currentStamina = 100;
    public bool wasExhausted;

    [Header("골드")]
    public int gold;

    [Header("커스터마이징")]
    public int skinTonePreset;  // 피부 톤 프리셋 인덱스
    public float skinColorR;
    public float skinColorG;
    public float skinColorB;

    public PlayerData()
    {
        playerName     = "플레이어";
        level          = 1;
        positionX      = 0f;
        positionY      = 0f;
        currentYear    = 1;
        currentSequence = 0;
        currentDay     = 1;
        gameMinutes    = 360f;  // 06:00
        inventorySlots = new InventorySlotSaveData[27];
        skinTonePreset = 1;
        skinColorR     = PlayerCustomizationData.SKIN_TONE_MEDIUM.r;
        skinColorG     = PlayerCustomizationData.SKIN_TONE_MEDIUM.g;
        skinColorB     = PlayerCustomizationData.SKIN_TONE_MEDIUM.b;
    }

    // PlayerData → PlayerCustomizationData 변환 (씬 간 전달용)
    public PlayerCustomizationData ToCustomizationData()
    {
        var data = new PlayerCustomizationData();
        data.playerName     = playerName;
        data.skinTonePreset = skinTonePreset;
        data.skinColor      = new Color(skinColorR, skinColorG, skinColorB, 1f);
        return data;
    }

    // PlayerCustomizationData → PlayerData에 반영
    public void ApplyCustomization(PlayerCustomizationData customData)
    {
        playerName     = customData.playerName;
        skinTonePreset = customData.skinTonePreset;
        skinColorR     = customData.skinColor.r;
        skinColorG     = customData.skinColor.g;
        skinColorB     = customData.skinColor.b;
    }
}

// ============================================================
// 농장 데이터
// ============================================================
[System.Serializable]
public class FarmData
{
    [Header("갈린 흙 타일 좌표")]
    public List<CellSaveData> tilledCells = new List<CellSaveData>();

    [Header("물 준 흙 타일 좌표")]
    public List<CellSaveData> wateredCells = new List<CellSaveData>();

    [Header("작물")]
    public List<CropSaveData> crops = new List<CropSaveData>();
}
