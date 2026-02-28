using UnityEngine;

/// <summary>
/// 상점 영역 트리거 - 플레이어가 트리거 안에 들어가면 F키로 상점 열기
/// 상점 건물 내부 바닥/벽에 Collider2D(Trigger) + 이 컴포넌트 부착
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ShopZoneTrigger : MonoBehaviour
{
    [Header("상점 데이터")]
    public ShopData shopData;

    [Header("설정")]
    public KeyCode interactKey = KeyCode.F;
    public string playerTag = "Player";

    private bool isPlayerInside;

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger) col.isTrigger = true;

        if (GetComponent<Rigidbody2D>() == null)
        {
            var rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            isPlayerInside = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            isPlayerInside = false;
    }

    void Update()
    {
        if (!isPlayerInside || shopData == null) return;
        if (ShopManager.Instance != null && ShopManager.Instance.IsShopOpen) return;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying) return;
        if (CutsceneManager.Instance != null && CutsceneManager.Instance.IsPlaying) return;

        if (Input.GetKeyDown(interactKey))
            ShopManager.Instance?.OpenShop(shopData);
    }
}
