using UnityEngine;
using System;
using System.IO;

/// <summary>
/// JSON 파일 기반 저장/로드 전담 클래스
/// 저장 위치: Application.persistentDataPath/Saves/Slot_0.json 등
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public const int MAX_SLOTS = 3;
    private const string SAVE_FOLDER = "Saves";
    private const string SAVE_FILE_PREFIX = "Slot_";
    private const string SAVE_FILE_EXT = ".json";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ================================================================
    // 경로 관련
    // ================================================================

    // 저장 폴더 경로
    private string SaveFolderPath => Path.Combine(Application.persistentDataPath, SAVE_FOLDER);

    // 슬롯 인덱스로 파일 경로 반환
    private string GetFilePath(int slotIndex)
    {
        return Path.Combine(SaveFolderPath, $"{SAVE_FILE_PREFIX}{slotIndex}{SAVE_FILE_EXT}");
    }

    // ================================================================
    // 저장
    // ================================================================

    /// <summary>
    /// 특정 슬롯에 SaveData를 JSON으로 저장
    /// </summary>
    public void Save(SaveData data)
    {
        // 폴더 없으면 생성
        if (!Directory.Exists(SaveFolderPath))
            Directory.CreateDirectory(SaveFolderPath);

        // 저장 시각 갱신
        data.lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        string json = JsonUtility.ToJson(data, prettyPrint: true);
        string path = GetFilePath(data.slotIndex);
        File.WriteAllText(path, json);

        Debug.Log($"[SaveManager] 저장 완료 → {path}");
    }

    // ================================================================
    // 로드
    // ================================================================

    /// <summary>
    /// 특정 슬롯의 SaveData 로드. 파일 없으면 null
    /// </summary>
    public SaveData Load(int slotIndex)
    {
        string path = GetFilePath(slotIndex);

        if (!File.Exists(path))
        {
            Debug.Log($"[SaveManager] 슬롯 {slotIndex} 파일 없음");
            return null;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        Debug.Log($"[SaveManager] 로드 완료 ← {path}");
        return data;
    }

    /// <summary>
    /// 전체 슬롯 로드. 비어있는 슬롯은 null
    /// </summary>
    public SaveData[] LoadAllSlots()
    {
        SaveData[] slots = new SaveData[MAX_SLOTS];
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            slots[i] = Load(i);
        }
        return slots;
    }

    // ================================================================
    // 삭제
    // ================================================================

    /// <summary>
    /// 특정 슬롯 파일 삭제
    /// </summary>
    public void Delete(int slotIndex)
    {
        string path = GetFilePath(slotIndex);

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[SaveManager] 슬롯 {slotIndex} 삭제 완료");
        }
        else
        {
            Debug.LogWarning($"[SaveManager] 슬롯 {slotIndex} 파일이 없어 삭제 불가");
        }
    }

    // ================================================================
    // 유틸리티
    // ================================================================

    /// <summary>
    /// 특정 슬롯에 저장 파일이 있는지 확인
    /// </summary>
    public bool SlotExists(int slotIndex)
    {
        return File.Exists(GetFilePath(slotIndex));
    }

    /// <summary>
    /// 비어있는 슬롯 인덱스 반환. 없으면 -1
    /// </summary>
    public int GetEmptySlot()
    {
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            if (!SlotExists(i)) return i;
        }
        return -1; // 빈 슬롯 없음 (꽉 참)
    }

    /// <summary>
    /// 가장 오래된 슬롯 인덱스 반환 (슬롯이 꽉 찼을 때)
    /// </summary>
    public int GetOldestSlot()
    {
        int oldestIndex = 0;
        DateTime oldestTime = DateTime.MaxValue;

        for (int i = 0; i < MAX_SLOTS; i++)
        {
            SaveData data = Load(i);
            if (data == null) return i; // 비어있으면 바로 반환

            if (DateTime.TryParse(data.createdTime, out DateTime created))
            {
                if (created < oldestTime)
                {
                    oldestTime = created;
                    oldestIndex = i;
                }
            }
        }

        return oldestIndex;
    }

    /// <summary>
    /// 플레이 시간을 "X시간 Y분" 형식 문자열로 변환
    /// </summary>
    public static string FormatPlayTime(float minutes)
    {
        int totalMinutes = Mathf.FloorToInt(minutes);
        int hours = totalMinutes / 60;
        int mins  = totalMinutes % 60;

        if (hours > 0)
            return $"{hours}시간 {mins}분";
        else
            return $"{mins}분";
    }
}
