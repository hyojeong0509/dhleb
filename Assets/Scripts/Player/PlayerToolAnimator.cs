using UnityEngine;
using System.Collections;

/// <summary>
/// 도구 사용 시 마우스 방향에 맞는 애니메이션 재생
/// Animator 파라미터: UseTool (Trigger), ToolDirection (Int: 0=정면, 1=우측, 2=뒤), ToolFaceLeft (Bool: 좌측이면 flipX)
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerToolAnimator : MonoBehaviour
{
    [Tooltip("디버그: Console에 트리거 로그 출력")]
    public bool debugLog;

    [Tooltip("애니메이션 길이를 가져오지 못할 때 사용할 폴백 잠금 시간 (초)")]
    public float toolUseLockDurationFallback = 1.1f;

    public static bool IsUsingTool { get; private set; }

    private static readonly int[] ToolStateHashes = {
        Animator.StringToHash("Interaction_Down"),
        Animator.StringToHash("Interaction_Up"),
        Animator.StringToHash("Interaction_Right")
    };

    private Animator anim;
    private SpriteRenderer sr;

    void Awake()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 플레이어 기준 마우스 방향으로 도구 애니메이션 재생
    /// </summary>
    public void PlayToolUse(Vector3 targetWorldPos)
    {
        if (anim == null) return;

        // 파라미터 존재 여부 확인 (없으면 로그)
        if (!HasParam("UseTool") || !HasParam("ToolDirection"))
        {
            Debug.LogWarning("[PlayerToolAnimator] Animator에 UseTool(Trigger), ToolDirection(Int) 파라미터가 필요합니다.");
            return;
        }

        Vector2 dir = (targetWorldPos - transform.position).normalized;

        // 0=정면(아래), 1=우측, 2=뒤(위) / 좌측은 우측+flipX
        // 수평이 더 크면 좌우, 수직이 더 크면 위/아래
        int toolDir;
        bool faceLeft = dir.x < 0;

        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))  // 좌우가 우선
            toolDir = 1;
        else if (dir.y < 0)  // 아래 (정면)
            toolDir = 0;
        else  // 위 (뒤)
            toolDir = 2;

        if (sr != null)
            sr.flipX = faceLeft;

        anim.SetInteger("ToolDirection", toolDir);
        anim.SetTrigger("UseTool");

        StartCoroutine(LockMovementDuringToolUse());

        if (debugLog)
            Debug.Log($"[PlayerToolAnimator] UseTool 트리거, ToolDirection={toolDir}");
    }

    IEnumerator LockMovementDuringToolUse()
    {
        IsUsingTool = true;

        // 트랜지션 완료 후 도구 상태 진입 대기 (최대 15프레임)
        for (int i = 0; i < 15; i++)
        {
            yield return null;
            if (IsToolState(anim.GetCurrentAnimatorStateInfo(0))) break;
        }

        // 도구 애니메이션이 끝날 때까지 대기 (상태에서 벗어날 때까지)
        float timeout = toolUseLockDurationFallback;
        float elapsed = 0f;
        while (IsToolState(anim.GetCurrentAnimatorStateInfo(0)) && elapsed < timeout)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }

        IsUsingTool = false;
    }

    bool IsToolState(AnimatorStateInfo stateInfo)
    {
        int hash = stateInfo.shortNameHash;
        foreach (int h in ToolStateHashes)
            if (hash == h) return true;
        return false;
    }

    bool HasParam(string name)
    {
        foreach (var p in anim.parameters)
            if (p.name == name) return true;
        return false;
    }
}
