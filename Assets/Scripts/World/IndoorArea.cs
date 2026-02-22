using UnityEngine;

/// <summary>
/// 실내 영역 (집 등)
/// 플레이어가 이 트리거 안에 있으면 IsPlayerIndoor = true
/// 괭이/물뿌리개 범위 하이라이트 숨김 등에 사용
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class IndoorArea : MonoBehaviour
{
    public static bool IsPlayerIndoor => indoorCount > 0;

    private static int indoorCount;

    [Header("설정")]
    public string playerTag = "Player";

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
        if (other.CompareTag(playerTag))
            indoorCount++;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            indoorCount = Mathf.Max(0, indoorCount - 1);
    }

    void OnDisable()
    {
        // 비활성화 시 카운트 초기화 (플레이어가 안에 있어도 영역이 꺼지면 실외로 간주)
        indoorCount = 0;
    }
}
