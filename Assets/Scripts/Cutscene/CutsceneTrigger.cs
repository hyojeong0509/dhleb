using UnityEngine;

/// <summary>
/// 구역 진입 시 컷신 재생. Collider2D(Trigger) 필요.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CutsceneTrigger : MonoBehaviour
{
    [Header("컷신")]
    public CutsceneData cutscene;

    [Header("조건 (모두 만족 시 재생)")]
    [Tooltip("스토리 진행도 최소값 (-1 = 무시)")]
    public int storyProgressMin = -1;
    [Tooltip("필요 플래그 (하나라도 있으면 OK)")]
    public string[] flagsRequired;
    [Tooltip("차단 플래그 (있으면 재생 안 함)")]
    public string[] flagsBlock;

    [Header("재생 후")]
    [Tooltip("재생 후 설정할 플래그 (중복 재생 방지)")]
    public string setFlagWhenDone;

    [Header("기타")]
    public string playerTag = "Player";

    void Start()
    {
        var col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
            col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (cutscene == null) return;
        if (!AreConditionsMet()) return;

        if (CutsceneManager.Instance != null)
        {
            CutsceneManager.Instance.Play(cutscene, () =>
            {
                if (!string.IsNullOrEmpty(setFlagWhenDone) && GameProgressManager.Instance != null)
                    GameProgressManager.Instance.SetFlag(setFlagWhenDone);
            });
        }
    }

    bool AreConditionsMet()
    {
        if (GameProgressManager.Instance == null) return false;

        if (!string.IsNullOrEmpty(setFlagWhenDone) && GameProgressManager.Instance.HasFlag(setFlagWhenDone))
            return false;

        if (storyProgressMin >= 0 && GameProgressManager.Instance.StoryProgress < storyProgressMin)
            return false;

        if (flagsBlock != null)
            foreach (var f in flagsBlock)
                if (!string.IsNullOrEmpty(f) && GameProgressManager.Instance.HasFlag(f))
                    return false;

        if (flagsRequired != null && flagsRequired.Length > 0)
        {
            bool hasAny = false;
            foreach (var f in flagsRequired)
                if (!string.IsNullOrEmpty(f) && GameProgressManager.Instance.HasFlag(f))
                { hasAny = true; break; }
            if (!hasAny) return false;
        }

        return true;
    }
}
