using UnityEngine;

/// <summary>
/// Game 씬 로드 시 저장 데이터 적용
/// Game 씬에 빈 오브젝트로 배치하고 이 스크립트 붙이기
/// </summary>
public class GameSceneInitializer : MonoBehaviour
{
    void Start()
    {
        if (GameDataManager.Instance == null || GameDataManager.Instance.currentSaveData == null)
            return;

        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        GameSaveHelper.ApplyLoadData(GameDataManager.Instance.currentSaveData, player);
    }
}
