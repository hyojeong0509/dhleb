using UnityEngine;

/// <summary>
/// 컷신 액션 타입
/// </summary>
public enum CutsceneActionType
{
    ShowDialogue,   // 대화 재생 (완료까지 대기)
    Wait,           // 대기
    FadeInFromBlack,// 검은 화면 → 페이드 인 (눈 뜨는 연출)
    CameraZoomOut,  // 줌 인 상태 → 기본 줌으로 (눈 뜨는 연출)
    PlayerLookLeft, // 플레이어 왼쪽 바라보기
    PlayerLookRight,// 플레이어 오른쪽 바라보기
    CameraZoomToTarget, // 특정 위치 줌인 → 홀드 → 줌아웃
    PushPlayer,     // 플레이어 밀치기
    SetAnimatorTrigger, // 플레이어 Animator 트리거 (눈 뜨는 애니 등)
    SetActive,      // GameObject 활성/비활성
    SetFlag,        // 이벤트 플래그 설정
    AdvanceStory,   // 스토리 진행도 +1
    AddAffection,   // NPC 호감도 추가
    Custom          // (확장용)
}

/// <summary>
/// 컷신 액션 1개
/// </summary>
[System.Serializable]
public class CutsceneAction
{
    public CutsceneActionType type;

    [Header("ShowDialogue")]
    public DialogueData dialogue;

    [Header("Wait")]
    public float waitDuration = 1f;

    [Header("FadeInFromBlack")]
    public float fadeInDuration = 1.5f;

    [Header("CameraZoomOut")]
    [Tooltip("줌 인 상태의 orthographicSize (작을수록 확대)")]
    public float zoomedInSize = 2f;
    [Tooltip("줌 아웃 시간")]
    public float zoomOutDuration = 1.5f;

    [Header("PlayerLookLeft / PlayerLookRight")]
    public float lookDuration = 1f;

    [Header("CameraZoomToTarget")]
    public Vector3 targetPosition;
    [Tooltip("줌 인 시 orthographicSize")]
    public float targetZoomedSize = 2f;
    [Tooltip("줌 인 시간")]
    public float zoomInDuration = 0.5f;
    [Tooltip("홀드 시간")]
    public float holdDuration = 1f;
    [Tooltip("줌 아웃 시간")]
    public float zoomOutDurationTarget = 1f;

    [Header("PushPlayer")]
    public Vector2 pushDirection = Vector2.left;
    public float pushDistance = 1f;

    [Header("SetAnimatorTrigger")]
    [Tooltip("플레이어 Animator에 넣을 트리거 이름 (예: WakeUp)")]
    public string triggerName;

    [Header("SetActive")]
    public GameObject targetObject;
    public bool setActive = true;

    [Header("SetFlag")]
    public string flagName;

    [Header("AdvanceStory")]
    public int storyAmount = 1;

    [Header("AddAffection")]
    public string npcId;
    public int affectionAmount = 5;
}
