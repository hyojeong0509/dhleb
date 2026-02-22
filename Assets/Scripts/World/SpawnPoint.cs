using UnityEngine;

/// <summary>
/// 문 입구/출구용 스폰 포인트
/// 플레이어가 이 영역에 들어오면 → 페이드 아웃 → 텔레포트 → 페이드 인 (스타듀밸리 스타일)
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SpawnPoint : MonoBehaviour
{
    [Header("이 영역에 들어오면 이동할 위치")]
    [Tooltip("다른 SpawnPoint를 드래그")]
    public SpawnPoint targetSpawnPoint;

    [Tooltip("또는 아무 오브젝트(빈 GameObject 포함)를 드래그 → 그 위치로 이동")]
    public Transform targetTransform;

    [Tooltip("위 둘 다 없을 때 사용할 좌표")]
    public Vector3 targetPosition;

    [Header("기타")]
    public string playerTag = "Player";

    [Tooltip("텔레포트 후 이 시간(초) 동안 다른 트리거 무시 (무한 루프 방지)")]
    public float teleportCooldown = 0.5f;

    [Tooltip("페이드 지속 시간 (0이면 페이드 없이 즉시 이동)")]
    public float fadeDuration = 0.4f;

    static float lastTeleportTime;

    void Start()
    {
        var col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
            col.isTrigger = true;

        if (GetComponent<Rigidbody2D>() == null)
        {
            var rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (Time.time - lastTeleportTime < teleportCooldown) return;

        var rb = other.GetComponentInParent<Rigidbody2D>();
        Transform moveTarget = rb != null ? rb.transform : other.transform;

        Vector3 dest = targetSpawnPoint != null
            ? targetSpawnPoint.transform.position
            : (targetTransform != null ? targetTransform.position : targetPosition);

        if (fadeDuration > 0f && ScreenFadeManager.Instance != null)
        {
            lastTeleportTime = Time.time;
            ScreenFadeManager.Instance.FadeOutIn(
                () => DoTeleport(moveTarget, rb, dest),
                fadeDuration,
                fadeDuration
            );
        }
        else
        {
            DoTeleport(moveTarget, rb, dest);
            lastTeleportTime = Time.time;
        }
    }

    void DoTeleport(Transform moveTarget, Rigidbody2D rb, Vector3 dest)
    {
        Vector3 newPos = new Vector3(dest.x, dest.y, moveTarget.position.z);
        moveTarget.position = newPos;
        if (rb != null)
            rb.position = new Vector2(dest.x, dest.y);
    }
}
