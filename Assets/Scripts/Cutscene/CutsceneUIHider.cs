using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 컷신 중 UI 숨김. CutsceneManager 이벤트에 구독.
/// Canvas의 HUD 또는 숨길 UI 부모를 할당.
/// </summary>
public class CutsceneUIHider : MonoBehaviour
{
    [Header("컷신 중 숨길 UI")]
    [Tooltip("숨길 오브젝트 (HUD 부모 등). 여러 개면 모두 추가")]
    public List<GameObject> uiToHide = new List<GameObject>();

    void Start()
    {
        StartCoroutine(SubscribeWhenReady());
    }

    System.Collections.IEnumerator SubscribeWhenReady()
    {
        while (CutsceneManager.Instance == null)
            yield return null;

        CutsceneManager.Instance.OnCutsceneStarted += OnCutsceneStarted;
        CutsceneManager.Instance.OnCutsceneEnded += OnCutsceneEnded;

        if (CutsceneManager.Instance.IsPlaying)
            OnCutsceneStarted(null);
    }

    void OnDestroy()
    {
        if (CutsceneManager.Instance != null)
        {
            CutsceneManager.Instance.OnCutsceneStarted -= OnCutsceneStarted;
            CutsceneManager.Instance.OnCutsceneEnded -= OnCutsceneEnded;
        }
    }

    void OnCutsceneStarted(CutsceneData _)
    {
        foreach (var go in uiToHide)
            if (go != null)
                go.SetActive(false);
    }

    void OnCutsceneEnded(CutsceneData _)
    {
        foreach (var go in uiToHide)
            if (go != null)
                go.SetActive(true);
    }
}
