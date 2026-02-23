using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// 컷신 재생. 시간 정지, 플레이어 입력 차단.
/// </summary>
public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }

    public bool IsPlaying { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public event Action<CutsceneData> OnCutsceneStarted;
    public event Action<CutsceneData> OnCutsceneEnded;

    public void Play(CutsceneData data, Action onComplete = null)
    {
        if (data == null || data.actions == null || data.actions.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        if (IsPlaying)
        {
            Debug.LogWarning("[CutsceneManager] 이미 컷신 재생 중");
            return;
        }

        StartCoroutine(PlayRoutine(data, onComplete));
    }

    IEnumerator PlayRoutine(CutsceneData data, Action onComplete)
    {
        IsPlaying = true;
        if (TimeManager.Instance != null)
            TimeManager.Instance.SetPause(true);

        var player = GameObject.FindGameObjectWithTag("Player");
        Animator playerAnim = player != null ? player.GetComponent<Animator>() : null;
        var prevUpdateMode = playerAnim != null ? playerAnim.updateMode : AnimatorUpdateMode.Normal;
        if (playerAnim != null)
            playerAnim.updateMode = AnimatorUpdateMode.UnscaledTime;

        OnCutsceneStarted?.Invoke(data);

        foreach (var action in data.actions)
        {
            if (action == null) continue;
            yield return ExecuteAction(action);
        }

        IsPlaying = false;
        if (playerAnim != null)
            playerAnim.updateMode = prevUpdateMode;
        if (TimeManager.Instance != null)
            TimeManager.Instance.SetPause(false);

        OnCutsceneEnded?.Invoke(data);
        onComplete?.Invoke();
    }

    IEnumerator ExecuteAction(CutsceneAction action)
    {
        switch (action.type)
        {
            case CutsceneActionType.ShowDialogue:
                yield return RunDialogue(action.dialogue);
                break;

            case CutsceneActionType.Wait:
                yield return new WaitForSecondsRealtime(action.waitDuration);
                break;

            case CutsceneActionType.FadeInFromBlack:
                yield return RunFadeInFromBlack(action.fadeInDuration);
                break;

            case CutsceneActionType.CameraZoomOut:
                yield return RunCameraZoomOut(action.zoomedInSize, action.zoomOutDuration);
                break;

            case CutsceneActionType.PlayerLookLeft:
                yield return RunPlayerLook(true, action.lookDuration);
                break;

            case CutsceneActionType.PlayerLookRight:
                yield return RunPlayerLook(false, action.lookDuration);
                break;

            case CutsceneActionType.CameraZoomToTarget:
                yield return RunCameraZoomToTarget(action.targetPosition, action.targetZoomedSize,
                    action.zoomInDuration, action.holdDuration, action.zoomOutDurationTarget);
                break;

            case CutsceneActionType.PushPlayer:
                PushPlayer(action.pushDirection, action.pushDistance);
                yield return new WaitForSecondsRealtime(0.3f);
                break;

            case CutsceneActionType.SetAnimatorTrigger:
                SetPlayerAnimatorTrigger(action.triggerName);
                break;

            case CutsceneActionType.SetActive:
                if (action.targetObject != null)
                    action.targetObject.SetActive(action.setActive);
                break;

            case CutsceneActionType.SetFlag:
                if (!string.IsNullOrEmpty(action.flagName) && GameProgressManager.Instance != null)
                    GameProgressManager.Instance.SetFlag(action.flagName);
                break;

            case CutsceneActionType.AdvanceStory:
                if (GameProgressManager.Instance != null)
                    GameProgressManager.Instance.AdvanceStoryProgress(action.storyAmount);
                break;

            case CutsceneActionType.AddAffection:
                if (!string.IsNullOrEmpty(action.npcId) && GameProgressManager.Instance != null)
                    GameProgressManager.Instance.AddAffection(action.npcId, action.affectionAmount);
                break;

            default:
                break;
        }
    }

    /// <summary>눈 뜨는 컷신 전에 저장해둔 기본 orthographicSize</summary>
    public static float WakeUpDefaultOrthoSize { get; set; }

    IEnumerator RunPlayerLook(bool lookLeft, float duration)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var movement = player.GetComponent<PlayerMovement>();
            if (movement != null)
                movement.SetFacingLeft(lookLeft);
        }
        yield return new WaitForSecondsRealtime(duration);
    }

    IEnumerator RunCameraZoomToTarget(Vector3 targetPos, float zoomedSize, float zoomInDur, float holdDur, float zoomOutDur)
    {
        var cam = Camera.main;
        var follow = cam != null ? cam.GetComponent<CameraFollow>() : null;
        if (cam == null || !cam.orthographic)
        {
            yield return new WaitForSecondsRealtime(zoomInDur + holdDur + zoomOutDur);
            yield break;
        }

        float defaultSize = WakeUpDefaultOrthoSize > 0 ? WakeUpDefaultOrthoSize : cam.orthographicSize;
        Vector3 defaultPos = follow != null && follow.target != null
            ? follow.target.position + follow.offset
            : cam.transform.position;

        if (follow != null) follow.pauseFollow = true;

        targetPos.z = defaultPos.z;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / zoomInDur;
            cam.transform.position = Vector3.Lerp(defaultPos, targetPos, t);
            cam.orthographicSize = Mathf.Lerp(defaultSize, zoomedSize, t);
            yield return null;
        }
        cam.transform.position = targetPos;
        cam.orthographicSize = zoomedSize;

        yield return new WaitForSecondsRealtime(holdDur);

        t = 0f;
        Vector3 startPos = cam.transform.position;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / zoomOutDur;
            cam.transform.position = Vector3.Lerp(startPos, defaultPos, t);
            cam.orthographicSize = Mathf.Lerp(zoomedSize, defaultSize, t);
            yield return null;
        }
        cam.transform.position = defaultPos;
        cam.orthographicSize = defaultSize;

        if (follow != null) follow.pauseFollow = false;
    }

    IEnumerator RunCameraZoomOut(float fromSize, float duration)
    {
        var cam = Camera.main;
        if (cam == null || !cam.orthographic)
        {
            yield return new WaitForSecondsRealtime(duration);
            yield break;
        }

        float toSize = WakeUpDefaultOrthoSize > 0 ? WakeUpDefaultOrthoSize : cam.orthographicSize;
        cam.orthographicSize = fromSize;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            cam.orthographicSize = Mathf.Lerp(fromSize, toSize, t);
            yield return null;
        }
        cam.orthographicSize = toSize;
    }

    IEnumerator RunFadeInFromBlack(float duration)
    {
        if (ScreenFadeManager.Instance == null)
        {
            yield return new WaitForSecondsRealtime(duration);
            yield break;
        }
        bool done = false;
        ScreenFadeManager.Instance.FadeInFromBlack(duration, () => done = true);
        while (!done)
            yield return null;
    }

    IEnumerator RunDialogue(DialogueData dialogue)
    {
        if (dialogue == null || DialogueManager.Instance == null)
            yield break;

        bool done = false;
        DialogueManager.Instance.Play(dialogue, () => done = true);
        while (!done)
            yield return null;
    }

    void SetPlayerAnimatorTrigger(string triggerName)
    {
        if (string.IsNullOrEmpty(triggerName)) return;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        var anim = player.GetComponent<Animator>();
        if (anim != null)
            anim.SetTrigger(triggerName);
    }

    void PushPlayer(Vector2 direction, float distance)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        var movement = player.GetComponent<PlayerMovement>();
        if (movement != null)
            movement.PushBack(direction.normalized * distance);
        else
            player.transform.position += (Vector3)(direction.normalized * distance);
    }
}
