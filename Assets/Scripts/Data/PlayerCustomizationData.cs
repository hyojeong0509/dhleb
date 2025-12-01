using UnityEngine;

[System.Serializable]
public class PlayerCustomizationData
{
    [Header("기본 정보")]
    public string playerName = "플레이어";
    
    [Header("캐릭터 외형")]
    public Color skinColor = Color.white;
    public Color hairColor = Color.white;
    public Color outfitColor = Color.white;
    
    [Header("설정")]
    public int characterPreset = 0; // 캐릭터 프리셋 인덱스 (추후 확장 가능)
    
    // 기본값으로 초기화
    public PlayerCustomizationData()
    {
        playerName = "플레이어";
        skinColor = Color.white;
        hairColor = Color.white;
        outfitColor = Color.white;
        characterPreset = 0;
    }
    
    // 복사 생성자
    public PlayerCustomizationData(PlayerCustomizationData other)
    {
        this.playerName = other.playerName;
        this.skinColor = other.skinColor;
        this.hairColor = other.hairColor;
        this.outfitColor = other.outfitColor;
        this.characterPreset = other.characterPreset;
    }
}





