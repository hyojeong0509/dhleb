using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 침대 상호작용
// 흐름 (꿈 있을 때): 확인 → 페이드 아웃 → 텔레포트 to 꿈맵 → 페이드 인 → 탐색
//                  → 포탈 "네" → 페이드 아웃 → 복귀 → 저장 → 저장 완료 패널 → [다음] → 다음날
// 흐름 (꿈 없을 때): 확인 → 페이드 아웃 → 페이드 인 → 저장 → 저장 완료 패널 → [다음] → 다음날
[RequireComponent(typeof(Collider2D))]
public class BedInteract : MonoBehaviour
{
    [Header("상호작용")]
    public string playerTag = "Player";

    [Header("팝업 UI")]
    public GameObject popupPanel;
    public TMP_Text popupMessage;
    public Button btnYes;
    public Button btnNo;

    [Header("저장 완료 표시")]
    public GameObject saveCompletePanel;
    public TMP_Text saveCompleteText;
    public Button btnNext;
    public float fadeDuration = 0.4f;
    public float btnNextEnableDelay = 1f;

    [Header("첫날 스토리")]
    [Tooltip("(레거시) 꿈 시퀀스로 대체됨. 꿈이 없을 때만 동작.")]
    public DialogueData firstDaySleepDialogue;

    [Header("꿈 시퀀스 (Dream-Link)")]
    [Tooltip("꿈 시퀀스 목록. 비우면 Resources/DreamSequenceRegistry 로드")]
    public DreamSequenceRegistry dreamRegistry;

    private bool playerInRange;
    private bool isProcessing;
    private DreamSequenceData pendingDream;
    private int dayBeforeSleep;
    private float sleepTimeRecord;

    void Start()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
        }

        var col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
            col.isTrigger = true;

        if (popupPanel != null) popupPanel.SetActive(false);
        if (saveCompletePanel != null) saveCompletePanel.SetActive(false);

        if (btnYes != null) btnYes.onClick.AddListener(OnConfirmSleep);
        if (btnNo != null) btnNo.onClick.AddListener(OnCancelSleep);
        if (btnNext != null) btnNext.onClick.AddListener(OnClickNext);

        if (popupMessage != null) popupMessage.text = "하루를 마무리 할거니?";
        if (saveCompleteText != null) saveCompleteText.text = "저장했삼";
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            if (popupPanel != null && !isProcessing)
            {
                popupPanel.SetActive(true);
                GameInputBlocker.Block();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            if (popupPanel != null)
            {
                popupPanel.SetActive(false);
                if (!isProcessing)
                    GameInputBlocker.Unblock();
            }
        }
    }

    void OnConfirmSleep()
    {
        if (isProcessing) return;
        isProcessing = true;

        if (popupPanel != null) popupPanel.SetActive(false);

        // 페이드 아웃 → (검은 화면 중) 수면 처리 + 필요시 텔레포트 → 페이드 인
        if (fadeDuration > 0f && ScreenFadeManager.Instance != null)
            ScreenFadeManager.Instance.FadeOutIn(
                onMid: DoSleepLogic,
                fadeOutDuration: fadeDuration,
                fadeInDuration: fadeDuration,
                onComplete: AfterSleepFadeIn
            );
        else
        {
            DoSleepLogic();
            AfterSleepFadeIn();
        }
    }

    // 검은 화면 상태에서 실행. 시간 동결 + 꿈 체크 + 텔레포트.
    // 날짜 전환/저장은 꿈이 끝난 뒤 SaveAndShowPanel()에서 처리.
    void DoSleepLogic()
    {
        sleepTimeRecord = TimeManager.Instance != null ? TimeManager.Instance.CurrentGameMinutes : 0f;
        dayBeforeSleep  = TimeManager.Instance != null ? TimeManager.Instance.CurrentDay : 1;

        // 꿈 중 시간이 흐르지 않도록 정지 (날짜는 아직 바꾸지 않음)
        if (TimeManager.Instance != null)
            TimeManager.Instance.SetPause(true);

        // 꿈 시퀀스 사전 체크
        CheckDreamSequence(dayBeforeSleep);

        // 맵 이동형 꿈: 검은 화면 중 텔레포트 (페이드 인 시 꿈맵이 보임)
        if (pendingDream != null && pendingDream.IsMapWalk)
            TeleportPlayerTo(pendingDream.dreamEntryPosition);
    }

    // 페이드 인 완료 후 호출. 꿈 or 저장패널 분기.
    void AfterSleepFadeIn()
    {
        if (pendingDream != null && pendingDream.IsMapWalk)
        {
            // 꿈맵으로 이동 완료. 플레이어 자유 탐색 허용.
            // pendingDream은 null로 초기화 (flag/저장은 DreamReturnPortal이 처리)
            pendingDream = null;
            GameInputBlocker.Unblock();
            // isProcessing은 OnDreamReturnComplete()에서 저장 후 패널이 닫힐 때까지 유지
        }
        else if (pendingDream != null)
        {
            // 컷신/대화형 꿈: 재생 후 저장
            StartNonMapDream();
        }
        else
        {
            // 꿈 없음: 바로 저장
            SaveAndShowPanel();
        }
    }

    // 컷신/대화형 꿈 재생. 종료 후 저장.
    void StartNonMapDream()
    {
        var dream = pendingDream;
        pendingDream = null;

        void OnDreamComplete()
        {
            if (!string.IsNullOrEmpty(dream.setFlagWhenDone) && GameProgressManager.Instance != null)
                GameProgressManager.Instance.SetFlag(dream.setFlagWhenDone);
            SaveAndShowPanel();
        }

        if (dream.cutscene != null && CutsceneManager.Instance != null)
            CutsceneManager.Instance.Play(dream.cutscene, OnDreamComplete);
        else if (dream.dialogue != null && DialogueManager.Instance != null)
            DialogueManager.Instance.Play(dream.dialogue, OnDreamComplete);
        else
            OnDreamComplete();
    }

    // ── 꿈 복귀 후 처리 (DreamReturnPortal에서 호출) ─────────────

    /// <summary>
    /// 꿈맵 포탈에서 귀환 완료 시 DreamReturnPortal이 호출.
    /// 저장 → 저장 완료 패널 표시.
    /// </summary>
    public void OnDreamReturnComplete()
    {
        GameInputBlocker.Block();
        SaveAndShowPanel();
    }

    // ── 저장 및 패널 ──────────────────────────────────────────────

    void SaveAndShowPanel()
    {
        // 시간 재개 → 날짜 전환 (Sleep()이 isDayRunning=true일 때만 동작하므로 먼저 해제)
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.SetPause(false);
            TimeManager.Instance.Sleep(); // 다음날로 전환, 시간 06:00으로 초기화
        }

        // 수면 시각 기반 스테미나 회복
        if (StaminaManager.Instance != null)
            StaminaManager.Instance.RestoreOnSleep(sleepTimeRecord);

        // 저장 (다음날 데이터로 저장됨)
        if (GameDataManager.Instance != null)
            GameDataManager.Instance.SaveGame();

        ShowSaveComplete();
    }

    void OnCancelSleep()
    {
        if (popupPanel != null) popupPanel.SetActive(false);
        GameInputBlocker.Unblock();
    }

    void ShowSaveComplete()
    {
        if (saveCompletePanel == null)
        {
            FinishSleep();
            return;
        }

        saveCompletePanel.SetActive(true);

        if (btnNext != null)
        {
            btnNext.gameObject.SetActive(false);
            Invoke(nameof(EnableNextButton), btnNextEnableDelay);
        }
        else
        {
            Invoke(nameof(AutoFinish), 2f);
        }
    }

    void EnableNextButton()
    {
        if (btnNext != null)
            btnNext.gameObject.SetActive(true);
    }

    // [다음] 버튼: 저장 패널 닫고 다음날 시작
    void OnClickNext()
    {
        if (saveCompletePanel != null) saveCompletePanel.SetActive(false);

        if (fadeDuration > 0f && ScreenFadeManager.Instance != null)
            ScreenFadeManager.Instance.FadeOutIn(FinishSleep, fadeDuration, fadeDuration);
        else
            FinishSleep();
    }

    void AutoFinish()
    {
        if (saveCompletePanel != null) saveCompletePanel.SetActive(false);
        FinishSleep();
    }

    void FinishSleep()
    {
        isProcessing = false;
        GameInputBlocker.Unblock();
    }

    // ── 꿈 체크 ───────────────────────────────────────────────────

    void CheckDreamSequence(int day)
    {
        var registry = dreamRegistry != null
            ? dreamRegistry
            : Resources.Load<DreamSequenceRegistry>("DreamSequenceRegistry");

        if (registry == null || registry.dreams == null || registry.dreams.Count == 0)
        {
            pendingDream = null;
            return;
        }

        pendingDream = registry.GetDreamToPlay(day);
    }

    // ── 유틸 ─────────────────────────────────────────────────────

    static void TeleportPlayerTo(Vector3 position)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Vector3 pos = position;
        pos.z = player.transform.position.z;

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.MovePosition(pos);
        else player.transform.position = pos;
    }
}
