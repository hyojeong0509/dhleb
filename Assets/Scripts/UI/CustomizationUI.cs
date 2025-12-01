using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Title 씬에서 커스터마이징 UI를 관리하는 컨트롤러
/// </summary>
public class CustomizationUI : MonoBehaviour
{
    [Header("UI 레퍼런스")]
    [Tooltip("플레이어 이름 입력 필드")]
    public TMP_InputField nameInputField;
    
    [Header("컬러 피커")]
    [Tooltip("피부색 선택 버튼")]
    public Button skinColorButton;
    [Tooltip("머리색 선택 버튼")]
    public Button hairColorButton;
    [Tooltip("옷 색상 선택 버튼")]
    public Button outfitColorButton;
    
    [Header("프리뷰")]
    [Tooltip("커스터마이징 프리뷰 이미지/스프라이트 렌더러")]
    public SpriteRenderer previewRenderer;
    public Image previewImage; // Image 컴포넌트 사용 시
    
    [Header("액션 버튼")]
    [Tooltip("게임 시작 버튼")]
    public Button startButton;
    [Tooltip("취소 버튼 (옵션)")]
    public Button cancelButton;

    [Header("색상 선택 UI")]
    [Tooltip("색상 선택 패널")]
    public GameObject colorPickerPanel;
    [Tooltip("색상 선택 슬라이더 또는 피커")]
    public Slider colorR, colorG, colorB;

    private PlayerCustomizationData customizationData;
    private Color currentEditColor;
    private System.Action<Color> onColorSelected;

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
    }

    void InitializeUI()
    {
        // 이름 입력 필드 초기화
        if (nameInputField != null)
        {
            nameInputField.text = customizationData.playerName;
            nameInputField.onValueChanged.AddListener(OnNameChanged);
        }

        // 색상 버튼 초기화
        UpdateColorButton(skinColorButton, customizationData.skinColor);
        UpdateColorButton(hairColorButton, customizationData.hairColor);
        UpdateColorButton(outfitColorButton, customizationData.outfitColor);
    }

    void SetupButtons()
    {
        // 색상 선택 버튼
        if (skinColorButton != null)
        {
            skinColorButton.onClick.AddListener(() => OpenColorPicker(customizationData.skinColor, 
                (color) => {
                    customizationData.skinColor = color;
                    UpdateColorButton(skinColorButton, color);
                    UpdatePreview();
                }));
        }
        
        if (hairColorButton != null)
        {
            hairColorButton.onClick.AddListener(() => OpenColorPicker(customizationData.hairColor, 
                (color) => {
                    customizationData.hairColor = color;
                    UpdateColorButton(hairColorButton, color);
                    UpdatePreview();
                }));
        }
        
        if (outfitColorButton != null)
        {
            outfitColorButton.onClick.AddListener(() => OpenColorPicker(customizationData.outfitColor, 
                (color) => {
                    customizationData.outfitColor = color;
                    UpdateColorButton(outfitColorButton, color);
                    UpdatePreview();
                }));
        }

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

    void OnNameChanged(string newName)
    {
        if (customizationData != null)
        {
            customizationData.playerName = string.IsNullOrEmpty(newName) ? "플레이어" : newName;
        }
    }

    void UpdateColorButton(Button button, Color color)
    {
        if (button != null)
        {
            // 버튼의 색상 업데이트 (Image 컴포넌트가 있다면)
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = color;
            }
        }
    }

    void OpenColorPicker(Color currentColor, System.Action<Color> onSelected)
    {
        currentEditColor = currentColor;
        onColorSelected = onSelected;

        // 색상 선택 패널이 있으면 열기
        if (colorPickerPanel != null)
        {
            colorPickerPanel.SetActive(true);
            
            // 슬라이더 값 설정
            if (colorR != null) colorR.value = currentColor.r;
            if (colorG != null) colorG.value = currentColor.g;
            if (colorB != null) colorB.value = currentColor.b;
            
            // 슬라이더 이벤트 연결
            if (colorR != null) colorR.onValueChanged.RemoveAllListeners();
            if (colorG != null) colorG.onValueChanged.RemoveAllListeners();
            if (colorB != null) colorB.onValueChanged.RemoveAllListeners();
            
            if (colorR != null) colorR.onValueChanged.AddListener(OnColorSliderChanged);
            if (colorG != null) colorG.onValueChanged.AddListener(OnColorSliderChanged);
            if (colorB != null) colorB.onValueChanged.AddListener(OnColorSliderChanged);
        }
        else
        {
            // 색상 선택 패널이 없으면 기본 색상 선택 사용 (간단한 방법)
            // 실제로는 더 복잡한 색상 피커 UI를 구현할 수 있습니다
            Debug.LogWarning("CustomizationUI: 색상 선택 패널이 설정되지 않았습니다.");
        }
    }

    void OnColorSliderChanged(float value)
    {
        if (colorR != null && colorG != null && colorB != null)
        {
            currentEditColor = new Color(colorR.value, colorG.value, colorB.value, 1f);
            
            // 프리뷰 업데이트
            UpdatePreview();
        }
    }

    public void ApplyColor()
    {
        if (onColorSelected != null)
        {
            onColorSelected(currentEditColor);
        }
        
        if (colorPickerPanel != null)
        {
            colorPickerPanel.SetActive(false);
        }
    }

    public void CancelColorPicker()
    {
        if (colorPickerPanel != null)
        {
            colorPickerPanel.SetActive(false);
        }
    }

    void UpdatePreview()
    {
        // 프리뷰 이미지 업데이트 (실제 구현은 프로젝트에 맞게)
        if (previewRenderer != null)
        {
            // SpriteRenderer를 사용한 프리뷰 업데이트
            // 예: 색상 적용 로직
        }
        
        if (previewImage != null)
        {
            // Image를 사용한 프리뷰 업데이트
            // 예: 색상 적용 로직
        }
    }

    void OnStartGame()
    {
        // 커스터마이징 데이터 저장
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.SaveCustomization(customizationData);
            GameDataManager.Instance.LoadGameScene();
        }
        else
        {
            Debug.LogError("CustomizationUI: GameDataManager를 찾을 수 없습니다!");
        }
    }

    void OnCancel()
    {
        // 취소 시 Title 씬의 메인 메뉴로 돌아가기
        // 또는 커스터마이징 UI 비활성화
        gameObject.SetActive(false);
    }
}





