using UnityEngine;

/// <summary>
/// 플레이어가 트리거에 진입하면 플래그 설정. 퀘스트 목표(SetFlag) 등에 사용.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FlagTrigger : MonoBehaviour
{
    [Header("설정")]
    public string flagName = "visited_friendly_npc_house";
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
        if (string.IsNullOrEmpty(flagName)) return;

        if (GameProgressManager.Instance != null)
            GameProgressManager.Instance.SetFlag(flagName); // SetFlag가 QuestManager.NotifyFlagSet 호출
    }
}
