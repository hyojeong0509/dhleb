using UnityEngine;
using System.Collections;

/// <summary>
/// 지정한 컷신이 끝나면 오브젝트를 활성화.
/// ★ Triones(활성 오브젝트)에 붙여야 함. GuideImage(비활성)에 붙이면 동작 안 함.
/// </summary>
public class WakeUpGuideActivator : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("이 컷신이 끝나면 활성화 (비우면 Cutscene_WakeUp)")]
    public CutsceneData triggerCutscene;
    [Tooltip("활성화할 오브젝트 (보통 자식 이미지)")]
    public GameObject objectToActivate;

    void Start()
    {
        StartCoroutine(SubscribeWhenReady());
    }

    IEnumerator SubscribeWhenReady()
    {
        while (CutsceneManager.Instance == null)
            yield return null;

        CutsceneManager.Instance.OnCutsceneEnded += OnCutsceneEnded;
    }

    void OnDestroy()
    {
        if (CutsceneManager.Instance != null)
            CutsceneManager.Instance.OnCutsceneEnded -= OnCutsceneEnded;
    }

    void OnCutsceneEnded(CutsceneData data)
    {
        if (data == null || objectToActivate == null) return;

        string triggerName = triggerCutscene != null ? triggerCutscene.name : "Cutscene_WakeUp";
        if (data.name != triggerName) return;

        objectToActivate.SetActive(true);
    }
}
