using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 벽난로: 우클릭 시 fire 자식 활성/비활성 토글 + 광원 동기화
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FireplaceInteract : MonoBehaviour
{
    [Header("참조")]
    [Tooltip("불 애니메이션 자식 (비어있으면 'fire' 이름으로 찾음)")]
    public GameObject fireObject;

    [Tooltip("불 켜질 때 함께 켜질 광원 오브젝트 (Spot Light 2D 등)")]
    public GameObject lightObject;

    [Tooltip("또는 Light2D 컴포넌트 직접 지정")]
    public Light2D light2D;

    void Awake()
    {
        if (fireObject == null)
            fireObject = transform.Find("fire")?.gameObject;
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(1)) return;
        if (GameInputBlocker.IsBlocked) return; // 메뉴/팝업 열려있으면 무시

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var hits = Physics2D.RaycastAll(mouseWorld, Vector2.zero);

        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;
            if (hit.collider.transform != transform && !hit.collider.transform.IsChildOf(transform))
                continue;

            ToggleFire();
            return;
        }
    }

    void ToggleFire()
    {
        if (fireObject == null) return;

        bool next = !fireObject.activeSelf;
        fireObject.SetActive(next);

        if (lightObject != null)
            lightObject.SetActive(next);
        else if (light2D != null)
            light2D.enabled = next;
    }
}
