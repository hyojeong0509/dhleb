using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 스타듀밸리 스타일 화면 페이드 인/아웃
/// 텔레포트, 씬 전환 등에 사용
/// </summary>
public class ScreenFadeManager : MonoBehaviour
{
    public static ScreenFadeManager Instance { get; private set; }

    [Header("설정")]
    public float defaultFadeDuration = 0.4f;
    public Color fadeColor = Color.black;

    private Image overlayImage;
    private CanvasGroup canvasGroup;
    private bool isFading;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureInstance()
    {
        if (Instance == null)
        {
            var go = new GameObject("ScreenFadeManager");
            go.AddComponent<ScreenFadeManager>();
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        CreateOverlay();
    }

    void CreateOverlay()
    {
        var canvasGo = new GameObject("ScreenFadeCanvas");
        canvasGo.transform.SetParent(transform);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 99999; // 최상단
        canvas.pixelPerfect = false;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var imgGo = new GameObject("FadeOverlay");
        imgGo.transform.SetParent(canvasGo.transform, false);

        overlayImage = imgGo.AddComponent<Image>();
        overlayImage.color = fadeColor;
        overlayImage.raycastTarget = false;

        var rt = imgGo.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        canvasGroup = imgGo.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// 페이드 아웃 → 콜백 → 페이드 인 (스타듀밸리 스타일)
    /// </summary>
    public void FadeOutIn(System.Action onMid, float fadeOutDuration = -1f, float fadeInDuration = -1f)
    {
        if (isFading) return;
        float outDur = fadeOutDuration > 0 ? fadeOutDuration : defaultFadeDuration;
        float inDur = fadeInDuration > 0 ? fadeInDuration : defaultFadeDuration;

        StartCoroutine(FadeOutInRoutine(onMid, outDur, inDur));
    }

    IEnumerator FadeOutInRoutine(System.Action onMid, float outDur, float inDur)
    {
        isFading = true;

        // 페이드 아웃
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / outDur;
            canvasGroup.alpha = Mathf.Clamp01(t);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // 중간 콜백 (텔레포트 등)
        onMid?.Invoke();

        yield return null; // 한 프레임 대기

        // 페이드 인
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / inDur;
            canvasGroup.alpha = 1f - Mathf.Clamp01(t);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        isFading = false;
    }
}
