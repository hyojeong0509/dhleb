using UnityEngine;
using System.Collections;

/// <summary>
/// Game 씬 로드 시 저장 데이터 적용
/// Game 씬에 빈 오브젝트로 배치하고 이 스크립트 붙이기
/// </summary>
public class GameSceneInitializer : MonoBehaviour
{
    void Start()
    {
        // GameProgressManager (스토리/호감도/이벤트 플래그)
        if (GameProgressManager.Instance == null)
        {
            var go = new GameObject("GameProgressManager");
            go.AddComponent<GameProgressManager>();
        }

        // DialogueManager (대화 시스템)
        if (DialogueManager.Instance == null)
        {
            var go = new GameObject("DialogueManager");
            go.AddComponent<DialogueManager>();
        }

        // EventManager (조건별 대화/이벤트)
        if (EventManager.Instance == null)
        {
            var go = new GameObject("EventManager");
            go.AddComponent<EventManager>();
        }

        // CutsceneManager (컷신 재생)
        if (CutsceneManager.Instance == null)
        {
            var go = new GameObject("CutsceneManager");
            go.AddComponent<CutsceneManager>();
        }

        // QuestManager (퀘스트)
        if (QuestManager.Instance == null)
        {
            var go = new GameObject("QuestManager");
            go.AddComponent<QuestManager>();
        }

        // DialogueTestTrigger (T키 테스트 - DialogueUI 없으면 Canvas에 추가)
        if (FindObjectOfType<DialogueTestTrigger>() == null)
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                var triggerGo = new GameObject("DialogueTestTrigger");
                triggerGo.transform.SetParent(canvas.transform, false);
                triggerGo.AddComponent<DialogueTestTrigger>();
            }
        }

        // 06:00 시퀀스 매니저 (씬에 없으면 자동 생성)
        if (FindObjectOfType<DayEndSequenceManager>() == null)
        {
            var go = new GameObject("DayEndSequenceManager");
            go.AddComponent<DayEndSequenceManager>();
        }

        // 자연물 매니저 (잔디/나무/돌) - 씬에 없으면 자동 생성 (프리팹은 Inspector에서 할당)
        if (NaturalObjectManager.Instance == null)
        {
            var go = new GameObject("NaturalObjectManager");
            go.AddComponent<NaturalObjectManager>();
        }

        // 플레이어에 FallHandler, ToolAnimator (없으면 추가)
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if (player.GetComponent<PlayerFallHandler>() == null)
                player.AddComponent<PlayerFallHandler>();
            if (player.GetComponent<PlayerToolAnimator>() == null)
                player.AddComponent<PlayerToolAnimator>();
        }

        if (GameDataManager.Instance != null && GameDataManager.Instance.currentSaveData != null)
            GameSaveHelper.ApplyLoadData(GameDataManager.Instance.currentSaveData, player?.transform);

        // 눈 뜨는 컷신 (한 번도 안 봤으면)
        TryPlayWakeUpCutscene();
    }

    void TryPlayWakeUpCutscene()
    {
        if (GameProgressManager.Instance == null || GameProgressManager.Instance.HasFlag("saw_wake_up"))
            return;

        var cutscene = Resources.Load<CutsceneData>("Cutscene/Cutscene_WakeUp");
        if (cutscene == null || CutsceneManager.Instance == null)
            return;

        // 카메라 줌 전에 WakeUp 트리거 → 줌인되기 전부터 애니메이션 시작
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var anim = player.GetComponent<Animator>();
            if (anim != null)
                anim.SetTrigger("WakeUp");
        }

        var cam = Camera.main;
        if (cam != null && cam.orthographic)
        {
            CutsceneManager.WakeUpDefaultOrthoSize = cam.orthographicSize;
            cam.orthographicSize = 2f;
        }

        CutsceneManager.Instance.Play(cutscene);
    }
}
