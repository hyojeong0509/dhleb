using UnityEngine;

/// <summary>
/// Y 좌표 기반 깊이 정렬. Y가 작을수록(화면 아래) 앞에 그려짐.
/// 타일맵 레이어(-10, -5, -1 등)보다 앞에 그리려면 baseOrderOffset을 타일맵 최대값(-1)보다 크게 설정.
/// 예: 타일맵이 -10, -5, -1 이면 baseOrderOffset = 1000 (Y 0~10에서 order 1000~0)
/// </summary>
public static class DepthSortByY
{
    const int MULTIPLIER = 100;

    /// <summary>타일맵 위에 그리기 위한 기본 오프셋. 타일맵 최대 sortingOrder(-1)보다 커야 함.</summary>
    public static int baseOrderOffset = 1000;

    /// <summary>Transform의 모든 SpriteRenderer에 Y 기반 sortingOrder 적용</summary>
    public static void Apply(Transform t)
    {
        if (t == null) return;
        int order = baseOrderOffset + Mathf.RoundToInt(-t.position.y * MULTIPLIER);
        foreach (var sr in t.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (sr != null)
                sr.sortingOrder = order;
        }
    }

    /// <summary>단일 SpriteRenderer에 적용</summary>
    public static void Apply(SpriteRenderer sr)
    {
        if (sr == null) return;
        sr.sortingOrder = baseOrderOffset + Mathf.RoundToInt(-sr.transform.position.y * MULTIPLIER);
    }
}
