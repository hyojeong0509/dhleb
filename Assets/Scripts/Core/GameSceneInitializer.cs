using UnityEngine;

/// <summary>
/// Game 씬 로드 시 저장 데이터 적용
/// Game 씬에 빈 오브젝트로 배치하고 이 스크립트 붙이기
/// </summary>
public class GameSceneInitializer : MonoBehaviour
{
    void Start()
    {
        // 06:00 시퀀스 매니저 (씬에 없으면 자동 생성)
        if (FindObjectOfType<DayEndSequenceManager>() == null)
        {
            var go = new GameObject("DayEndSequenceManager");
            go.AddComponent<DayEndSequenceManager>();
        }

        // 플레이어에 FallHandler (없으면 추가)
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player.GetComponent<PlayerFallHandler>() == null)
            player.AddComponent<PlayerFallHandler>();

        if (GameDataManager.Instance == null || GameDataManager.Instance.currentSaveData == null)
            return;

        GameSaveHelper.ApplyLoadData(GameDataManager.Instance.currentSaveData, player?.transform);
    }
}
