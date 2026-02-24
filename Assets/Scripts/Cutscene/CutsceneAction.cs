using UnityEngine;
using System.Collections.Generic;

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
    NpcGroupMove,   // NPC 여러 명이 동시에 목표 위치로 걸어감
    NpcGroupReturnToStart, // NpcGroupMove로 온 NPC들을 원래 자리로 돌려보냄
    ShowNotification, // 하단 팝업 표시 (호감도 +5 등)
    AcceptQuest,    // 퀘스트 수락
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
    [Tooltip("밀리는 애니메이션 시간 (0이면 기본 0.3초)")]
    public float pushDuration = 0.3f;
    [Tooltip("밀치는 NPC ID (CutsceneNpcRef, 비우면 애니 없이 밀기만)")]
    public string pushNpcId;
    [Tooltip("밀치는 NPC Animator 트리거 (Any State 전환용, pushNpcStateName보다 우선)")]
    public string pushNpcTrigger;
    [Tooltip("밀치는 NPC Animator 상태 이름 (트리거 대신 직접 재생, Animator에 있는 State 이름 정확히 입력)")]
    public string pushNpcStateName;

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

    [Header("NpcGroupMove")]
    [Tooltip("CutsceneNpcRef의 npcId 목록 (순서대로 targetPositions와 매칭)")]
    public List<string> npcIds = new List<string>();
    [Tooltip("각 NPC가 걸어갈 목표 위치. usePlayerPositionAsOrigin이면 플레이어 위치 + 오프셋")]
    public List<Vector3> npcTargetPositions = new List<Vector3>();
    [Tooltip("true면 목표 위치 = 플레이어 위치 + npcTargetPositions (씨 심을 때 등 플레이어 위치 기준)")]
    public bool usePlayerPositionAsOrigin = false;
    [Tooltip("걷는 데 걸리는 시간")]
    public float npcMoveDuration = 2f;

    [Header("NpcGroupReturnToStart")]
    [Tooltip("돌아갈 NPC ID (NpcGroupMove에서 사용한 것과 동일, 비우면 마지막 NpcGroupMove의 NPC 전원)")]
    public List<string> npcReturnIds = new List<string>();
    [Tooltip("돌아가는 데 걸리는 시간 (0이면 NpcGroupMove와 동일)")]
    public float npcReturnDuration = 0f;
    [Tooltip("복귀 시간 배율 (1=동일, 1.5=1.5배 느리게, 2=2배 느리게)")]
    public float npcReturnDurationMultiplier = 1f;

    [Header("ShowNotification")]
    [Tooltip("팝업에 표시할 텍스트 (예: 호감도 +5)")]
    public string notificationText = "호감도 +5";
    [Tooltip("표시 시간 (0이면 기본 2.5초)")]
    public float notificationDuration = 2.5f;

    [Header("AcceptQuest")]
    [Tooltip("수락할 퀘스트 ID")]
    public string questId;
}
