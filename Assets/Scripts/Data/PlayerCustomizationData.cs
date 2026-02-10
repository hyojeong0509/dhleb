using UnityEngine;

[System.Serializable]
public class PlayerCustomizationData
{
    // ========== 피부 톤 프리셋 ==========
    // 미리 정의된 피부 톤 색상들 (그레이스케일 스프라이트에 적용)
    public static readonly Color SKIN_TONE_LIGHT = new Color(1f, 0.88f, 0.78f);      // 밝은 피부
    public static readonly Color SKIN_TONE_MEDIUM = new Color(0.8f, 0.65f, 0.54f);   // 보통 피부
    public static readonly Color SKIN_TONE_DARK = new Color(0.55f, 0.42f, 0.35f);    // 어두운 피부
    
    // 추가 프리셋들 (원하면 더 추가 가능)
    public static readonly Color SKIN_TONE_PALE = new Color(0.95f, 0.76f, 0.65f);    // 창백한
    public static readonly Color SKIN_TONE_TAN = new Color(0.72f, 0.57f, 0.47f);     // 그을린
    public static readonly Color SKIN_TONE_DEEP = new Color(0.45f, 0.35f, 0.28f);    // 짙은
    
    [Header("기본 정보")]
    public string playerName = "플레이어";
    
    [Header("캐릭터 외형")]
    public Color skinColor = SKIN_TONE_MEDIUM; // 기본값: 보통 피부 톤
    public Color hairColor = Color.white;      // 향후 그레이스케일 머리카락용
    public Color outfitColor = Color.white;    // 향후 그레이스케일 옷용
    
    [Header("설정")]
    public int skinTonePreset = 1; // 0=밝은, 1=보통, 2=어두운 (UI에서 사용)
    
    // 기본값으로 초기화
    public PlayerCustomizationData()
    {
        playerName = "플레이어";
        skinColor = SKIN_TONE_MEDIUM; // 보통 톤으로 시작
        hairColor = Color.white;
        outfitColor = Color.white;
        skinTonePreset = 1; // 보통 톤
    }
    
    // 복사 생성자
    public PlayerCustomizationData(PlayerCustomizationData other)
    {
        this.playerName = other.playerName;
        this.skinColor = other.skinColor;
        this.hairColor = other.hairColor;
        this.outfitColor = other.outfitColor;
        this.skinTonePreset = other.skinTonePreset;
    }
    
    // ========== 헬퍼 메서드 ==========
    
    /// <summary>
    /// 프리셋 인덱스로 피부 톤 설정
    /// </summary>
    public void SetSkinToneByPreset(int presetIndex)
    {
        skinTonePreset = presetIndex;
        
        switch (presetIndex)
        {
            case 0:
                skinColor = SKIN_TONE_LIGHT;
                break;
            case 1:
                skinColor = SKIN_TONE_MEDIUM;
                break;
            case 2:
                skinColor = SKIN_TONE_DARK;
                break;
            case 3:
                skinColor = SKIN_TONE_PALE;
                break;
            case 4:
                skinColor = SKIN_TONE_TAN;
                break;
            case 5:
                skinColor = SKIN_TONE_DEEP;
                break;
            default:
                skinColor = SKIN_TONE_MEDIUM;
                break;
        }
    }
    
    /// <summary>
    /// 커스텀 색상으로 피부 톤 설정
    /// </summary>
    public void SetCustomSkinColor(Color color)
    {
        skinColor = color;
        skinTonePreset = -1; // 커스텀 색상 사용 중
    }
}





