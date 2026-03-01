using UnityEngine;

/// <summary>
/// 저장/로드 시 게임 상태 수집 및 적용
/// </summary>
public static class GameSaveHelper
{
    /// <summary>
    /// 현재 게임 상태를 data에 채움
    /// </summary>
    public static void CollectSaveData(SaveData data, Transform playerTransform)
    {
        if (data == null) return;

        if (playerTransform != null)
        {
            data.playerData.positionX = playerTransform.position.x;
            data.playerData.positionY = playerTransform.position.y;
        }

        if (TimeManager.Instance != null)
        {
            data.playerData.currentYear = TimeManager.Instance.CurrentYear;
            data.playerData.currentSequence = TimeManager.Instance.CurrentSequence;
            data.playerData.currentDay = TimeManager.Instance.CurrentDay;
            data.playerData.gameMinutes = TimeManager.Instance.CurrentGameMinutes;
        }

        if (InventoryManager.Instance != null)
        {
            if (data.playerData.inventorySlots == null || data.playerData.inventorySlots.Length != 27)
                data.playerData.inventorySlots = new InventorySlotSaveData[27];
            InventoryManager.Instance.FillSaveData(data.playerData.inventorySlots);
        }

        if (StaminaManager.Instance != null)
        {
            data.playerData.currentStamina = StaminaManager.Instance.CurrentStamina;
            data.playerData.wasExhausted = StaminaManager.Instance.IsExhausted;
        }

        if (GoldManager.Instance != null)
            data.playerData.gold = GoldManager.Instance.CurrentGold;

        if (TileManager.Instance != null)
            data.farmData = TileManager.Instance.ExportFarmData();
        if (NaturalObjectManager.Instance != null)
            NaturalObjectManager.Instance.ExportToFarmData(data.farmData);

        // 스토리/호감도/이벤트 플래그
        if (GameProgressManager.Instance != null)
        {
            if (data.gameProgressData == null)
                data.gameProgressData = new GameProgressData();
            GameProgressManager.Instance.FillSaveData(data.gameProgressData);
        }

        // 퀘스트
        if (QuestManager.Instance != null && data.gameProgressData != null)
        {
            if (data.gameProgressData.questData == null)
                data.gameProgressData.questData = new QuestSaveData();
            QuestManager.Instance.FillSaveData(data.gameProgressData.questData);
        }
    }

    /// <summary>
    /// 저장 데이터를 게임에 적용 (로드 시)
    /// </summary>
    public static void ApplyLoadData(SaveData data, Transform playerTransform)
    {
        if (data == null || data.playerData == null) return;

        // 플레이어 위치
        if (playerTransform != null)
        {
            playerTransform.position = new Vector3(
                data.playerData.positionX,
                data.playerData.positionY,
                playerTransform.position.z
            );
        }

        // 시간/날짜
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.LoadState(
                data.playerData.currentYear,
                data.playerData.currentSequence,
                data.playerData.currentDay,
                data.playerData.gameMinutes
            );
        }

        // 인벤토리
        if (InventoryManager.Instance != null && data.playerData.inventorySlots != null)
            InventoryManager.Instance.LoadFromSaveData(data.playerData.inventorySlots);

        // 스테미나 (구버전 세이브는 currentStamina가 0일 수 있음 → 최대치로)
        if (StaminaManager.Instance != null)
        {
            int stamina = data.playerData.currentStamina;
            if (stamina <= 0) stamina = StaminaManager.Instance.MaxStamina;
            StaminaManager.Instance.SetStamina(stamina);
            StaminaManager.Instance.SetWasExhausted(data.playerData.wasExhausted);
        }

        // 골드
        if (GoldManager.Instance != null)
            GoldManager.Instance.SetGold(data.playerData.gold);

        // 농장
        if (TileManager.Instance != null && data.farmData != null)
            TileManager.Instance.LoadFarmData(data.farmData);
        if (NaturalObjectManager.Instance != null && data.farmData != null)
            NaturalObjectManager.Instance.LoadFromFarmData(data.farmData);

        // 스토리/호감도/이벤트 플래그 (구버전 호환: gameProgressData 없으면 기본값)
        if (GameProgressManager.Instance != null)
        {
            var progressData = data.gameProgressData ?? new GameProgressData();
            GameProgressManager.Instance.LoadFromSaveData(progressData);
        }

        // 퀘스트
        if (QuestManager.Instance != null && data.gameProgressData?.questData != null)
            QuestManager.Instance.LoadFromSaveData(data.gameProgressData.questData);

        Debug.Log("[GameSaveHelper] 로드 적용 완료");
    }
}
