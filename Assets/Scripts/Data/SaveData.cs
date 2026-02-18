using UnityEngine;

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
    public int currentDay;      // 게임 내 날짜

    [Header("위치")]
    public float positionX;
    public float positionY;

    [Header("커스터마이징")]
    public int skinTonePreset;  // 피부 톤 프리셋 인덱스
    public float skinColorR;
    public float skinColorG;
    public float skinColorB;

    public PlayerData()
    {
        playerName  = "플레이어";
        level       = 1;
        currentDay  = 1;
        positionX   = 0f;
        positionY   = 0f;
        skinTonePreset = 1; // 기본: 보통 톤
        skinColorR  = PlayerCustomizationData.SKIN_TONE_MEDIUM.r;
        skinColorG  = PlayerCustomizationData.SKIN_TONE_MEDIUM.g;
        skinColorB  = PlayerCustomizationData.SKIN_TONE_MEDIUM.b;
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
// 농장 데이터 (현재는 빈 껍데기, 농장 시스템 생기면 여기에 추가)
// ============================================================
[System.Serializable]
public class FarmData
{
    // 향후 농장 시스템 생기면 여기에 추가
    // 예: public List<CropData> crops;
    //     public int farmLevel;
    //     public int gold; 등
}
