using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// 침대 상호작용
// 흐름: 침대 접근 → "하루 마무리?" 팝업 → [예] → Sleep → 저장 → 정산 표시 → (꿈 시퀀스 분기) → 다음날
// 꿈 시퀀스는 DreamSequenceRegistry에서 조건별로 지정. Inspector에서 설정 가능.
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
    [Tooltip("첫날 잠 시 재생할 대화 (튜토리얼 중)")]
    public DialogueData firstDaySleepDialogue;

    [Header("꿈 시퀀스 (Dream-Link)")]
    [Tooltip("꿈 시퀀스 목록. 비우면 Resources/DreamSequenceRegistry 로드")]
    public DreamSequenceRegistry dreamRegistry;

    private bool playerInRange;
    private bool isProcessing;
    private DreamSequenceData pendingDream;

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

        // 페이드 아웃 → Sleep/저장/패널 표시 → 페이드 인
        if (fadeDuration > 0f && ScreenFadeManager.Instance != null)
        {
            ScreenFadeManager.Instance.FadeOutIn(DoSleepAndSave, fadeDuration, fadeDuration);
        }
        else
        {
            DoSleepAndSave();
        }
    }

    void DoSleepAndSave()
    {
        StartCoroutine(DoSleepAndSaveRoutine());
    }

    IEnumerator DoSleepAndSaveRoutine()
    {
        // 1. Sleep 전 현재 시각 저장 (Sleep()이 날짜를 바꾸므로)
        float sleepTime = TimeManager.Instance != null ? TimeManager.Instance.CurrentGameMinutes : 0f;
        int dayBeforeSleep = TimeManager.Instance != null ? TimeManager.Instance.CurrentDay : 1;

        // 2. 하루 종료 (다음날로)
        if (TimeManager.Instance != null)
            TimeManager.Instance.Sleep();

        // 3. 스테미나 회복 (시각별: 02:00 이전 100%, 03:00까지 60%, 04:00까지 40%, 05:00~ 20%)
        if (StaminaManager.Instance != null)
            StaminaManager.Instance.RestoreOnSleep(sleepTime);

        // 4. 첫날 잠 시 "스트레스" 대화 (저장 전)
        if (dayBeforeSleep == 1 && firstDaySleepDialogue != null
            && GameProgressManager.Instance != null && GameProgressManager.Instance.IsBeforeTutorialEnd)
        {
            if (DialogueManager.Instance != null)
            {
                bool done = false;
                DialogueManager.Instance.Play(firstDaySleepDialogue, () => done = true);
                while (!done)
                    yield return null;
            }
        }

        // 5. 저장
        if (GameDataManager.Instance != null)
            GameDataManager.Instance.SaveGame();

        // 6. 저장 완료 표시
        ShowSaveComplete();

        // 7. 꿈 시퀀스 분기 (Dream-Link)
        if (ShouldPlayDreamSequence(dayBeforeSleep))
        {
            StartDreamSequence();
        }
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
            isProcessing = false;
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
            Invoke(nameof(HideSaveComplete), 2f);
        }
    }

    void EnableNextButton()
    {
        if (btnNext != null)
            btnNext.gameObject.SetActive(true);
    }

    void OnClickNext()
    {
        if (fadeDuration > 0f && ScreenFadeManager.Instance != null)
        {
            ScreenFadeManager.Instance.FadeOutIn(HideSaveComplete, fadeDuration, fadeDuration);
        }
        else
        {
            HideSaveComplete();
        }
    }

    void HideSaveComplete()
    {
        if (saveCompletePanel != null) saveCompletePanel.SetActive(false);
        isProcessing = false;
        GameInputBlocker.Unblock();
    }

    // 꿈 시퀀스 재생 여부. dayBeforeSleep = 잠 자기 직전 날짜 (1일차 잠 = 1).
    bool ShouldPlayDreamSequence(int dayBeforeSleep)
    {
        var registry = dreamRegistry != null ? dreamRegistry : Resources.Load<DreamSequenceRegistry>("DreamSequenceRegistry");
        if (registry == null || registry.dreams == null || registry.dreams.Count == 0)
            return false;

        pendingDream = registry.GetDreamToPlay(dayBeforeSleep);
        return pendingDream != null;
    }

    // 꿈 시퀀스 시작. 컷신 또는 대화 재생.
    void StartDreamSequence()
    {
        if (pendingDream == null) return;

        var dream = pendingDream;

        void OnDreamComplete()
        {
            if (!string.IsNullOrEmpty(dream.setFlagWhenDone) && GameProgressManager.Instance != null)
                GameProgressManager.Instance.SetFlag(dream.setFlagWhenDone);
            pendingDream = null;
        }

        if (dream.cutscene != null && CutsceneManager.Instance != null)
        {
            CutsceneManager.Instance.Play(dream.cutscene, OnDreamComplete);
        }
        else if (dream.dialogue != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.Play(dream.dialogue, OnDreamComplete);
        }
        else
        {
            OnDreamComplete();
        }
    }

    // 꿈 시퀀스 종료 시 호출 (꿈 씬/시퀀스 스크립트에서 호출)
    public void OnDreamSequenceEnd()
    {
        isProcessing = false;
    }
}
