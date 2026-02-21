using UnityEngine;
using TMPro;

/// <summary>
/// TimeManager와 HUD 텍스트 + 시계 바늘 연결
/// </summary>
public class HUDTimeDisplay : MonoBehaviour
{
    [Header("HUD 텍스트")]
    public TMP_Text txtTime;            // "06:00"
    public TMP_Text txtDate;            // "Dawn · 1"

    [Header("시계 바늘")]
    public Transform clockHand;         // ClockHand 오브젝트
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
        string seqShort = TimeManager.SequenceNames[TimeManager.Instance.CurrentSequence]
                          .Replace(" Sequence", "");
        txtDate.text = $"{seqShort} · {TimeManager.Instance.CurrentDay}";
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
