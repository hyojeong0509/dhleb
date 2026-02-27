using UnityEngine;

/// <summary>
/// 상점 열기 트리거 - NPC나 건물에 부착
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ShopInteract : MonoBehaviour
{
    [Header("상점 데이터")]
    public ShopData shopData;

    [Header("상호작용 거리 (비워두면 항상 가능)")]
    public float interactRange = 2f;

    [Tooltip("F키로 상점 열기 (true면 F키 지원)")]
    public bool useInteractKey = true;
    public KeyCode interactKey = KeyCode.F;

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger) col.isTrigger = true;
    }

    void Update()
    {
        if (!useInteractKey || shopData == null) return;
        if (ShopManager.Instance != null && ShopManager.Instance.IsShopOpen) return;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying) return;
        if (CutsceneManager.Instance != null && CutsceneManager.Instance.IsPlaying) return;

        if (Input.GetKeyDown(interactKey) && IsPlayerInRange())
            OpenShop();
    }

    public bool IsPlayerInRange()
    {
        if (interactRange <= 0f) return true;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.transform.position) <= interactRange;
    }

    public void OpenShop()
    {
        if (shopData == null)
        {
            Debug.LogWarning("[ShopInteract] shopData가 설정되지 않았습니다.", this);
            return;
        }
        ShopManager.Instance?.OpenShop(shopData);
    }
}
