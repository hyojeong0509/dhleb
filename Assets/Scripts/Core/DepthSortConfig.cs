using UnityEngine;

/// <summary>
/// 씬에 배치하여 DepthSortByY.baseOrderOffset 설정.
/// 타일맵이 -10, -5, -1 이면 1000 이상 권장 (예: 1000).
/// </summary>
public class DepthSortConfig : MonoBehaviour
{
    [Tooltip("타일맵 최대 sortingOrder(-1)보다 커야 오브젝트가 앞에 그려짐")]
    public int baseOrderOffset = 1000;

    void Awake()
    {
        DepthSortByY.baseOrderOffset = baseOrderOffset;
    }
}
