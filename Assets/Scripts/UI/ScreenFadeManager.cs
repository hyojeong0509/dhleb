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
    /// 오버레이 알파 즉시 설정 (0=투명, 1=완전 검정)
    /// </summary>
    public void SetOverlayAlpha(float alpha)
    {
        if (canvasGroup != null)
            canvasGroup.alpha = Mathf.Clamp01(alpha);
    }

    /// <summary>
    /// 검은 화면에서 시작해서 페이드 인 (눈 뜨는 연출 등)
    /// </summary>
    public void FadeInFromBlack(float duration = -1f, System.Action onComplete = null)
    {
        if (isFading) return;
        float dur = duration > 0 ? duration : defaultFadeDuration;
        StartCoroutine(FadeInFromBlackRoutine(dur, onComplete));
    }

    IEnumerator FadeInFromBlackRoutine(float duration, System.Action onComplete)
    {
        isFading = true;
        canvasGroup.alpha = 1f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            canvasGroup.alpha = 1f - Mathf.Clamp01(t);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        isFading = false;
        onComplete?.Invoke();
    }

    /// <summary>
    /// 페이드 아웃만 (검은 화면에서 멈춤). onComplete에서 텔레포트 등 처리 후 FadeInFromBlack 호출.
    /// </summary>
    public void FadeOut(float duration = -1f, System.Action onComplete = null)
    {
        if (isFading) return;
        float dur = duration > 0 ? duration : defaultFadeDuration;
        StartCoroutine(FadeOutRoutine(dur, onComplete));
    }

    IEnumerator FadeOutRoutine(float duration, System.Action onComplete)
    {
        isFading = true;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            canvasGroup.alpha = Mathf.Clamp01(t);
            yield return null;
        }
        canvasGroup.alpha = 1f;
        isFading = false;
        onComplete?.Invoke();
    }

    /// <summary>
    /// 페이드 아웃 → 중간 콜백 → 페이드 인 (스타듀밸리 스타일)
    /// onMid: 화면이 완전히 검은 상태에서 호출 (텔레포트 등)
    /// onComplete: 페이드 인이 완전히 끝난 뒤 호출
    /// </summary>
    public void FadeOutIn(System.Action onMid, float fadeOutDuration = -1f, float fadeInDuration = -1f, System.Action onComplete = null)
    {
        if (isFading) return;
        float outDur = fadeOutDuration > 0 ? fadeOutDuration : defaultFadeDuration;
        float inDur = fadeInDuration > 0 ? fadeInDuration : defaultFadeDuration;

        StartCoroutine(FadeOutInRoutine(onMid, outDur, inDur, onComplete));
    }

    IEnumerator FadeOutInRoutine(System.Action onMid, float outDur, float inDur, System.Action onComplete)
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

        // 페이드 인 완료 콜백
        onComplete?.Invoke();
    }
}
