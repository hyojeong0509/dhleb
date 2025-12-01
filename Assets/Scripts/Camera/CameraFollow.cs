using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("추적할 타겟 (플레이어)")]
    public Transform target;

    [Header("Follow Settings")]
    [Tooltip("카메라 오프셋 (플레이어 기준 위치)")]
    public Vector3 offset = new Vector3(0, 0, -10);
    
    [Tooltip("추적 방식")]
    public FollowMode followMode = FollowMode.Smooth;
    
    [Tooltip("추적 속도 (높을수록 빨리 따라감")]
    [Range(0f, 50f)]
    public float smoothSpeed = 30f;
    
    [Tooltip("추적을 시작할 거리 (이 거리 이상 멀어지면 따라감)")]
    public float followThreshold = 0.01f;
    
    public enum FollowMode
    {
        Instant,    // 즉시 따라감 (지연 없음)
        Smooth      // 부드럽게 따라감
    }

    private Vector3 velocity = Vector3.zero;

    void Start()
    {
        // 타겟이 설정되지 않았으면 플레이어 찾기
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogWarning("CameraFollow: Player 태그를 가진 오브젝트를 찾을 수 없습니다. Inspector에서 타겟을 직접 설정해주세요.");
            }
        }
        
        // 게임 시작 시 즉시 타겟 위치로 이동
        SnapToTarget();
        velocity = Vector3.zero; // velocity 초기화
    }

    void FixedUpdate()
    {
        if (target == null)
            return;

        // 목표 위치 계산
        Vector3 targetPosition = target.position + offset;
        
        // 현재 카메라 위치
        Vector3 currentPosition = transform.position;

        if (followMode == FollowMode.Instant)
        {
            // 즉시 따라감
            transform.position = targetPosition;
        }
        else // FollowMode.Smooth
        {
            // 타겟과의 거리 계산
            float distance = Vector3.Distance(currentPosition, targetPosition);
            
            // 일정 거리 이상 멀어졌을 때만 추적
            if (distance > followThreshold)
            {
                // 부드러운 추적
                transform.position = Vector3.SmoothDamp(
                    currentPosition,
                    targetPosition,
                    ref velocity,
                    1f / smoothSpeed,
                    Mathf.Infinity,
                    Time.fixedDeltaTime
                );
            }
        }
    }

    // 즉시 타겟 위치로 이동
    public void SnapToTarget()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }
}

