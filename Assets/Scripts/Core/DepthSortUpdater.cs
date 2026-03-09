using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 이동하는 오브젝트용 - LateUpdate에서 Y 기반 sortingOrder 갱신.
/// SpriteRenderer 캐싱으로 GetComponentsInChildren 매 프레임 호출 방지.
/// </summary>
public class DepthSortUpdater : MonoBehaviour
{
    SpriteRenderer[] _cachedRenderers;
    int _lastOrder = int.MinValue;

    void Awake()
    {
        _cachedRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    void LateUpdate()
    {
        int order = DepthSortByY.baseOrderOffset + Mathf.RoundToInt(-transform.position.y * 100);
        if (order == _lastOrder) return; // 변경 없으면 스킵
        _lastOrder = order;

        for (int i = 0; i < _cachedRenderers.Length; i++)
        {
            if (_cachedRenderers[i] != null)
                _cachedRenderers[i].sortingOrder = order;
        }
    }
}
