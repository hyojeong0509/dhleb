using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Title 씬에서 커스터마이징 UI를 관리하는 컨트롤러
/// </summary>
public class CustomizationUI : MonoBehaviour
{
    [Header("기본 정보")]
    [Tooltip("플레이어 이름 입력 필드")]
    public TMP_InputField nameInputField;
    
    [Header("피부 톤 프리셋 버튼")]
    [Tooltip("밝은 피부 톤 버튼")]
    public Button lightSkinButton;
    [Tooltip("보통 피부 톤 버튼")]
    public Button mediumSkinButton;
    [Tooltip("어두운 피부 톤 버튼")]
    public Button darkSkinButton;
    
    [Header("추가 프리셋 (선택)")]
    [Tooltip("창백한 피부 톤 버튼")]
    public Button paleSkinButton;
    [Tooltip("그을린 피부 톤 버튼")]
    public Button tanSkinButton;
    [Tooltip("짙은 피부 톤 버튼")]
    public Button deepSkinButton;
    
    [Header("프리뷰")]
    [Tooltip("커스터마이징 프리뷰 스프라이트 렌더러")]
    public SpriteRenderer previewRenderer;
    [Tooltip("커스터마이징 프리뷰 이미지 (UI)")]
    public Image previewImage;
    
    [Header("선택 표시")]
    [Tooltip("현재 선택된 버튼을 표시할 이미지들 (선택 테두리 등)")]
    public Image[] selectionIndicators; // 각 버튼에 대응되는 선택 표시
    
    [Header("액션 버튼")]
    [Tooltip("게임 시작 버튼")]
    public Button startButton;
    [Tooltip("취소 버튼 (옵션)")]
    public Button cancelButton;
    
    [Header("디버그")]
    [Tooltip("선택된 피부 톤 정보 표시 텍스트")]
    public TMP_Text debugText;

    private PlayerCustomizationData customizationData;
    private int currentPresetIndex = 1; // 기본값: 보통 톤

    void Start()
    {
        // GameDataManager에서 데이터 가져오기
        if (GameDataManager.Instance != null)
        {
            customizationData = new PlayerCustomizationData(GameDataManager.Instance.currentCustomization);
        }
        else
        {
            customizationData = new PlayerCustomizationData();
        }

        // UI 초기화
        InitializeUI();
        
        // 버튼 이벤트 연결
        SetupButtons();
        
        // 초기 프리뷰 업데이트
        UpdatePreview();
        UpdateSelectionIndicator();
    }

    void InitializeUI()
    {
        // 이름 입력 필드 초기화
        if (nameInputField != null)
        {
            nameInputField.text = customizationData.playerName;
            nameInputField.onValueChanged.AddListener(OnNameChanged);
        }

        // 버튼 색상 설정 (버튼 자체를 해당 피부색으로)
        SetButtonColor(lightSkinButton, PlayerCustomizationData.SKIN_TONE_LIGHT);
        SetButtonColor(mediumSkinButton, PlayerCustomizationData.SKIN_TONE_MEDIUM);
        SetButtonColor(darkSkinButton, PlayerCustomizationData.SKIN_TONE_DARK);
        SetButtonColor(paleSkinButton, PlayerCustomizationData.SKIN_TONE_PALE);
        SetButtonColor(tanSkinButton, PlayerCustomizationData.SKIN_TONE_TAN);
        SetButtonColor(deepSkinButton, PlayerCustomizationData.SKIN_TONE_DEEP);
        
        // 현재 선택된 프리셋 설정
        currentPresetIndex = customizationData.skinTonePreset;
    }

    void SetupButtons()
    {
        // 피부 톤 프리셋 버튼
        if (lightSkinButton != null)
            lightSkinButton.onClick.AddListener(() => SelectSkinTone(0));
        
        if (mediumSkinButton != null)
            mediumSkinButton.onClick.AddListener(() => SelectSkinTone(1));
        
        if (darkSkinButton != null)
            darkSkinButton.onClick.AddListener(() => SelectSkinTone(2));
        
        if (paleSkinButton != null)
            paleSkinButton.onClick.AddListener(() => SelectSkinTone(3));
        
        if (tanSkinButton != null)
            tanSkinButton.onClick.AddListener(() => SelectSkinTone(4));
        
        if (deepSkinButton != null)
            deepSkinButton.onClick.AddListener(() => SelectSkinTone(5));

        // 게임 시작 버튼
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartGame);
        }

        // 취소 버튼
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancel);
        }
    }

    /// <summary>
    /// 피부 톤 선택
    /// </summary>
    void SelectSkinTone(int presetIndex)
    {
        currentPresetIndex = presetIndex;
        customizationData.SetSkinToneByPreset(presetIndex);
        
        // UI 업데이트
        UpdatePreview();
        UpdateSelectionIndicator();
        UpdateDebugText();
        
        Debug.Log($"[CustomizationUI] 피부 톤 선택: {GetPresetName(presetIndex)}");
    }

    void OnNameChanged(string newName)
    {
        if (customizationData != null)
        {
            customizationData.playerName = string.IsNullOrEmpty(newName) ? "플레이어" : newName;
        }
    }

    /// <summary>
    /// 버튼 색상 설정 (버튼을 해당 피부색으로 표시)
    /// </summary>
    void SetButtonColor(Button button, Color color)
    {
        if (button != null)
        {
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = color;
            }
        }
    }

    /// <summary>
    /// 프리뷰 업데이트
    /// </summary>
    void UpdatePreview()
    {
        // SpriteRenderer 프리뷰 (3D 오브젝트)
        if (previewRenderer != null)
        {
            previewRenderer.color = customizationData.skinColor;
        }
        
        // UI Image 프리뷰
        if (previewImage != null)
        {
            previewImage.color = customizationData.skinColor;
        }
    }

    /// <summary>
    /// 선택 표시 업데이트 (현재 선택된 버튼 강조)
    /// </summary>
    void UpdateSelectionIndicator()
    {
        if (selectionIndicators == null || selectionIndicators.Length == 0)
            return;

        // 모든 인디케이터 비활성화
        for (int i = 0; i < selectionIndicators.Length; i++)
        {
            if (selectionIndicators[i] != null)
            {
                selectionIndicators[i].enabled = (i == currentPresetIndex);
            }
        }
    }

    /// <summary>
    /// 디버그 텍스트 업데이트
    /// </summary>
    void UpdateDebugText()
    {
        if (debugText != null)
        {
            Color c = customizationData.skinColor;
            debugText.text = $"선택: {GetPresetName(currentPresetIndex)}\n" +
                           $"RGB({c.r:F2}, {c.g:F2}, {c.b:F2})";
        }
    }

    /// <summary>
    /// 프리셋 이름 가져오기
    /// </summary>
    string GetPresetName(int preset)
    {
        switch (preset)
        {
            case 0: return "밝은 톤";
            case 1: return "보통 톤";
            case 2: return "어두운 톤";
            case 3: return "창백한 톤";
            case 4: return "그을린 톤";
            case 5: return "짙은 톤";
            default: return "알 수 없음";
        }
    }

    /// <summary>
    /// 게임 시작 버튼 클릭
    /// </summary>
    void OnStartGame()
    {
        // 커스터마이징 데이터 저장
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.SaveCustomization(customizationData);
            
            Debug.Log($"[CustomizationUI] 커스터마이징 저장 완료!");
            Debug.Log($"  - 이름: {customizationData.playerName}");
            Debug.Log($"  - 피부 톤: {GetPresetName(customizationData.skinTonePreset)}");
            Debug.Log($"  - 색상: RGB({customizationData.skinColor.r:F2}, {customizationData.skinColor.g:F2}, {customizationData.skinColor.b:F2})");
            
            // Game 씬으로 이동
            GameDataManager.Instance.LoadGameScene();
        }
        else
        {
            Debug.LogError("[CustomizationUI] GameDataManager를 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// 취소 버튼 클릭
    /// </summary>
    void OnCancel()
    {
        // 취소 시 Title 씬의 메인 메뉴로 돌아가기
        // 또는 커스터마이징 UI 비활성화
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 테스트용: 모든 프리셋 색상 출력
    /// </summary>
    [ContextMenu("Print All Presets")]
    void PrintAllPresets()
    {
        Debug.Log("===== 피부 톤 프리셋 =====");
        Debug.Log($"0. 밝은 톤: {PlayerCustomizationData.SKIN_TONE_LIGHT}");
        Debug.Log($"1. 보통 톤: {PlayerCustomizationData.SKIN_TONE_MEDIUM}");
        Debug.Log($"2. 어두운 톤: {PlayerCustomizationData.SKIN_TONE_DARK}");
        Debug.Log($"3. 창백한 톤: {PlayerCustomizationData.SKIN_TONE_PALE}");
        Debug.Log($"4. 그을린 톤: {PlayerCustomizationData.SKIN_TONE_TAN}");
        Debug.Log($"5. 짙은 톤: {PlayerCustomizationData.SKIN_TONE_DEEP}");
    }
}





