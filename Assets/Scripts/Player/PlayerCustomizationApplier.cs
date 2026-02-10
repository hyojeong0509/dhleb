using UnityEngine;

/// <summary>
/// Game 씬에서 플레이어에 커스터마이징을 적용하는 스크립트
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerCustomizationApplier : MonoBehaviour
{
    [Header("커스터마이징 적용 대상")]
    [Tooltip("피부색을 적용할 스프라이트 렌더러 (비어있으면 메인 렌더러 사용)")]
    public SpriteRenderer skinRenderer;
    
    [Tooltip("머리색을 적용할 스프라이트 렌더러 (향후 Multi-Layer용)")]
    public SpriteRenderer hairRenderer;
    
    [Tooltip("옷 색상을 적용할 스프라이트 렌더러 (향후 Multi-Layer용)")]
    public SpriteRenderer outfitRenderer;

    [Header("설정")]
    [Tooltip("시작 시 자동으로 커스터마이징 적용")]
    public bool applyOnStart = true;
    
    [Header("디버그")]
    [Tooltip("적용된 색상 정보를 로그로 출력")]
    public bool showDebugLogs = true;

    private SpriteRenderer mainRenderer;

    void Start()
    {
        mainRenderer = GetComponent<SpriteRenderer>();

        // 커스터마이징 자동 적용
        if (applyOnStart)
        {
            ApplyCustomization();
        }
    }

    /// <summary>
    /// 저장된 커스터마이징 데이터를 플레이어에 적용
    /// </summary>
    public void ApplyCustomization()
    {
        if (GameDataManager.Instance == null || GameDataManager.Instance.currentCustomization == null)
        {
            Debug.LogWarning("PlayerCustomizationApplier: 커스터마이징 데이터가 없습니다. 기본값을 사용합니다.");
            
            // 기본값 적용
            if (mainRenderer != null)
            {
                mainRenderer.color = PlayerCustomizationData.SKIN_TONE_MEDIUM;
            }
            return;
        }

        PlayerCustomizationData data = GameDataManager.Instance.currentCustomization;

        // ===== 단일 레이어 모드 (현재) =====
        // skinRenderer가 비어있으면 메인 렌더러에 피부색 적용
        if (skinRenderer == null)
        {
            if (mainRenderer != null)
            {
                mainRenderer.color = data.skinColor;
                
                if (showDebugLogs)
                {
                    Debug.Log($"[PlayerCustomizationApplier] 피부 톤 적용 완료");
                    Debug.Log($"  - 플레이어 이름: {data.playerName}");
                    Debug.Log($"  - 피부색: RGB({data.skinColor.r:F2}, {data.skinColor.g:F2}, {data.skinColor.b:F2})");
                    Debug.Log($"  - 프리셋: {GetPresetName(data.skinTonePreset)}");
                }
            }
        }
        // ===== 멀티 레이어 모드 (향후) =====
        else
        {
            ApplyColor(data.skinColor, skinRenderer);
            ApplyColor(data.hairColor, hairRenderer);
            ApplyColor(data.outfitColor, outfitRenderer);
            
            if (showDebugLogs)
            {
                Debug.Log($"[PlayerCustomizationApplier] Multi-Layer 커스터마이징 적용 완료 - {data.playerName}");
            }
        }
    }

    void ApplyColor(Color color, SpriteRenderer renderer)
    {
        if (renderer != null)
        {
            renderer.color = color;
        }
    }

    /// <summary>
    /// 특정 커스터마이징 데이터를 적용
    /// </summary>
    public void ApplyCustomization(PlayerCustomizationData data)
    {
        if (data == null)
        {
            Debug.LogWarning("PlayerCustomizationApplier: 적용할 커스터마이징 데이터가 null입니다.");
            return;
        }

        // 단일 레이어 모드
        if (skinRenderer == null)
        {
            if (mainRenderer != null)
            {
                mainRenderer.color = data.skinColor;
            }
        }
        // 멀티 레이어 모드
        else
        {
            ApplyColor(data.skinColor, skinRenderer);
            ApplyColor(data.hairColor, hairRenderer);
            ApplyColor(data.outfitColor, outfitRenderer);
        }
    }
    
    /// <summary>
    /// 프리셋 인덱스를 이름으로 변환 (디버그용)
    /// </summary>
    private string GetPresetName(int preset)
    {
        switch (preset)
        {
            case 0: return "밝은 톤";
            case 1: return "보통 톤";
            case 2: return "어두운 톤";
            case 3: return "창백한 톤";
            case 4: return "그을린 톤";
            case 5: return "짙은 톤";
            case -1: return "커스텀 색상";
            default: return "알 수 없음";
        }
    }
    
    /// <summary>
    /// 현재 적용된 색상을 즉시 확인 (테스트용)
    /// </summary>
    [ContextMenu("Show Current Color")]
    void ShowCurrentColor()
    {
        if (mainRenderer != null)
        {
            Color c = mainRenderer.color;
            Debug.Log($"현재 적용된 색상: RGB({c.r:F2}, {c.g:F2}, {c.b:F2})");
        }
    }
}





