using UnityEngine;
using System;

/// <summary>
/// 게임 내 시간/날짜 관리
/// - 1년 = 6 Sequence
/// - 1 Sequence = 20일
/// - 하루 = 06:00 ~ 02:00 (다음날)
/// - 실제 15분 = 게임 하루
/// </summary>
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    // ── 설정 (Inspector에서 조정 가능) ─────────────────────
    [Header("하루 시간 설정")]
    public int dayStartHour = 6;            // 하루 시작 (06:00)
    public int dayEndHour = 26;             // 하루 종료 (02:00 = 26:00으로 표현)
    public float realSecondsPerDay = 900f;  // 실제 몇 초 = 게임 하루 (900초 = 15분)

    [Header("디버그")]
    public float timeScale = 1f;            // 시간 배속 (1 = 정상, 10 = 10배속)
    public KeyCode debugNextDayKey = KeyCode.Tab; // 누르면 즉시 다음날

    [Header("날짜 설정")]
    public int daysPerSequence = 20;        // 1 Sequence당 일수
    public int sequencesPerYear = 6;        // 1년당 Sequence 수

    // ── Sequence 이름 ────────────────────────────────────
    public static readonly string[] SequenceNames =
    {
        "Dawn Sequence",
        "Bloom Sequence",
        "Flux Sequence",
        "Veil Sequence",
        "Rift Sequence",
        "Echo Sequence"
    };

    // ── 현재 시간 상태 ────────────────────────────────────
    [Header("현재 상태 (읽기 전용)")]
    [SerializeField] private int currentYear = 1;
    [SerializeField] private int currentSequence = 0;  // 0 = Dawn, 5 = Echo
    [SerializeField] private int currentDay = 1;
    [SerializeField] private float currentGameMinutes;  // 자정(00:00)부터의 게임 분

    // ── 이벤트 ────────────────────────────────────────────
    public event Action OnMinuteChanged;        // 1분마다
    public event Action OnTenMinuteChanged;     // 10분마다 (HUD 시간 표시용)
    public event Action OnHourChanged;          // 1시간마다
    public event Action OnDayChanged;           // 날짜 바뀔 때
    public event Action OnSequenceChanged;      // Sequence 바뀔 때
    public event Action OnDayEnd;               // 하루 종료 (02:00)

    // ── 내부 계산용 ───────────────────────────────────────
    private float gameMinutesPerRealSecond;
    private int lastMinute;
    private int lastHour;
    private bool isDayRunning = true;

    // ── 프로퍼티 ──────────────────────────────────────────
    public int CurrentYear     => currentYear;
    public int CurrentSequence => currentSequence;
    public int CurrentDay      => currentDay;
    public string CurrentSequenceName => SequenceNames[currentSequence];

    //현재 시(0~23)
    public int CurrentHour => Mathf.FloorToInt(currentGameMinutes / 60f) % 24;

    //현재 분(0~59)
    public int CurrentMinute => Mathf.FloorToInt(currentGameMinutes % 60f);

    //현재 시간을 "HH:MM" 형식 문자열로 반환
    public string CurrentTimeString
    {
        get
        {
            int hour = Mathf.FloorToInt(currentGameMinutes / 60f) % 24;
            int minute = Mathf.FloorToInt(currentGameMinutes % 60f);
            return $"{hour:D2}:{minute:D2}";
        }
    }

    // 하루 진행도 (0.0 = 06:00, 1.0 = 02:00)
    public float DayProgress
    {
        get
        {
            float start = dayStartHour * 60f;
            float end   = dayEndHour   * 60f;
            return Mathf.Clamp01((currentGameMinutes - start) / (end - start));
        }
    }

    // ─────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // 하루 시작 시간(분)으로 초기화
        currentGameMinutes = dayStartHour * 60f;

        // 실제 1초당 흘러야 할 게임 분 계산
        // 예: (26-6)*60 / 900 = 1200/900 = 1.333 게임분/초
        int totalGameMinutes = (dayEndHour - dayStartHour) * 60;
        gameMinutesPerRealSecond = totalGameMinutes / realSecondsPerDay;

        lastMinute = CurrentMinute;
        lastHour = CurrentHour;
    }

    void Update()
    {
        if (!isDayRunning) return;

        // 디버그: Tab 키로 즉시 다음날
        if (Input.GetKeyDown(debugNextDayKey))
        {
            EndDay();
            return;
        }

        // 게임 시간 진행 (timeScale 배속 적용)
        currentGameMinutes += gameMinutesPerRealSecond * Time.deltaTime * timeScale;

        // 분 바뀜 감지
        if (CurrentMinute != lastMinute)
        {
            lastMinute = CurrentMinute;
            OnMinuteChanged?.Invoke();

            // 10분 단위마다 추가 이벤트
            if (CurrentMinute % 10 == 0)
                OnTenMinuteChanged?.Invoke();
        }

        // 시 바뀜 감지
        if (CurrentHour != lastHour)
        {
            lastHour = CurrentHour;
            OnHourChanged?.Invoke();
        }

        // 하루 종료 체크 (dayEndHour:00 도달 시)
        if (currentGameMinutes >= dayEndHour * 60f)
            EndDay();
    }

    // ── 하루 종료 / 다음날 ────────────────────────────────

    // 하루 종료 (시간 초과 or 침대 취침 시 호출)
    public void EndDay()
    {
        if (!isDayRunning) return;
        isDayRunning = false;
        OnDayEnd?.Invoke();
        AdvanceToNextDay();
    }

    // 다음날로 진행 (날짜, Sequence, Year 갱신)
    void AdvanceToNextDay()
    {
        currentDay++;

        if (currentDay > daysPerSequence)
        {
            currentDay = 1;
            currentSequence++;

            if (currentSequence >= sequencesPerYear)
            {
                currentSequence = 0;
                currentYear++;
            }

            OnSequenceChanged?.Invoke();
        }

        // 시간 초기화
        currentGameMinutes = dayStartHour * 60f;
        lastMinute = CurrentMinute;
        lastHour = CurrentHour;
        isDayRunning = true;

        OnDayChanged?.Invoke();

        Debug.Log($"[TimeManager] 새로운 날: Year {currentYear} / {CurrentSequenceName} / Day {currentDay}");
    }

    // ── 외부 호출용 ──────────────────────────────────────

    // 침대에서 잘 때 호출
    public void Sleep() => EndDay();

    // 시간 일시정지 / 재개 (대화, 메뉴 열릴 때 등)
    public void SetPause(bool paused) => isDayRunning = !paused;
}
