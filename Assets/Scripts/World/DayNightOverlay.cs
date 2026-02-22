using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 시간에 따라 화면을 어둡게/밝게 하는 오버레이
/// [설정 방법] Canvas > UI Image (Stretch 전체) > 이 스크립트 추가
/// [시간 빠르게] F2 키로 10배속 토글 (TimeManager)
/// </summary>
[RequireComponent(typeof(Image))]
public class DayNightOverlay : MonoBehaviour
{
    [Header("오버레이 색/밝기")]
    [Tooltip("밤에 씌울 어두운 색. R,G,B가 어두울수록 진한 밤. (Alpha는 스크립트가 자동 조절)")]
    public Color nightColor = new Color(0.1f, 0.1f, 0.2f, 0.7f);

    [Tooltip("06:00 직후 새벽의 어두움. 0=밝음, 1=완전 어둠. 보통 0.3~0.5")]
    [Range(0f, 1f)] public float dawnMinAlpha = 0.4f;

    [Tooltip("24:00~02:00 밤의 최대 어두움. 0=밝음, 1=완전 어둠. 보통 0.6~0.85")]
    [Range(0f, 1f)] public float nightMaxAlpha = 0.75f;

    [Header("시간 구간 (DayProgress 0~1 = 06:00~02:00)")]
    [Tooltip("새벽 끝. 0.25≈10:00. 이 구간까지: dawnMinAlpha→0 (밝아짐)")]
    [Range(0f, 0.5f)] public float dawnEnd = 0.25f;

    [Tooltip("낮 끝. 0.5≈14:00. dawnEnd~noonEnd: 완전 밝음 유지")]
    [Range(0.25f, 0.75f)] public float noonEnd = 0.5f;

    [Tooltip("밤 시작. 0.75≈18:00. noonEnd~nightStart: 0→nightMaxAlpha (어두워짐)")]
    [Range(0.5f, 1f)] public float nightStart = 0.75f;

    private Image overlayImage;

    void Awake()
    {
        overlayImage = GetComponent<Image>();
        if (overlayImage == null) return;

        overlayImage.raycastTarget = false;
        overlayImage.color = new Color(nightColor.r, nightColor.g, nightColor.b, 0f);
    }

    void Update()
    {
        UpdateOverlay();
    }

    void UpdateOverlay()
    {
        if (overlayImage == null || TimeManager.Instance == null) return;

        float progress = TimeManager.Instance.DayProgress;
        float alpha = GetAlphaForProgress(progress);

        Color c = nightColor;
        c.a = alpha;
        overlayImage.color = c;
    }

    /// <summary>
    /// DayProgress(0~1)에 따른 오버레이 알파 반환
    /// </summary>
    float GetAlphaForProgress(float progress)
    {
        if (progress <= dawnEnd)
        {
            // 새벽: dawnMinAlpha → 0 (밝아짐)
            return Mathf.Lerp(dawnMinAlpha, 0f, progress / dawnEnd);
        }
        if (progress <= noonEnd)
        {
            // 낮: 0 유지
            return 0f;
        }
        if (progress <= nightStart)
        {
            // 저녁: 0 → nightMaxAlpha (어두워짐)
            float t = (progress - noonEnd) / (nightStart - noonEnd);
            return Mathf.Lerp(0f, nightMaxAlpha, t);
        }
        // 밤: nightMaxAlpha 유지
        return nightMaxAlpha;
    }
}
