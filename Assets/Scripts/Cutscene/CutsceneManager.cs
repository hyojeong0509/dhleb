using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 컷신 재생. 시간 정지, 플레이어 입력 차단.
/// </summary>
public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }

    public bool IsPlaying { get; private set; }

    /// <summary>NpcGroupMove 시 저장한 NPC별 시작 위치 (NpcGroupReturnToStart에서 사용)</summary>
    static Dictionary<string, Vector3> _npcStartPositions = new Dictionary<string, Vector3>();
    /// <summary>마지막 NpcGroupMove의 이동 시간 (NpcGroupReturnToStart에서 npcReturnDuration 0일 때 사용)</summary>
    static float _lastNpcMoveDuration = 2f;

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

        // Time.timeScale은 건드리지 않음 (0이면 Animator가 멈추므로, 1 유지로 애니메이션 재생)
        float prevTimeScale = Time.timeScale;
        Time.timeScale = 1f;

        var player = GameObject.FindGameObjectWithTag("Player");
        Animator playerAnim = player != null ? player.GetComponent<Animator>() : null;
        var prevUpdateMode = playerAnim != null ? playerAnim.updateMode : AnimatorUpdateMode.Normal;
        if (playerAnim != null)
            playerAnim.updateMode = AnimatorUpdateMode.Normal; // timeScale 1이므로 Normal로 충분

        OnCutsceneStarted?.Invoke(data);

        foreach (var action in data.actions)
        {
            if (action == null) continue;
            yield return ExecuteAction(action);
        }

        IsPlaying = false;
        if (playerAnim != null)
            playerAnim.updateMode = prevUpdateMode;
        Time.timeScale = prevTimeScale;
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
                yield return RunPushPlayer(action);
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

            case CutsceneActionType.NpcGroupMove:
                yield return RunNpcGroupMove(action);
                break;

            case CutsceneActionType.NpcGroupReturnToStart:
                yield return RunNpcGroupReturnToStart(action);
                break;

            case CutsceneActionType.ShowNotification:
                if (NotificationPopupManager.Instance != null && !string.IsNullOrEmpty(action.notificationText))
                    NotificationPopupManager.Instance.Show(action.notificationText, action.notificationDuration > 0 ? action.notificationDuration : 2.5f);
                break;

            case CutsceneActionType.AcceptQuest:
                if (!string.IsNullOrEmpty(action.questId) && QuestManager.Instance != null)
                    QuestManager.Instance.AcceptQuest(action.questId);
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

    IEnumerator RunPushPlayer(CutsceneAction action)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) yield break;

        // 밀치는 NPC 애니메이션 재생 (NPCWander 비활성화 → 애니 재생 → 밀치기)
        if (!string.IsNullOrEmpty(action.pushNpcId))
        {
            var refs = FindObjectsOfType<CutsceneNpcRef>(true);
            foreach (var r in refs)
            {
                if (r.npcId != action.pushNpcId) continue;

                var pushNpcWander = r.GetComponent<NPCWander>();
                if (pushNpcWander != null)
                    pushNpcWander.enabled = false; // 밀치기 애니 재생 중 배회 방지

                // 밀치는 방향에 맞게 스프라이트 뒤집기 (왼쪽 밀면 flipX=true)
                var sr = r.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                    sr.flipX = action.pushDirection.x < 0;

                var anim = r.GetComponent<Animator>();
                if (anim == null) anim = r.GetComponentInChildren<Animator>(true);
                if (anim != null)
                {
                    if (!string.IsNullOrEmpty(action.pushNpcStateName))
                        anim.Play(action.pushNpcStateName, 0, 0f);
                    else if (!string.IsNullOrEmpty(action.pushNpcTrigger))
                        anim.SetTrigger(action.pushNpcTrigger);
                    anim.Update(0f); // 즉시 전환 적용
                    yield return null;
                }
                yield return new WaitForSecondsRealtime(0.12f);
                break;
            }
            // NPCWander는 NpcGroupReturnToStart에서 다시 활성화됨
        }

        Vector2 dir = action.pushDirection.normalized;
        float distance = action.pushDistance;
        float duration = action.pushDuration > 0 ? action.pushDuration : 0.3f;
        float arcHeight = 0.4f; // 점프처럼 살짝 뜨는 높이

        Vector3 start = player.transform.position;
        Vector3 end = start + (Vector3)(dir * distance);
        end.z = start.z;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easeOut = 1f - (1f - t) * (1f - t); // EaseOutQuad
            float arc = 4f * t * (1f - t); // 0→1→0 포물선

            Vector3 pos = Vector3.Lerp(start, end, easeOut);
            pos.y += arcHeight * arc;

            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.MovePosition(pos);
            else
                player.transform.position = pos;

            yield return null;
        }

        if (player != null)
        {
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.MovePosition(end);
            else
                player.transform.position = end;
        }
    }

    IEnumerator RunNpcGroupMove(CutsceneAction action)
    {
        if (action.npcIds == null || action.npcIds.Count == 0 ||
            action.npcTargetPositions == null || action.npcTargetPositions.Count == 0)
        {
            yield break;
        }

        // 플레이어 위치 기준이면 컷신 시작 시점의 플레이어 좌표를 원점으로 사용
        Vector3 origin = Vector3.zero;
        if (action.usePlayerPositionAsOrigin)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                origin = player.transform.position;
        }

        var refs = FindObjectsOfType<CutsceneNpcRef>(true);
        var npcMap = new Dictionary<string, Transform>();
        foreach (var r in refs)
        {
            if (!string.IsNullOrEmpty(r.npcId) && !npcMap.ContainsKey(r.npcId))
                npcMap[r.npcId] = r.transform;
        }

        var npcs = new List<(Transform tr, Vector3 target, Rigidbody2D rb, SpriteRenderer sr, Animator anim, NPCWander wander)>();
        bool anyFound = false;
        for (int i = 0; i < action.npcIds.Count && i < action.npcTargetPositions.Count; i++)
        {
            if (!npcMap.TryGetValue(action.npcIds[i], out Transform tr)) continue;
            anyFound = true;
            Vector3 target = action.usePlayerPositionAsOrigin
                ? origin + action.npcTargetPositions[i]
                : action.npcTargetPositions[i];
            var rb = tr.GetComponent<Rigidbody2D>();
            var sr = tr.GetComponentInChildren<SpriteRenderer>();
            var anim = tr.GetComponentInChildren<Animator>();
            var wander = tr.GetComponent<NPCWander>();
            npcs.Add((tr, target, rb, sr, anim, wander));
        }
        if (!anyFound) yield break;

        float duration = Mathf.Max(0.1f, action.npcMoveDuration);
        _lastNpcMoveDuration = duration;

        // NPCWander 비활성화
        foreach (var (_, _, _, _, _, wander) in npcs)
        {
            if (wander != null) wander.enabled = false;
        }

        // 걷기 애니메이션 + 이동 (병렬)
        var startPositions = new List<Vector3>();
        _npcStartPositions.Clear();
        for (int i = 0; i < npcs.Count; i++)
        {
            var (tr, _, _, _, _, _) = npcs[i];
            Vector3 pos = tr.position;
            startPositions.Add(pos);
            if (i < action.npcIds.Count)
                _npcStartPositions[action.npcIds[i]] = pos;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            for (int i = 0; i < npcs.Count; i++)
            {
                var (tr, target, rb, sr, anim, _) = npcs[i];
                Vector3 start = startPositions[i];
                target.z = start.z;
                Vector3 pos = Vector3.Lerp(start, target, t);

                if (rb != null)
                    rb.MovePosition(pos);
                else
                    tr.position = pos;

                if (sr != null)
                {
                    float dirX = (target - start).x;
                    sr.flipX = dirX < 0;
                }
                if (anim != null) SetAnimSpeed(anim, 1f);
            }
            yield return null;
        }

        // 최종 위치 보정
        for (int i = 0; i < npcs.Count; i++)
        {
            var (tr, target, rb, sr, anim, _) = npcs[i];
            target.z = tr.position.z;
            if (rb != null) rb.MovePosition(target);
            else tr.position = target;
            if (anim != null) SetAnimSpeed(anim, 0f);
        }

        if (npcs.Count > 0)
            yield return new WaitForSecondsRealtime(0.2f);

        foreach (var (_, _, _, _, _, wander) in npcs)
        {
            if (wander != null) wander.enabled = true;
        }
    }

    IEnumerator RunNpcGroupReturnToStart(CutsceneAction action)
    {
        var ids = action.npcReturnIds != null && action.npcReturnIds.Count > 0
            ? action.npcReturnIds
            : new List<string>(_npcStartPositions.Keys);
        if (ids.Count == 0) yield break;

        var refs = FindObjectsOfType<CutsceneNpcRef>(true);
        var npcMap = new Dictionary<string, Transform>();
        foreach (var r in refs)
        {
            if (!string.IsNullOrEmpty(r.npcId) && !npcMap.ContainsKey(r.npcId))
                npcMap[r.npcId] = r.transform;
        }

        var npcs = new List<(Transform tr, Vector3 target, Rigidbody2D rb, SpriteRenderer sr, Animator anim, NPCWander wander)>();
        foreach (var id in ids)
        {
            if (!_npcStartPositions.TryGetValue(id, out Vector3 home) || !npcMap.TryGetValue(id, out Transform tr))
                continue;
            var rb = tr.GetComponent<Rigidbody2D>();
            var sr = tr.GetComponentInChildren<SpriteRenderer>();
            var anim = tr.GetComponentInChildren<Animator>();
            var wander = tr.GetComponent<NPCWander>();
            npcs.Add((tr, home, rb, sr, anim, wander));
        }
        if (npcs.Count == 0) yield break;

        float baseDuration = action.npcReturnDuration > 0 ? action.npcReturnDuration : _lastNpcMoveDuration;
        float mult = action.npcReturnDurationMultiplier > 0.01f ? action.npcReturnDurationMultiplier : 2f; // 기본 2배
        float duration = Mathf.Max(4f, baseDuration * mult); // 최소 4초

        foreach (var (_, _, _, _, _, wander) in npcs)
        {
            if (wander != null) wander.enabled = false;
        }

        // 복귀 시작 위치 고정 (매 프레임 tr.position 쓰면 lerp가 깨짐)
        var startPositions = new List<Vector3>();
        foreach (var (tr, target, _, _, _, _) in npcs)
        {
            var p = tr.position;
            startPositions.Add(p);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            for (int i = 0; i < npcs.Count; i++)
            {
                var (tr, target, rb, sr, anim, _) = npcs[i];
                Vector3 start = startPositions[i];
                target.z = start.z;
                Vector3 pos = Vector3.Lerp(start, target, t);

                if (rb != null)
                    rb.MovePosition(pos);
                else
                    tr.position = pos;

                if (sr != null)
                {
                    float dirX = (target - start).x;
                    sr.flipX = dirX < 0;
                }
                if (anim != null) SetAnimSpeed(anim, 1f);
            }
            yield return null;
        }

        for (int i = 0; i < npcs.Count; i++)
        {
            var (tr, target, rb, sr, anim, _) = npcs[i];
            target.z = tr.position.z;
            if (rb != null) rb.MovePosition(target);
            else tr.position = target;
            if (anim != null) SetAnimSpeed(anim, 0f);
        }

        foreach (var (_, _, _, _, _, wander) in npcs)
        {
            if (wander != null) wander.enabled = true;
        }
    }

    static void SetAnimSpeed(Animator anim, float speed)
    {
        if (anim == null) return;
        foreach (var p in anim.parameters)
        {
            if (p.name == "Speed") { anim.SetFloat("Speed", speed); return; }
        }
    }
}
