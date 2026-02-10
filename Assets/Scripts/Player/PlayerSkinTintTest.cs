using UnityEngine;

/// <summary>
/// 그레이스케일 스프라이트에 색상 틴팅을 테스트하는 스크립트
/// </summary>
public class PlayerSkinTintTest : MonoBehaviour
{
    [Header("타겟 렌더러")]
    [Tooltip("색상을 적용할 스프라이트 렌더러")]
    public SpriteRenderer targetRenderer;
    
    [Header("피부 톤 프리셋")]
    [Tooltip("밝은 피부 톤")]
    public Color lightSkinTone = new Color(1f, 0.88f, 0.78f);      // 밝은 톤
    
    [Tooltip("보통 피부 톤")]
    public Color mediumSkinTone = new Color(0.8f, 0.65f, 0.54f);   // 보통 톤
    
    [Tooltip("어두운 피부 톤")]
    public Color darkSkinTone = new Color(0.55f, 0.42f, 0.35f);    // 어두운 톤
    
    [Header("현재 색상 (실시간 조정 가능)")]
    [Tooltip("현재 적용된 색상 - Inspector에서 실시간으로 조정 가능")]
    public Color currentColor = Color.white;
    
    [Header("추가 프리셋들")]
    public Color[] customPresets = new Color[]
    {
        new Color(0.95f, 0.76f, 0.65f),  // 연한 복숭아
        new Color(0.82f, 0.71f, 0.55f),  // 황토색
        new Color(0.72f, 0.57f, 0.47f),  // 갈색
        new Color(0.45f, 0.35f, 0.28f)   // 진한 갈색
    };

    void Start()
    {
        // 렌더러 자동 찾기
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (targetRenderer == null)
        {
            Debug.LogError("PlayerSkinTintTest: SpriteRenderer를 찾을 수 없습니다!");
            return;
        }
        
        // 현재 색상 적용
        ApplyColor(currentColor);
        
        Debug.Log("=== 피부 톤 테스트 시작 ===");
        Debug.Log("단축키:");
        Debug.Log("  1 키: 밝은 피부 톤");
        Debug.Log("  2 키: 보통 피부 톤");
        Debug.Log("  3 키: 어두운 피부 톤");
        Debug.Log("  4~7 키: 커스텀 프리셋");
        Debug.Log("  0 키: 원본 (흰색)");
        Debug.Log("Inspector에서 Current Color를 조정하면 실시간으로 변경됩니다!");
    }
    
    void Update()
    {
        // 단축키로 프리셋 적용
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ApplyColor(lightSkinTone);
            Debug.Log($"밝은 피부 톤 적용: RGB({lightSkinTone.r:F2}, {lightSkinTone.g:F2}, {lightSkinTone.b:F2})");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ApplyColor(mediumSkinTone);
            Debug.Log($"보통 피부 톤 적용: RGB({mediumSkinTone.r:F2}, {mediumSkinTone.g:F2}, {mediumSkinTone.b:F2})");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ApplyColor(darkSkinTone);
            Debug.Log($"어두운 피부 톤 적용: RGB({darkSkinTone.r:F2}, {darkSkinTone.g:F2}, {darkSkinTone.b:F2})");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            ApplyColor(Color.white);
            Debug.Log("원본 색상 (흰색) 적용");
        }
        
        // 커스텀 프리셋 (4~7번 키)
        for (int i = 0; i < customPresets.Length && i < 4; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha4 + i))
            {
                ApplyColor(customPresets[i]);
                Debug.Log($"커스텀 프리셋 {i + 1} 적용");
            }
        }
    }
    
    /// <summary>
    /// 색상을 스프라이트 렌더러에 적용
    /// </summary>
    public void ApplyColor(Color color)
    {
        currentColor = color;
        
        if (targetRenderer != null)
        {
            targetRenderer.color = color;
        }
        else
        {
            Debug.LogWarning("PlayerSkinTintTest: Target Renderer가 설정되지 않았습니다.");
        }
    }
    
    /// <summary>
    /// Inspector에서 값을 변경할 때 실시간으로 적용
    /// </summary>
    void OnValidate()
    {
        if (targetRenderer != null && Application.isPlaying)
        {
            targetRenderer.color = currentColor;
        }
    }
    
    /// <summary>
    /// 현재 색상의 HSV 값을 로그로 출력 (디버깅용)
    /// </summary>
    [ContextMenu("Print Current Color Info")]
    void PrintColorInfo()
    {
        float h, s, v;
        Color.RGBToHSV(currentColor, out h, out s, out v);
        
        Debug.Log("=== 현재 색상 정보 ===");
        Debug.Log($"RGB: ({currentColor.r:F3}, {currentColor.g:F3}, {currentColor.b:F3})");
        Debug.Log($"HSV: (H:{h:F3}, S:{s:F3}, V:{v:F3})");
        Debug.Log($"Hex: #{ColorUtility.ToHtmlStringRGB(currentColor)}");
    }
}
