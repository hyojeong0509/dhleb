using UnityEngine;
using TMPro;

// TimeManager와 HUD 텍스트 + 시계 바늘 연결
public class HUDTimeDisplay : MonoBehaviour
{
    [Header("HUD 텍스트")]
    public TMP_Text txtTime;
    public TMP_Text txtDate;

    [Header("시간 색상")]
    public Color normalColor = Color.black;   // 평소 색상
    public Color lateNightColor = Color.red;  // 02:00~05:50 빨간 시간

    [Header("시계 바늘")]
    public Transform clockHand;
    public float startAngle = 90f;      // 하루 시작(06:00)일 때 Z 회전값

    void Start()
    {
        if (TimeManager.Instance == null) return;

        TimeManager.Instance.OnTenMinuteChanged += UpdateTime;
        TimeManager.Instance.OnDayChanged       += UpdateDate;
        TimeManager.Instance.OnSequenceChanged  += UpdateDate;

        UpdateTime();
        UpdateDate();
    }

    void OnDestroy()
    {
        if (TimeManager.Instance == null) return;

        TimeManager.Instance.OnTenMinuteChanged -= UpdateTime;
        TimeManager.Instance.OnDayChanged       -= UpdateDate;
        TimeManager.Instance.OnSequenceChanged  -= UpdateDate;
    }

    void Update()
    {
        UpdateClockHand();
        // 매 프레임 시간 색상 갱신 (02:00~05:50 진입 시)
        if (txtTime != null && TimeManager.Instance != null)
            txtTime.color = TimeManager.Instance.IsLateNightHours ? lateNightColor : normalColor;
    }

    void UpdateTime()
    {
        if (txtTime == null) return;

        // 10분 단위로 내림해서 표시 (06:00, 06:10, 06:20 ...)
        int hour = TimeManager.Instance.CurrentHour;
        int minute = (TimeManager.Instance.CurrentMinute / 10) * 10;
        txtTime.text = $"{hour:D2} : {minute:D2}";
    }

    void UpdateDate()
    {
        if (txtDate == null) return;
        string seqName = TimeManager.SequenceNames[TimeManager.Instance.CurrentSequence];
        txtDate.text = $"{seqName}, {TimeManager.Instance.CurrentDay}";
    }

    void UpdateClockHand()
    {
        if (clockHand == null || TimeManager.Instance == null) return;

        // 하루 진행도(0~1)에 따라 시계 방향으로 360도 회전
        float progress = TimeManager.Instance.DayProgress;
        float zAngle = startAngle - (progress * 360f);
        clockHand.localRotation = Quaternion.Euler(0f, 0f, zAngle);
    }
}
