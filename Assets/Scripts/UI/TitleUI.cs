using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TitleUI : MonoBehaviour
{
    // ================================================================
    // 메인 버튼
    // ================================================================
    [Header("메인 버튼")]
    public Button startBtn;
    public Button newBtn;
    public Button loadBtn;
    public Button settingBtn;
    public Button exitBtn;

    // ================================================================
    // 패널
    // ================================================================
    [Header("패널")]
    public GameObject pnNewGame;
    public GameObject pnSlotSelect;
    public GameObject pnWarning;

    // ================================================================
    // Pn_NewGame 요소
    // ================================================================
    [Header("새 게임 패널")]
    public TMP_InputField inputFarmName;
    public TMP_InputField inputPlayerName;
    public Button btnSkinLight;
    public Button btnSkinMedium;
    public Button btnSkinDark;
    public Button btnCreate;
    public Button btnNewCancel;

    // ================================================================
    // Pn_SlotSelect 요소 (슬롯 3개)
    // ================================================================
    [Header("슬롯 선택 패널")]
    public TMP_Text[] txtFarmNames;    // 농장 이름 x3
    public TMP_Text[] txtPlayerNames;  // 플레이어 이름 x3
    public TMP_Text[] txtPlayTimes;    // 플레이 시간 x3
    public TMP_Text[] txtLastSaves;    // 마지막 저장 x3
    public Button[] btnSlotStarts;     // 시작 버튼 x3
    public Button[] btnSlotDeletes;    // 삭제 버튼 x3
    public Button btnSlotClose;

    // ================================================================
    // Pn_Warning 요소
    // ================================================================
    [Header("경고 패널")]
    public TMP_Text txtWarning;
    public Button btnWarningConfirm;
    public Button btnWarningCancel;

    // ================================================================
    // 내부 상태
    // ================================================================
    private int selectedSkinPreset = 1; // 기본: 보통 톤
    private int targetSlotIndex = -1;   // 새 게임을 저장할 슬롯 번호
    private bool isInfoPopup = false;   // true: 단순 알림 / false: 슬롯 삭제 확인

    // ================================================================
    // 초기화
    // ================================================================
    void Start()
    {
        SetupButtons();
        ShowMainMenu();
    }

    void SetupButtons()
    {
        // 메인 버튼
        startBtn.onClick.AddListener(OnClickStart);
        newBtn.onClick.AddListener(OnClickNew);
        loadBtn.onClick.AddListener(OnClickLoad);
        exitBtn.onClick.AddListener(OnClickExit);

        // 새 게임 패널
        btnSkinLight.onClick.AddListener(() => SelectSkin(0));
        btnSkinMedium.onClick.AddListener(() => SelectSkin(1));
        btnSkinDark.onClick.AddListener(() => SelectSkin(2));
        btnCreate.onClick.AddListener(OnClickCreate);
        btnNewCancel.onClick.AddListener(() =>
        {
            pnNewGame.SetActive(false);
        });

        // 슬롯 선택 패널
        for (int i = 0; i < SaveManager.MAX_SLOTS; i++)
        {
            int index = i; // 클로저 캡처용
            btnSlotStarts[i].onClick.AddListener(() => OnClickSlotStart(index));
            btnSlotDeletes[i].onClick.AddListener(() => OnClickSlotDelete(index));
        }
        btnSlotClose.onClick.AddListener(() => pnSlotSelect.SetActive(false));

        // 경고 패널
        btnWarningConfirm.onClick.AddListener(OnClickWarningConfirm);
        btnWarningCancel.onClick.AddListener(() => pnWarning.SetActive(false));
    }

    // ================================================================
    // 메인 메뉴 / New·Load 버튼 전환
    // ================================================================
    void ShowMainMenu()
    {
        startBtn.gameObject.SetActive(true);
        settingBtn.gameObject.SetActive(true);
        exitBtn.gameObject.SetActive(true);
        newBtn.gameObject.SetActive(false);
        loadBtn.gameObject.SetActive(false);
    }

    void ShowNewLoadButtons()
    {
        startBtn.gameObject.SetActive(false);
        settingBtn.gameObject.SetActive(false);
        exitBtn.gameObject.SetActive(false);
        newBtn.gameObject.SetActive(true);
        loadBtn.gameObject.SetActive(true);
    }

    void OnClickStart() => ShowNewLoadButtons();

    void OnClickExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ================================================================
    // 새 게임
    // ================================================================
    void OnClickNew()
    {
        int emptySlot = SaveManager.Instance.GetEmptySlot();

        if (emptySlot >= 0)
        {
            // 빈 슬롯 있음 → 바로 새 게임 패널
            targetSlotIndex = emptySlot;
            OpenNewGamePanel();
        }
        else
        {
            // 슬롯 꽉 참 → 삭제 확인 경고
            int oldestSlot = SaveManager.Instance.GetOldestSlot();
            SaveData oldest = SaveManager.Instance.Load(oldestSlot);
            targetSlotIndex = oldestSlot;

            string farmName = oldest != null ? oldest.farmName : "알 수 없음";
            ShowWarningPopup(
                $"저장 슬롯이 가득 찼습니다.\n" +
                $"가장 오래된 파일 [ {farmName} ] 이(가) 삭제됩니다.\n" +
                $"계속하시겠습니까?",
                isInfo: false
            );
        }
    }

    void OpenNewGamePanel()
    {
        inputFarmName.text = "";
        inputPlayerName.text = "";
        selectedSkinPreset = 1;
        pnNewGame.SetActive(true);
    }

    void SelectSkin(int preset)
    {
        selectedSkinPreset = preset;
        Debug.Log($"[TitleUI] 피부 톤 선택: {preset}");
        // 선택 표시 UI는 향후 추가
    }

    void OnClickCreate()
    {
        string farmName   = inputFarmName.text.Trim();
        string playerName = inputPlayerName.text.Trim();

        // 입력값 검증 → 빈 칸 있으면 알림 팝업
        if (string.IsNullOrEmpty(farmName) || string.IsNullOrEmpty(playerName))
        {
            ShowWarningPopup("농장 이름과 플레이어 이름을\n모두 입력해주세요.", isInfo: true);
            return;
        }

        // SaveData 생성
        SaveData newSave = new SaveData();
        newSave.slotIndex            = targetSlotIndex;
        newSave.farmName             = farmName;
        newSave.createdTime          = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        newSave.totalPlayTimeMinutes = 0f;

        // ApplyCustomization 먼저 → 그 다음 playerName 덮어쓰기 (추후 확인 필요)
        newSave.playerData.ApplyCustomization(GetCustomizationFromPreset(selectedSkinPreset));
        newSave.playerData.playerName = playerName;

        // 저장
        SaveManager.Instance.Save(newSave);

        // GameDataManager에 등록
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.currentSaveData = newSave;
            GameDataManager.Instance.SaveCustomization(newSave.playerData.ToCustomizationData());
        }

        Debug.Log($"[TitleUI] 새 게임 생성 완료 → 슬롯 {targetSlotIndex}: {farmName}");
        pnNewGame.SetActive(false);

        // 게임 씬으로 이동
        if (GameSceneManager.Instance != null)
            GameSceneManager.Instance.LoadGameScene();
    }

    /// <summary>
    /// 경고 팝업 표시
    /// isInfo: true  → 확인 버튼만 (단순 알림)
    /// isInfo: false → 확인 + 취소 버튼 (슬롯 삭제 확인)
    /// </summary>
    void ShowWarningPopup(string message, bool isInfo)
    {
        isInfoPopup = isInfo;
        txtWarning.text = message;
        btnWarningCancel.gameObject.SetActive(!isInfo); // 알림이면 취소 버튼 숨김
        pnWarning.SetActive(true);
    }

    PlayerCustomizationData GetCustomizationFromPreset(int preset)
    {
        var data = new PlayerCustomizationData();
        data.SetSkinToneByPreset(preset);
        return data;
    }

    // ================================================================
    // 경고 다이얼로그 (슬롯 꽉 찼을 때)
    // ================================================================
    void OnClickWarningConfirm()
    {
        pnWarning.SetActive(false);

        if (isInfoPopup)
        {
            // 단순 알림 → 팝업만 닫기
            return;
        }

        // 슬롯 삭제 확인 → 삭제 후 새 게임 패널
        SaveManager.Instance.Delete(targetSlotIndex);
        OpenNewGamePanel();
    }

    // ================================================================
    // 로드 (슬롯 선택)
    // ================================================================
    void OnClickLoad()
    {
        RefreshSlotUI();
        pnSlotSelect.SetActive(true);
    }

    void RefreshSlotUI()
    {
        SaveData[] slots = SaveManager.Instance.LoadAllSlots();

        for (int i = 0; i < SaveManager.MAX_SLOTS; i++)
        {
            SaveData data = slots[i];

            if (data != null)
            {
                txtFarmNames[i].text   = data.farmName;
                txtPlayerNames[i].text = data.playerData.playerName;
                txtPlayTimes[i].text   = SaveManager.FormatPlayTime(data.totalPlayTimeMinutes);
                txtLastSaves[i].text   = data.lastSaveTime;

                btnSlotStarts[i].interactable  = true;
                btnSlotDeletes[i].interactable = true;
            }
            else
            {
                txtFarmNames[i].text   = "빈 슬롯";
                txtPlayerNames[i].text = "-";
                txtPlayTimes[i].text   = "-";
                txtLastSaves[i].text   = "-";

                btnSlotStarts[i].interactable  = false;
                btnSlotDeletes[i].interactable = false;
            }
        }
    }

    void OnClickSlotStart(int slotIndex)
    {
        SaveData data = SaveManager.Instance.Load(slotIndex);
        if (data == null) return;

        // GameDataManager에 등록
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.currentSaveData = data;
            GameDataManager.Instance.SaveCustomization(data.playerData.ToCustomizationData());
        }

        Debug.Log($"[TitleUI] 슬롯 {slotIndex} 로드: {data.farmName}");
        pnSlotSelect.SetActive(false);

        // 게임 씬으로 이동
        if (GameSceneManager.Instance != null)
            GameSceneManager.Instance.LoadGameScene();
    }

    void OnClickSlotDelete(int slotIndex)
    {
        SaveManager.Instance.Delete(slotIndex);
        RefreshSlotUI();
        Debug.Log($"[TitleUI] 슬롯 {slotIndex} 삭제");
    }
}
