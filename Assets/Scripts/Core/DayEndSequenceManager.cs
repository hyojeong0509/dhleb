using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 06:00 도달 시 시퀀스 (잠 안 잔 채)
/// 플레이어 엎어짐 → 페이드 → 이벤트 1(응급실) or 2(하루 날림) → 저장 → 다음
/// </summary>
public class DayEndSequenceManager : MonoBehaviour
{
    public static DayEndSequenceManager Instance { get; private set; }

    [Header("플레이어")]
    public GameObject playerObject;
    public string playerTag = "Player";

    [Header("이벤트 1 - 응급실")]
    public GameObject event1Panel;  // 응급실 컷씬 패널

    [Header("이벤트 2 - 하루 날림")]
    public GameObject event2Panel;
    public TMP_Text event2Text;  // "닉네임은 시원하게 하루를 통으로 날렸다!"

    [Header("공통")]
    public GameObject nextButtonPanel;  // [다음] 버튼이 있는 패널
    public Button btnNext;
    public float fallDisplayDuration = 1.5f;  // 엎어짐 표시 시간
    public float fadeDuration = 0.5f;
    [Tooltip("이벤트 패널 표시 시간 (저장패널로 전환 전)")]
    public float eventDisplayDuration = 1.5f;

    [Header("이벤트 1 비용")]
    public int hospitalCost = 1000;
    public int event1StaminaRecovery = 10;

    private bool isRunning;
    private int selectedEvent;  // 1 or 2

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.OnSixAMReached += OnSixAMReached;

        if (btnNext != null)
            btnNext.onClick.AddListener(OnClickNext);

        if (playerObject == null)
            playerObject = GameObject.FindGameObjectWithTag(playerTag);

        HideAllPanels();
    }

    void OnDestroy()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.OnSixAMReached -= OnSixAMReached;
    }

    void HideAllPanels()
    {
        if (event1Panel != null) event1Panel.SetActive(false);
        if (event2Panel != null) event2Panel.SetActive(false);
        if (nextButtonPanel != null) nextButtonPanel.SetActive(false);
    }

    void OnSixAMReached()
    {
        if (isRunning) return;
        StartCoroutine(RunSequence());
    }

    IEnumerator RunSequence()
    {
        isRunning = true;
        GameInputBlocker.Block();

        // 1. 플레이어 엎어짐 (이벤트 발송 - PlayerFallHandler 등에서 수신)
        PlayerFallHandler.NotifyFall(true);
        yield return new WaitForSeconds(fallDisplayDuration);

        // 2. 페이드 아웃 → 이벤트 패널 표시 → 페이드 인
        selectedEvent = Random.Range(0, 2) + 1;  // 1 or 2

        if (selectedEvent == 2 && event2Text != null)
        {
            string name = "플레이어";
            if (GameDataManager.Instance?.currentSaveData?.playerData != null)
                name = GameDataManager.Instance.currentSaveData.playerData.playerName;
            else if (GameDataManager.Instance?.currentCustomization != null)
                name = GameDataManager.Instance.currentCustomization.playerName;
            event2Text.text = $"{name}은 시원하게 하루를 통으로 날렸다!";
        }

        if (ScreenFadeManager.Instance != null)
        {
            ScreenFadeManager.Instance.FadeOutIn(() =>
            {
                // 페이드 아웃 완료 시점에 패널 표시
                if (selectedEvent == 1 && event1Panel != null) event1Panel.SetActive(true);
                if (selectedEvent == 2 && event2Panel != null) event2Panel.SetActive(true);
            }, fadeDuration, fadeDuration);
            yield return new WaitForSeconds(fadeDuration * 2f + eventDisplayDuration);
        }

        // 5. 저장
        if (GameDataManager.Instance != null)
            GameDataManager.Instance.SaveGame();

        // 6. 이벤트 1: 스테미나 10%, 골드 -1000
        if (selectedEvent == 1)
        {
            if (StaminaManager.Instance != null)
                StaminaManager.Instance.SetStamina(event1StaminaRecovery);
            if (GoldManager.Instance != null)
                GoldManager.Instance.ForceSpend(hospitalCost);
        }

        // 7. 이벤트 패널 비활성화 → 저장패널 표시
        if (event1Panel != null) event1Panel.SetActive(false);
        if (event2Panel != null) event2Panel.SetActive(false);
        if (nextButtonPanel != null) nextButtonPanel.SetActive(true);
        if (btnNext != null) btnNext.gameObject.SetActive(true);
    }

    void OnClickNext()
    {
        if (!isRunning) return;

        int daysToAdvance = selectedEvent == 2 ? 2 : 1;

        HideAllPanels();
        PlayerFallHandler.NotifyFall(false);

        if (TimeManager.Instance != null)
            TimeManager.Instance.AdvanceToNextDay(daysToAdvance);

        isRunning = false;
        GameInputBlocker.Unblock();
    }
}
