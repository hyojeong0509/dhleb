using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Global Light 2D로 낮/밤 밝기 제어.
/// 이 방식을 쓰면 촛불 등에 Point Light 2D를 붙이면 그 주변이 자연스럽게 환해짐.
///
/// [사용법]
/// 1. Global Light 2D 오브젝트에 이 스크립트 추가
/// 2. DayNightOverlay는 끄거나 제거 (둘 다 쓰면 오버레이가 2D 라이트까지 가림)
/// 3. 촛불: Point Light 2D 추가, Intensity 0.5~1, Outer Radius 조절
/// </summary>
public class DayNightGlobalLight : MonoBehaviour
{
    [Header("밝기")]
    [Tooltip("낮의 Global Light intensity (1 = 밝음)")]
    [Range(0.5f, 1.5f)] public float dayIntensity = 1f;

    [Tooltip("06:00 새벽 시작 시 밝기 (높을수록 밝음, nightIntensity보다 크게)")]
    [Range(0.2f, 1f)] public float dawnIntensity = 0.5f;

    [Tooltip("밤의 Global Light intensity (0.2 = 어두움)")]
    [Range(0f, 0.5f)] public float nightIntensity = 0.2f;

    [Header("색상 (밤에 약간 푸르게)")]
    public Color dayColor = Color.white;
    public Color nightColor = new Color(0.7f, 0.75f, 0.9f, 1f);

    [Header("시간 구간 (DayProgress 0~1)")]
    [Range(0f, 0.5f)] public float dawnEnd = 0.25f;
    [Range(0.25f, 0.75f)] public float noonEnd = 0.5f;
    [Range(0.5f, 1f)] public float nightStart = 0.75f;

    private Light2D light2D;

    void Awake()
    {
        light2D = GetComponent<Light2D>();
        if (light2D == null)
        {
            Debug.LogWarning("[DayNightGlobalLight] Light2D 컴포넌트가 없습니다. Global Light 2D에 붙여주세요.");
            enabled = false;
        }
    }

    void Update()
    {
        if (light2D == null || TimeManager.Instance == null) return;

        float progress = TimeManager.Instance.DayProgress;
        float intensity = GetIntensityForProgress(progress);
        Color color = GetColorForProgress(progress);

        light2D.intensity = intensity;
        light2D.color = color;
    }

    float GetIntensityForProgress(float progress)
    {
        if (progress <= dawnEnd)
            return Mathf.Lerp(dawnIntensity, dayIntensity, progress / dawnEnd);
        if (progress <= noonEnd)
            return dayIntensity;
        if (progress <= nightStart)
            return Mathf.Lerp(dayIntensity, nightIntensity, (progress - noonEnd) / (nightStart - noonEnd));
        return nightIntensity;
    }

    Color GetColorForProgress(float progress)
    {
        if (progress <= dawnEnd)
            return Color.Lerp(nightColor, dayColor, progress / dawnEnd);
        if (progress <= noonEnd)
            return dayColor;
        if (progress <= nightStart)
            return Color.Lerp(dayColor, nightColor, (progress - noonEnd) / (nightStart - noonEnd));
        return nightColor;
    }
}
